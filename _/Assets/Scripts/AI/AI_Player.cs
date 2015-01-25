namespace RagingTribes.Game.AI {
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using RagingTribes.Game.Units;



  public partial class AI_Player: MonoBehaviour{
    public Race PlayerRace;

    private List<Tribe> _AlliedTribes;
    private List<Tribe> _HostileTribes;

    private List<MoveHistoryRecord> _LastUpdateHistory;

    private List<DelayedAction> _DelayedActions;

    private Strategy _CurrentStrategy;
    private Queue<Situation> _CurrentSituations;

    private float _InteligenceCoeff;

    private float _NextAttackTimeLeft;
    private float _NextReactionTime;

    private float _CurrentDominationIndex;

    void Start() {
      _LastUpdateHistory = new List<MoveHistoryRecord>();
      _AlliedTribes = new List<Tribe>();
      _HostileTribes = new List<Tribe>();
      _CurrentSituations = new Queue<Situation>();
      _DelayedActions = new List<DelayedAction>();
      _CurrentStrategy = Strategy.Expansion;
      _InteligenceCoeff = GameManager.Current.GetCoeffAI(PlayerRace);
      _CurrentDominationIndex = 1.523f;//!!
      SetNextTimeAttack();
      SetNextReactionTime();
      foreach(Tribe t in GameManager.Current.AllTribesOnMap) {
        if(t.Race == PlayerRace)
          _AlliedTribes.Add(t);
        else
          _HostileTribes.Add(t);
      }

      Tribe.OnSendTroops += (from, to, crowd) => {
        if(from.Race != PlayerRace)
          _LastUpdateHistory.Add(new MoveHistoryRecord(from, to, crowd));
      };
      Tribe.OnCaptured += (tribe, oldRace, newRace) => {
        if(oldRace == PlayerRace) {
          _AlliedTribes.Remove(tribe);
          _HostileTribes.Add(tribe);
        }
        if(newRace == PlayerRace) {
          _HostileTribes.Remove(tribe);
          _AlliedTribes.Add(tribe);
        }
      };
    }

    void Update() {
      _NextAttackTimeLeft -= Time.deltaTime;
      _NextReactionTime -= Time.deltaTime;
      UpdateStrategy();
      UpdateDelayedActions();
      switch(_CurrentStrategy) {
        case Strategy.Expansion: {
            DoExpansion();
          } break;
        case Strategy.Stable: {
            DetectSituation();
            ReactOnSituation();
          } break;
        case Strategy.Domination: {
            DoDomination();
          } break;
      }
      ClearHistory();
      if(_CurrentSituations.Count >= 5)//!!
        _CurrentSituations.Dequeue();
    }

    private void UpdateStrategy() {
      var alliedPower = _AlliedTribes.Sum(_ => _.Factory.GetTroopsPower());
      var hostileAveragePower = _HostileTribes.Sum(_ => _.Factory.GetTroopsPower()) / (GameManager.Current.PlayersCount);
      if(_HostileTribes.Count(_ => _.Race == Race.Neutral) > (_HostileTribes.Count / 3)) {
        _CurrentStrategy = Strategy.Expansion;
      }
      else {
        if(alliedPower > hostileAveragePower) {
          _CurrentStrategy = Strategy.Domination;
        }
        else{
          _CurrentStrategy = Strategy.Stable;
        }
      }
    }

    private void UpdateDelayedActions() {
      _DelayedActions.ForEach(_ => {
        _.TimeLeft -= Time.deltaTime;
        if(_.TimeLeft <= 0) {
          _.Action(_.Arg1, _.Arg2, _.Arg3, _.Arg4);
        }
      });
      _DelayedActions.RemoveAll(_ => _.TimeLeft <= 0);
    }

    private void DoExpansion() {
      if(_NextAttackTimeLeft <= 0) {
        Tribe target = null;
        Tribe tribe = null;
        float minVal = float.MaxValue;
        foreach(Tribe t in _AlliedTribes) {
          var FullFactoryTroopsCount = GameplaySettings.HousingRestriction.GetValue(t.HousingLevel) / GameplaySettings.TrainCost.GetValue(t.Factory.GetProductionType());
          if(t.Factory.GetTroopsCount() / FullFactoryTroopsCount > 0.3f)//!!!!
            foreach(Tribe _ in _HostileTribes) {
              var val = (_.Position - t.Position).magnitude * _.Factory.GetTroopsPower() / t.Factory.GetTroopsPower();
              if(minVal > val) {
                minVal = val;
                target = _;
                tribe = t;
              }
            }
        }
        if(tribe != null && target != null)
          tribe.SendTroops(target, tribe.Factory.GetTroopsCount() / 2);

        SetNextTimeAttack();
      }
    }

    private void DoDomination() {
      if(_NextAttackTimeLeft <= 0) {
        Tribe target = null;
        List<Tribe> tribes = new List<Tribe>();
        float maxVal = float.MinValue;
        foreach(Tribe _ in _HostileTribes) {
          var val = _.Factory.GetTroopsPower();
          if(maxVal < val) {
            maxVal = val;
            target = _;
          }
        }
        if(target != null)
          maxVal = maxVal / (1 - GameplaySettings.WallsAdsorbation.GetValue(target.WallsLevel));
        foreach(Tribe t in _AlliedTribes) {
          if(maxVal > 0) {
            var FullFactoryTroopsCount = GameplaySettings.HousingRestriction.GetValue(t.HousingLevel) / GameplaySettings.TrainCost.GetValue(t.Factory.GetProductionType());
            if(t.Factory.GetTroopsCount() / FullFactoryTroopsCount > 0.8f) {//!!
              tribes.Add(t);
              maxVal -= t.Factory.GetTroopsPower() / (1 + _CurrentDominationIndex) ;//!!!!!!!
            }
          }
          else {
            break;
          }
        }
        if(tribes.Count != 0 && target != null) {
          if(maxVal <= 0) {
            tribes.ForEach(_ => _.SendTroops(target, (int)(_.Factory.GetTroopsCount() / (1 + _CurrentDominationIndex))));//!!!!!!!! 
            _CurrentDominationIndex = 1.523f;
          }
          else {
            if(_CurrentDominationIndex == 0)
              StableAtack();
            if(_CurrentDominationIndex - 0.3f > 0)//!!!!!
              _CurrentDominationIndex -= 0.3f;
            else
              _CurrentDominationIndex = 0;
          }
          SetNextTimeAttack();
        }
      }
    }

    private void DetectSituation() {
      if(_LastUpdateHistory.Count > 0) {
        foreach(MoveHistoryRecord r in _LastUpdateHistory) {
          if(r.To.Race == PlayerRace) {
            _CurrentSituations.Enqueue(new Situation() { Source = r, Type = SituationType.EnemyAtack });
          }
          if(HostIsWeak(r)) {
            _CurrentSituations.Enqueue(new Situation() { Source = r, Type = SituationType.HostWeak });
          }
          if(TargetIsWeak(r)) {
            _CurrentSituations.Enqueue(new Situation() { Source = r, Type = SituationType.TargetWeak });
          }
        }
      }
    }

    private void ReactOnSituation() {
      if(_NextReactionTime <= 0) {
        var wasNoReaction = true;
        if(_CurrentSituations.Count != 0) {
          var situation = _CurrentSituations.Dequeue();
          switch(situation.Type) {
            case SituationType.EnemyAtack: ReactOnEnemyAtack(situation.Source); wasNoReaction = false; break;
            case SituationType.HostWeak: ReactOnHostWeak(situation.Source); wasNoReaction = false; break;
            case SituationType.TargetWeak: ReactOnTargetWeak(situation.Source); wasNoReaction = false; break;
          }
        }
        if(wasNoReaction && _NextAttackTimeLeft <= 0) {
          StableAtack();
          SetNextTimeAttack();
        }
        SetNextReactionTime();
      }
    }

    private bool TargetIsWeak(MoveHistoryRecord r) {
      var ToProdType = r.To.Factory.GetProductionType();
      var ToPower = r.To.Factory.GetTroopsCount() * GameplaySettings.Health.GetValue(ToProdType) * GameplaySettings.Damage.GetValue(ToProdType);
      return (ToPower - r.Power) / GameplaySettings.HousingRestriction.GetValue(r.To.HousingLevel) < 0.5f; // !!!!!!!
    }

    private bool HostIsWeak(MoveHistoryRecord r) {
      var FromProdType = r.From.Factory.GetProductionType();
      var FullFactoryTroopsCount = (GameplaySettings.HousingRestriction.GetValue(r.From.HousingLevel) / GameplaySettings.TrainCost.GetValue(FromProdType));
      return r.From.Factory.GetTroopsCount() / FullFactoryTroopsCount < 0.3f;//!!!!!!!
    }

    private void ReactOnEnemyAtack(MoveHistoryRecord r) {
      if(r.To.Race == PlayerRace) {
        var AtackPow = r.Power * (1 - GameplaySettings.WallsAdsorbation.GetValue(r.To.WallsLevel));
        var DefencePow = r.To.Factory.GetTroopsPower();
        if(AtackPow * 0.85f > DefencePow) {//!!!!!!
          if(r.To.Factory.GetProductionType() == typeof(Archer) && r.TroopsType != typeof(Archer)) {
            r.To.SendTroops(r.From, (int)((r.Power / 3) / GameplaySettings.TrainCost.GetValue(typeof(Archer))));//!!!!!!!!!
          }
          if(r.To.Factory.GetProductionType() != typeof(Archer) || r.To.Factory.GetTroopsCount() == 0) {
            var minVal = float.MaxValue;
            Tribe supply = null;
            foreach(Tribe _ in _AlliedTribes) {
              if(_.Factory.GetTroopsCount() / (GameplaySettings.HousingRestriction.GetValue(_.HousingLevel) / GameplaySettings.TrainCost.GetValue(_.Factory.GetProductionType())) > 0.5f) {//!!
                var val = (_.Position - r.To.Position).magnitude;
                if(minVal > val) {
                  minVal = val;
                  supply = _;
                }
              }
            }
            if(supply != null)
              supply.SendTroops(r.To, supply.Factory.GetTroopsCount() / 2);
          }
        }
      }
    }

    private void ReactOnHostWeak(MoveHistoryRecord r) {
      if(r.From.Race != PlayerRace) {
        var minVal = float.MaxValue;
        Tribe from = null;
        foreach(Tribe _ in _AlliedTribes) {
          if(_.Factory.GetTroopsCount() / (GameplaySettings.HousingRestriction.GetValue(_.HousingLevel) / GameplaySettings.TrainCost.GetValue(_.Factory.GetProductionType())) > 0.6f) {//!!
            var val = (_.Position - r.To.Position).magnitude / _.Factory.GetTroopsPower();
            if(minVal > val) {
              minVal = val;
              from = _;
            }
          }
        }
        if(from != null) {
          from.SendTroops(r.From, (int)(from.Factory.GetTroopsCount() * 0.6f));//!!!
        }
      }
    }

    private void ReactOnTargetWeak(MoveHistoryRecord r) {
      if(r.To.Race != PlayerRace) {
        var minVal = float.MaxValue;
        Tribe from = null;
        foreach(Tribe _ in _AlliedTribes) {
          if(_.Factory.GetTroopsCount() / (GameplaySettings.HousingRestriction.GetValue(_.HousingLevel) / GameplaySettings.TrainCost.GetValue(_.Factory.GetProductionType())) > 0.7f) {//!!
            var val = (_.Position - r.To.Position).magnitude / _.Factory.GetTroopsPower();
            if(minVal > val) {
              minVal = val;
              from = _;
            }
          }
        }
        if(from != null) {
          var aliedSpeed = GameplaySettings.Velocities.GetValue(from.Factory.GetProductionType());
          var hostileSpeed = GameplaySettings.Velocities.GetValue(r.TroopsType);
          var predictedTime = (r.From.Position - r.To.Position).magnitude / hostileSpeed - (from.Position - r.To.Position).magnitude / aliedSpeed + 2;//!!!!!
          if(predictedTime > 0)
            _DelayedActions.Add(new DelayedAction() { Action = SendTroopsDelayed, TimeLeft = predictedTime, Arg1 = from, Arg2 = r.To, Arg3 = from.Factory.GetTroopsCount() / 2 });//!!!!
        }
      }
    }

    private void StableAtack() {
      Tribe target = null;
      Tribe tribe = null;
      float minVal = float.MaxValue;
      float maxVal = float.MinValue;
      foreach(Tribe t in _AlliedTribes) {
        var FullFactoryTroopsCount = GameplaySettings.HousingRestriction.GetValue(t.HousingLevel) / GameplaySettings.TrainCost.GetValue(t.Factory.GetProductionType());
        if(t.Factory.GetTroopsCount() / FullFactoryTroopsCount > 0.8f) {//!!!!
          var val = t.Factory.GetTroopsPower();
          if(maxVal < val) {
            maxVal = val;
            tribe = t;
          }
        }
      }
      foreach(Tribe _ in _HostileTribes) {
        var val = _.Factory.GetTroopsPower();
        if(minVal > val) {
          minVal = val;
          target = _;
        }
      }
      if(tribe != null && target != null)
        tribe.SendTroops(target, tribe.Factory.GetTroopsCount() / 3);//!!!!
    }

    private void SendTroopsDelayed(object from, object to, object count, object arg4) {
      (from as Tribe).SendTroops((to as Tribe), (int)count);
    }

    private void ClearHistory() {
      _LastUpdateHistory.Clear();
    }

    private void ClearSituations() {
      _CurrentSituations.Clear();
    }

    private void SetNextTimeAttack() {
      _NextAttackTimeLeft = 5 + (UnityEngine.Random.Range(-1f, 1f) * 3);//!!!
    }

    private void SetNextReactionTime() {
      _NextReactionTime = 1.5f + (UnityEngine.Random.Range(-1f, 1f) * 0.5f);//!!!!!
    }
  }
}