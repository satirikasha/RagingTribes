namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;

  public class Cavalier: Troops {
    public static Cavalier_GO Prefab {
      get {
        if(_Prefab == null)
          _Prefab = Resources.Load<Cavalier_GO>(ResourcePaths.Cavalier);
        return _Prefab;
      }
    }
    private static Cavalier_GO _Prefab;

    public Cavalier_GO GameObj;

    public Cavalier(Vector2 position, Race race) {
      IsMelee = true;
      Race = race;
      Range = new Ellipse(Position, GameplaySettings.RangeWidth.GetValue(typeof(Cavalier)), GameplaySettings.RangeWidth.GetValue(typeof(Cavalier)) * Settings.HeightToWidthRelation);
      Position = position;
      Velocity = GameplaySettings.Velocities.GetValue(typeof(Cavalier));
      HealthPoints = GameplaySettings.Health.GetValue(typeof(Cavalier));
      DamagePoints = GameplaySettings.Damage.GetValue(typeof(Cavalier));
    }

    public override void Instantiate() {
      base.Instantiate();
      GameObj = Object.Instantiate(Prefab) as Cavalier_GO;
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