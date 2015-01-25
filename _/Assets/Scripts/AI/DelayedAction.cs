using System;
namespace RagingTribes.Game.AI {


  public class DelayedAction {
    public Action<object,object,object,object> Action;
    public float TimeLeft;
    public object Arg1;
    public object Arg2;
    public object Arg3;
    public object Arg4;
  }
}