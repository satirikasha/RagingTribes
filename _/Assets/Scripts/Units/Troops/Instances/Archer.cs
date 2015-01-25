namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;

  public class Archer: Troops {
    public static Archer_GO Prefab {
      get {
        if(_Prefab == null)
          _Prefab = Resources.Load<Archer_GO>(ResourcePaths.Archer);
        return _Prefab;
      }
    }
    private static Archer_GO _Prefab;

    public Archer_GO GameObj;

    public Archer(Vector2 position, Race race) {
      IsMelee = false;
      Race = race;
      Range = new Ellipse(Position, GameplaySettings.RangeWidth.GetValue(typeof(Archer)), GameplaySettings.RangeWidth.GetValue(typeof(Archer)) * Settings.HeightToWidthRelation);
      Position = position;
      Velocity = GameplaySettings.Velocities.GetValue(typeof(Archer));
      HealthPoints = GameplaySettings.Health.GetValue(typeof(Archer));
      DamagePoints = GameplaySettings.Damage.GetValue(typeof(Archer));
      //Collider = new Ellipse(Position, Settings.ColliderWidth, Settings.ColliderWidth * Settings.HeightToWidthRelation);
    }

    public override void Instantiate() {
      base.Instantiate();
      GameObj = Object.Instantiate(Prefab) as Archer_GO;
      GameObj.Entity = this;
      GameObj.transform.position = Position;
   
    }

    public override void Hide() {
      base.Hide();
      if(GameObj != null)
        Object.Destroy(GameObj.gameObject);
    }
  }
}
