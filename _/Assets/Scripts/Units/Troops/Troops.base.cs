namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using System;
  using Engine.Interface;
  using Engine.Pathfinding;

  public abstract partial class Troops : IHasPosition {
   
    public static event Action<Troops, IHasPosition> OnMainDestinationReached;
    public static event Action<Troops, IHasPosition> OnMinorDestinationReached;

    public float HealthPoints  { get; set; }
    public Vector2 Position    { get; set; }
    public Crowd Parent        { get; set; }
    public float DamagePoints  { get; protected set; }
    public float Velocity      { get; protected set; }
    public Race Race           { get; protected set; }

    public TroopsState State { get; set; }

    public bool HasInstance { get; private set; }

    public Ellipse Range { get; protected set; }// радиус атаки у Ranged или радиус засечения воина у Melee
    public bool IsMelee { get; protected set; }
    private float _RangedShotCooldown;
    private Troops _RangedTarget;

    private IHasPosition _MinorDestination; // если null? то стоим на месте
    private IHasPosition _MainDestination; // если null? стоим на месте

    public Vector2 MainDeltaPos { get; set; }

    private Func<object, Vector2, bool> _DetectCurrentDestinationReached;
    private Func<object, Vector2, bool> _DetectMainDestinationReached;

    public virtual void Instantiate() {
      HasInstance = true;
    }

    public virtual void Hide() {
      HasInstance = false;
    }

    private void FollowCrowd() {
      Position += (Parent.Position + MainDeltaPos - Position).normalized * Velocity;
    }

    private void MoveToMain() {
      Position += (_MainDestination.Position + MainDeltaPos - Position).normalized * Velocity;
    }

    public void OnParentSizeChanged(Vector2 relativeDelta) {
      MainDeltaPos = Vector2.Scale(MainDeltaPos,relativeDelta);
    }

    /// <summary>
    /// Проверяет на наличие врагов поблизости и выполняет необходимые действия
    /// </summary>
   private void MelleeDetectEnemies() {
     if(Parent != null) {
       if(Parent.NearEnemies.Count > 0 && State != TroopsState.MovingToMinorDestination) {
         var a = UnityEngine.Random.Range(0, Parent.NearEnemies.Count);
         Troops enemy = Parent.NearEnemies.ElementAtOrDefault(a);
         GoToMinor(enemy, (obj, point) => {
           var troops = obj as Troops;
           var meleeRange = new Ellipse(point, Settings.MeleeRangeWidth, Settings.MeleeRangeWidth * Settings.HeightToWidthRelation);
           if(meleeRange.Contains(troops.Position))
             return true;
           if(!Parent.NearEnemies.Contains(troops))
             return true;
           return false;
         });
         OnMinorDestinationReached += OnMinorDestinationReachedMeleeReaction;
       }
     }
    }
    private void OnMinorDestinationReachedMeleeReaction(Troops t, IHasPosition p) {
      if(ReferenceEquals(this, t) && p.GetType().IsSubclassOf(typeof(Troops))) {
        if(Parent.NearEnemies.Contains(p as Troops)) {
          var target = p as Troops;
          Duel(target);
          State = Parent == null ? TroopsState.MovingToMainDestination : TroopsState.FollowingCrowd;
          OnMinorDestinationReached -= OnMinorDestinationReachedMeleeReaction;
        }
        else {
          State = Parent == null ? TroopsState.MovingToMainDestination : TroopsState.FollowingCrowd;
          OnMinorDestinationReached -= OnMinorDestinationReachedMeleeReaction;
        }
      }
   }

   private void RangedDetectEnemies() {
     if(Parent != null) {
       Range.Center = Position;
       Troops enemy = null;
       float minVal = float.MaxValue;
       foreach(Troops _ in Parent.NearEnemies) {
         var val = (_.Position - Position).magnitude;
         if(minVal > val) {
           minVal = val;
           enemy = _;
         }
       }
       if(enemy != null && State != TroopsState.MovingToMinorDestination && State != TroopsState.Shooting) {
         if(!Range.Contains(enemy.Position)) {
           GoToMinor(enemy, (obj, point) => {
             var troops = obj as Troops;
             if(Range.Contains(troops.Position))
               return true;
             if(!Parent.NearEnemies.Contains(troops))
               return true;
             return false;
           });
           OnMinorDestinationReached += OnMinorDestinationReachedRangedReaction;
         }
         else {
           State = TroopsState.Shooting;
           _RangedTarget = enemy;
         }
       }
     }
     if(State == TroopsState.Shooting) {
       if(_RangedTarget != null) {
         if(_RangedShotCooldown <= 0) {
           _RangedTarget.ApplyDamage(DamagePoints);
           if(_RangedTarget.HealthPoints == 0) {//??
             _RangedTarget = null;//??
           }
           _RangedShotCooldown = GameplaySettings.ArcherCooldown;
         }
         else {
           _RangedShotCooldown -= Time.deltaTime;
         }
       }
       else {
         _RangedShotCooldown = 0;
         State = Parent == null ? TroopsState.MovingToMainDestination : TroopsState.FollowingCrowd;
       }
     }
   }
   private void OnMinorDestinationReachedRangedReaction(Troops t, IHasPosition p) {
     if(ReferenceEquals(this, t) && p.GetType().IsSubclassOf(typeof(Troops))) {
       if(Parent.NearEnemies.Contains(p as Troops)) {
         _RangedTarget = p as Troops;
         State = TroopsState.Shooting;
         OnMinorDestinationReached -= OnMinorDestinationReachedRangedReaction;
       }
       else {
         State = Parent == null ? TroopsState.MovingToMainDestination : TroopsState.FollowingCrowd;
         OnMinorDestinationReached -= OnMinorDestinationReachedRangedReaction;
       }
     }
   }
    //private void OnCurrentDestinationReachedReaction(Troops t, IHasPosition p) {
    //  if(ReferenceEquals(this, t) && p.GetType().IsSubclassOf(typeof(Troops))) {
    //    if(Range.Contains(p.Position)) {
    //      var target = p as Troops;
    //      Duel(target);
    //      ContinueToMainDestination();
    //      OnCurrentDestinationReached -= OnCurrentDestinationReachedReaction;
    //    }
    //  }
    //}

    /// <summary>
    ///  Моментальная дуэль, true если дуэль выиграна
    /// </summary>
    public bool Duel(Troops enemy) {
      if(HealthPoints * DamagePoints > enemy.HealthPoints * enemy.DamagePoints) {        
        ApplyDamage(enemy.HealthPoints * enemy.DamagePoints / DamagePoints);
        enemy.Kill();
        return true;
      }
      if(HealthPoints * DamagePoints < enemy.HealthPoints * enemy.DamagePoints) {      
        enemy.ApplyDamage(HealthPoints * DamagePoints / enemy.DamagePoints);
        Kill();
        return false;
      }
      if(HealthPoints * DamagePoints == enemy.HealthPoints * enemy.DamagePoints) {
        Kill();
        enemy.Kill();
        return false;
      }
      throw new Exception("Bad duel result");
    }

    public void ApplyDamage(float value){
      var health = HealthPoints - value;
      if(health <= 0) {
        Kill();
      }
      else {
        HealthPoints = health;
      }
    }

    public void Kill() {
      if(Parent != null)
        Parent.Troops.Remove(this);
      HealthPoints = 0;
      Hide();
    }

    public void Heal() {
      HealthPoints = GameplaySettings.Health.GetValue(this.GetType());
    }

    /// <summary>
    /// Основной метод отправки войск
    /// </summary>
    //public virtual void GoTo<L, T>(L location, T destination, Func<object, Vector2, bool> detectDestinationReached, float mainDeltaX = 0, float mainDeltaY = 0, bool isMainDestination = false)
    //  where T: IHasPosition
    //  where L: IHasPosition {
    //  _MainDeltaPos = new Vector2(mainDeltaX, mainDeltaY);
    //  if(isMainDestination) {
    //    if(_MainDestination != null)
    //      throw new Exception("MainDestination можно задать только один раз");
    //    GoesToMainDestination = true;
    //    _MainDestination = destination;
    //    _DetectMainDestinationReached = detectDestinationReached;
    //    if(typeof(L) == typeof(Tribe) && typeof(T) == typeof(Tribe))//11!!!! ВСЕ ТУТ ПООТМЕНЯТЬ
    //      _MainPath = GameManager.Current.Pathfinder.Find(location as Tribe, destination as Tribe);
    //    _CurrentPathElement = _MainPath.First.Value;
    //    Instantiate();
    //    return;
    //  }
    //  if(!isMainDestination && _MainDestination != null) {
    //    GoesToMainDestination = false;
    //    _MinorDestination = destination;
    //    _DetectCurrentDestinationReached = detectDestinationReached;
    //    return;
    //  }
    //  throw new Exception("Необходимо задать MainDestination перед тем как задавать CurrentDestination");
    //}

    public void FollowCrowdTo(IHasPosition destination) {
      if(_MainDestination != null)
        throw new Exception("MainDestination можно задать только один раз");
      _MainDestination = destination;
      State = Units.Troops.TroopsState.FollowingCrowd;
    }

    public void ContinueToMain(Func<object, Vector2, bool> detectDestinationReached) {
      State = TroopsState.MovingToMainDestination;
      Parent = null; 
      _DetectMainDestinationReached = detectDestinationReached;
    }

    public void GoToMinor(IHasPosition destination, Func<object, Vector2, bool> detectDestinationReached) {
      if(_MainDestination != null) {
        State = TroopsState.MovingToMinorDestination;
        _MinorDestination = destination;
        _DetectCurrentDestinationReached = detectDestinationReached;
        return;
      }
      throw new Exception("Необходимо задать MainDestination перед тем как задавать CurrentDestination");
    }

    public void Update() {
      if(State == TroopsState.MovingToMainDestination && _MainDestination != null) {

        MoveToMain();

        if(_DetectMainDestinationReached(_MainDestination, Position)) {
          OnMainDestinationReached(this, _MainDestination);
          _MainDestination = null;

        }
      }
      if(State == TroopsState.MovingToMinorDestination && _MinorDestination != null) {

        Position += (_MinorDestination.Position - Position).normalized * Velocity;

        if(_DetectCurrentDestinationReached(_MinorDestination, Position)) {
          if(OnMinorDestinationReached != null)
            OnMinorDestinationReached(this, _MinorDestination);
          _MinorDestination = null;
        }
      }
      if(State == TroopsState.FollowingCrowd && Parent != null)
        FollowCrowd();
      // Атака враждебных юнитов
      if(IsMelee) {
        MelleeDetectEnemies();
      }
      else {
        RangedDetectEnemies();
      }
    }
  }
}
