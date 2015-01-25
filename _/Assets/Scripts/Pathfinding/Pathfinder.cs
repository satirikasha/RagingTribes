namespace RagingTribes.Pathfinding {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Threading;
  using Engine.Pathfinding;
  using Engine.Utils;
  using UnityEngine;
  using RagingTribes.Game;

  public class Pathfinder {
    public Dictionary<Vector4, Vector2> CrowdStartPositionCache { get; set; }
    public Dictionary<Vector4, LinkedList<Node>> CrowdPathCache { get; set; }

    public NodeGrid Grid { get; private set; }
    public List<Vector2> ObstacleWorldPositions { get; private set; }
    private SpatialAStar<Node,object> Solver;

    public Pathfinder(NodeGrid grid, Tribe[] tribes) {
      Grid = grid;
      CrowdStartPositionCache = new Dictionary<Vector4, Vector2>();
      CrowdPathCache = new Dictionary<Vector4,LinkedList<Node>>();
      Solver = new SpatialAStar<Node, object>(grid.Grid);
      foreach(Tribe start in tribes) {
        foreach(Tribe end in tribes) {
          if(!ReferenceEquals(start, end)) {
            var startPos = Grid.WorldToGridPosition(start.Position);
            var endPos = Grid.WorldToGridPosition(end.Position);
            var path = Solver.Search(startPos, endPos, null);
            while(start.Container.Contains(Grid.GridToWorldPosition(path.First.Value.ToVector2()))) {
              path.RemoveFirst();
            }
            path.RemoveFirst(); // запасной
            CrowdStartPositionCache.Add(Utils.Combine(startPos, endPos), path.First.Value.ToVector2());
          }
        }
      }
      foreach(Tribe t in tribes)
        Grid.AddObstacle(t);
      Solver = new SpatialAStar<Node, object>(Grid.Grid);
      foreach(Vector4 startEnd in CrowdStartPositionCache.Keys) {
        var startPos = CrowdStartPositionCache.GetValue(startEnd);
        var endPos = CrowdStartPositionCache.GetValue(new Vector4(startEnd.z, startEnd.w,startEnd.x, startEnd.y));
        var path = PostProcessPath(Solver.Search(startPos, endPos, null));
        CrowdPathCache.Add(startEnd, path);
      }
      RefreshObstacleWorldPositions();
    }

    public LinkedList<Vector2> Find(Tribe position, Tribe target) {
      var result = new LinkedList<Vector2>();
      var startEnd = Utils.Combine(Grid.WorldToGridPosition(position.Position), Grid.WorldToGridPosition(target.Position));
      var path = new LinkedList<Node>();
      if(!CrowdPathCache.TryGetValue(startEnd, out path)) {
        path = PostProcessPath(Solver.Search(Grid.WorldToGridPosition(position.Position), Grid.WorldToGridPosition(target.Position), null));
        CrowdPathCache.Add(startEnd, path);
      }
      var resultPath = new LinkedList<Node>(path);//чтобы дальнейшие преобразования не влияли на Cache
      resultPath.RemoveFirst();
      resultPath.RemoveLast();
      var n = resultPath.Count;
      for(int i = 0; i < n; i++) {
        result.AddLast(Grid.GridToWorldPosition(resultPath.First.Value.ToVector2()));
        resultPath.RemoveFirst();
      }
      result.AddLast(target.Position);
      return result;
    }

    public void RefreshObstacleWorldPositions(){
      ObstacleWorldPositions = new List<Vector2>();
      for(int i = 0; i < Grid.Grid.GetLength(0); i++) {
        for(int j = 0; j < Grid.Grid.GetLength(1); j++) {
          if(Grid.Grid[i, j].IsObstacle)
            ObstacleWorldPositions.Add(Grid.GridToWorldPosition(Grid.Grid[i, j].ToVector2()));
        }
      }
    }

    public LinkedList<Node> PostProcessPath(LinkedList<Node> rawPath) {
      var currentNode = rawPath.First.Next;
      while(currentNode.Next != null){
        var t = Grid.Grid.GetValueOrDefault(currentNode.Value.X, currentNode.Value.Y + 1);
        var r = Grid.Grid.GetValueOrDefault(currentNode.Value.X + 1, currentNode.Value.Y);
        var b = Grid.Grid.GetValueOrDefault(currentNode.Value.X, currentNode.Value.Y - 1);
        var l = Grid.Grid.GetValueOrDefault(currentNode.Value.X - 1, currentNode.Value.Y);
        var tl = Grid.Grid.GetValueOrDefault(currentNode.Value.X - 1, currentNode.Value.Y + 1);
        var tr = Grid.Grid.GetValueOrDefault(currentNode.Value.X + 1, currentNode.Value.Y + 1);
        var br = Grid.Grid.GetValueOrDefault(currentNode.Value.X + 1, currentNode.Value.Y - 1);
        var bl = Grid.Grid.GetValueOrDefault(currentNode.Value.X - 1, currentNode.Value.Y - 1);
        if((t == null || !t.IsObstacle) &&
          (r == null || !r.IsObstacle) &&
          (b == null || !b.IsObstacle) &&
          (l == null || !l.IsObstacle) &&
          (tl == null || !tl.IsObstacle) &&
          (tr == null || !tr.IsObstacle) &&
          (br == null || !br.IsObstacle) &&
          (bl == null || !br.IsObstacle)) {
            var node = currentNode.Next;
            rawPath.Remove(currentNode);
            currentNode = node;
        }
        else {
          currentNode = currentNode.Next;
        }
      }
      return rawPath;
    }
  }
}
