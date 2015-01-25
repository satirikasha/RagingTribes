namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;

  public class Giant : Troops{
    public static Giant_GO Prefab {
      get {
        if(_Prefab == null)
          _Prefab = Resources.Load<Giant_GO>(ResourcePaths.Giant);
        return _Prefab;
      }
    }
    private static Giant_GO _Prefab;

    public Giant_GO GameObj;

    public Giant(Vector2 position, Race race) {
      IsMelee = true;
      Race = race;
      Range = new Ellipse(Position, GameplaySettings.RangeWidth.GetValue(typeof(Giant)), GameplaySettings.RangeWidth.GetValue(typeof(Giant)) * Settings.HeightToWidthRelation);
      Position = position;
      Velocity = GameplaySettings.Velocities.GetValue(typeof(Giant));
      HealthPoints = GameplaySettings.Health.GetValue(typeof(Giant));
      DamagePoints = GameplaySettings.Damage.GetValue(typeof(Giant));
    }

    public override void Instantiate() {
      base.Instantiate();
      GameObj = Object.Instantiate(Prefab) as Giant_GO;
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
