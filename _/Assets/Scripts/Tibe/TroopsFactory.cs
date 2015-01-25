namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;
  using System;
  using System.Linq;
  using System.Collections.Generic;
  using RagingTribes.Game.Units;

  public class TroopsFactory {

    public event Action<Troops[]> OnTroopsAdded;
    public event Action<Troops[]> OnTroopsRemoved;
    public event Action<Troops[]> OnTroopsRequalified;

    private List<Troops> _Troops;
    private Troops _WoundedTroops;
    private float _HealCooldown;

    private Tribe _Parent;

    private float _SecondsPassed;
    private int _TrainPoints;

    private bool _RequalifyMode;

    public TroopsFactory(Tribe parent, int initialTroopsCount) {
      _Troops = new List<Troops>();
      _Parent = parent;
      _SecondsPassed = UnityEngine.Random.Range(0,0.5f);
      _TrainPoints = 0;
      _RequalifyMode = false;

      if(initialTroopsCount > 0) {
        _TrainPoints = initialTroopsCount * GameplaySettings.TrainCost.GetValue(GetProductionType());
      }
    }

    public void Update() {
      if(_Parent.Race != Race.Neutral) {
        EarnBuildPoints();
        HealTroops();
        if(_RequalifyMode) {
          RequalifyTroops();
        }
        else {
          RecruitTroops();
        }
        KillRedundantTroops();
      }
      else {
        RecruitTroops();
      }
    }

    public void StartRequalification() {
      _RequalifyMode = true;
    }

    public Troops[] RequestTroops(int count) {
      if(count >= _Troops.Count) {
        return RemoveTroops();
      }
      if(count > 0)
        return RemoveTroops(count);
      return new Troops[0];
    }

    public void AddTroops(Troops[] troops) {
      foreach(Troops t in troops)
        t.Position = _Parent.Container.GetRandomPoint();
      _Troops.AddRange(troops);
      OnTroopsAdded(troops);
    }

    /// <summary>
    /// Наносит урон и возвращает true, если это убило племя
    /// </summary>
    /// <param name="damage">урон, предварительно уменьшеный стенами</param>
    public bool ApplyDamage(float damage){
      _HealCooldown = Settings.HealCooldown;
      while(damage > 0) {
        var currentDefender = _Troops.FirstOrDefault();
        if(currentDefender == null)
          return true;
        if(currentDefender.HealthPoints * currentDefender.DamagePoints - damage <= 0) {        
          damage -= currentDefender.HealthPoints * currentDefender.DamagePoints;
          RemoveTroops(currentDefender);
          currentDefender = null;
        }
        else {
          currentDefender.HealthPoints -= damage / currentDefender.DamagePoints;
          damage = 0;
          _WoundedTroops = currentDefender;
        }
      }
      return false;
    }

    private void EarnBuildPoints() {
      _SecondsPassed += Time.deltaTime;
      if(_SecondsPassed > GameplaySettings.BarracksProduction.GetValue(_Parent.BarracksLevel)){
        _TrainPoints++;
        _SecondsPassed -= GameplaySettings.BarracksProduction.GetValue(_Parent.BarracksLevel);
      }
    }

    private void RequalifyTroops() {
      var productionType = GetProductionType();
      var nonNativeTroops = _Troops.Where(_ => _.GetType() != productionType);
      if(nonNativeTroops.Count() == 0) {
        _RequalifyMode = false;
        return;
      }
      // Переобучаем
      if(_TrainPoints >= GameplaySettings.TrainCost.GetValue(productionType)) {
        _TrainPoints -= GameplaySettings.TrainCost.GetValue(productionType);
        var soldier = nonNativeTroops.First();
        OnTroopsRequalified(new[] { soldier });
        _Troops.Remove(soldier);
        _Troops.Add(CreateTroopsOfType());
      }
    }

    private void RecruitTroops() {
      var productionType = GetProductionType();
      var trainCost = GameplaySettings.TrainCost.GetValue(productionType);
      var housingRestriction = GameplaySettings.HousingRestriction.GetValue(_Parent.HousingLevel) / trainCost;
      if(_TrainPoints >= trainCost) {
        _TrainPoints -= trainCost;
        float failPossibility = 0;
        var runningOutPoint = housingRestriction * GameplaySettings.RunningOutOfHouses;
        if(_Troops.Count > runningOutPoint) {
          failPossibility = (_Troops.Count - runningOutPoint) / (housingRestriction - runningOutPoint);
        }

        if(!Utils.HasHappened(failPossibility)) {
          var soldier = CreateTroopsOfType();
          OnTroopsAdded(new[] { soldier });
          _Troops.Add(soldier);
        }
      }
    }

    private void KillRedundantTroops() {
      var housingRestrictionAbs = GameplaySettings.HousingRestriction.GetValue(_Parent.HousingLevel);
      var currentCapacity = _Troops.Sum(_ => GameplaySettings.TrainCost.GetValue(_.GetType()));
      float diePossibility = 0;
      var criticalyOutPoint = housingRestrictionAbs * GameplaySettings.CriticalyOutOfHouses;
      if(currentCapacity > housingRestrictionAbs) {
        diePossibility = (currentCapacity - housingRestrictionAbs) / (criticalyOutPoint - housingRestrictionAbs);
      }

      if(Utils.HasHappened(diePossibility)) {
        RemoveTroops(1);
      }
    }

    private void HealTroops(){      
      if(_WoundedTroops != null){
        _HealCooldown -= Time.deltaTime;
        if(_HealCooldown <= 0) {
          _WoundedTroops.Heal();
          _WoundedTroops = null;
        }
      }
      else {
        _HealCooldown = Settings.HealCooldown;
      }
    }

    private Troops[] RemoveTroops(int count) {
      var troops = _Troops.GetRange(0, count).ToArray();
      _Troops.RemoveRange(0, count);
      OnTroopsRemoved(troops);
      return troops;
    }

    private Troops[] RemoveTroops(Troops obj) {
      var troops = new[] { obj };
      _Troops.Remove(obj);
      OnTroopsRemoved(troops);
      return troops;
    }

    private Troops[] RemoveTroops() {
      var troops = _Troops.ToArray();
      _Troops = new List<Troops>();
      OnTroopsRemoved(troops);
      return troops;
    }

    private Troops CreateTroopsOfType() {
      switch(_Parent.ProductionType) {
        case TroopsType.Archer: return new Archer(_Parent.Container.GetRandomPoint(), _Parent.Race);
        case TroopsType.Warrior: return new Warrior(_Parent.Container.GetRandomPoint(), _Parent.Race);
        case TroopsType.Cavalier: return new Cavalier(_Parent.Container.GetRandomPoint(), _Parent.Race);
        case TroopsType.Giant: return new Giant(_Parent.Container.GetRandomPoint(), _Parent.Race);
        default: throw new Exception("wrong ProductionType");
      }
    }

    public Type GetProductionType() {
      switch(_Parent.ProductionType) {
        case TroopsType.Archer: return typeof(Archer);
        case TroopsType.Warrior: return typeof(Warrior);
        case TroopsType.Cavalier: return typeof(Cavalier);
        case TroopsType.Giant: return typeof(Giant);
        default: throw new Exception("wrong ProductionType");
      }
    }

    public int GetTroopsCount() {
      return _Troops.Count;
    }

    public float GetTroopsPower() {
      return _Troops.Sum(_ => _.HealthPoints * _.DamagePoints);
    }
  }
}
