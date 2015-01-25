namespace RagingTribes.Pathfinding {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using Engine.Pathfinding;
  using UnityEngine;

  public class Node: IPathNode<object> {
    public Int32 X { get; set; }
    public Int32 Y { get; set; }
    public Boolean IsObstacle { get; set; }

    public bool IsWalkable(object unused) {
      return !IsObstacle;
    }

    public Vector2 ToVector2() {
      return new Vector2(X, Y);
    }
  }
}
