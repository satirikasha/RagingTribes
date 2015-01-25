namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;

  public class Warrior: Troops {
    public static Warrior_GO Prefab {
      get {
        if(_Prefab == null)
          _Prefab = Resources.Load<Warrior_GO>(ResourcePaths.Warrior);
        return _Prefab;
      }
    }
    private static Warrior_GO _Prefab;

    public Warrior_GO GameObj;
    public Vector2 DirectionDelta;

    public Warrior(Vector2 position, Race race) {
      IsMelee = true;
      Race = race;
      Range = new Ellipse(Position, GameplaySettings.RangeWidth.GetValue(typeof(Warrior)), GameplaySettings.RangeWidth.GetValue(typeof(Warrior)) * Settings.HeightToWidthRelation);
      Position = position;
      Velocity = GameplaySettings.Velocities.GetValue(typeof(Warrior));
      HealthPoints = GameplaySettings.Health.GetValue(typeof(Warrior));
      DamagePoints = GameplaySettings.Damage.GetValue(typeof(Warrior));
      //Collider = new Ellipse(Position, Settings.ColliderWidth, Settings.ColliderWidth * Settings.HeightToWidthRelation);
    }

    public override void Instantiate() {
      base.Instantiate();
      GameObj = Object.Instantiate(Prefab) as Warrior_GO;
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
