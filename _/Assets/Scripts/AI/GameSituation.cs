namespace RagingTribes.Game.AI {


  public enum SituationType {
    EnemyAtack,
    TargetWeak,
    HostWeak,
  }

  public class Situation {
    public SituationType Type;
    public MoveHistoryRecord Source;
  }
}