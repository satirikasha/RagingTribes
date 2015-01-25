namespace RagingTribes.Pathfinding {
  using System.Collections.Generic;
  using System.Linq;
  using UnityEngine;
  using Engine.Pathfinding;
  using RagingTribes.Game;
using Engine.Interface;


  public class NodeGrid {
    
    public Node[,] Grid { get; private set; }

    public float XGridToWorld;
    public float YGridToWorld;

    public static NodeGrid GetFromSprite(Texture2D obstacleMap, float cellSize, float mapWidth, float mapHeight){
      var result = new NodeGrid();
      result.Grid = new Node[(int)(mapWidth / cellSize), (int)(mapHeight / cellSize)];
      result.XGridToWorld = Settings.WoldspaceMapWidth / result.Grid.GetLength(0);
      result.YGridToWorld = Settings.WoldspaceMapHeight / result.Grid.GetLength(1);
      var xSizeToPixels = cellSize * (obstacleMap.width / mapWidth);
      var ySizeToPixels = cellSize * (obstacleMap.height / mapHeight);
      int currentX = 0;
      int currentY = 0;
      while(Mathf.CeilToInt((currentX + 1) * xSizeToPixels) <= obstacleMap.width) {
        while(Mathf.CeilToInt((currentY + 1) * ySizeToPixels) <= obstacleMap.height) {
          if(obstacleMap.GetPixel((int)(currentX * xSizeToPixels), obstacleMap.height - (int)(currentY * ySizeToPixels)).r >= 0.9)
            result.Grid[currentX, currentY] = new Node() { X = currentX, Y = currentY, IsObstacle = true };
          else
            result.Grid[currentX, currentY] = new Node() { X = currentX, Y = currentY, IsObstacle = false };
          currentY ++;
        }
        currentY = 0;
        currentX ++;
      }
      return result;
    }

    public void AddObstacle(IHasContainer obstacle) { // Можно оптимизировать!!
      for(int i = 0; i < Grid.GetLength(0); i++) {
        for(int j = 0; j < Grid.GetLength(1); j++) {
          if(obstacle.Container.Contains(GridToWorldPosition(Grid[i, j].ToVector2())))
            Grid[i, j].IsObstacle = true;
        }
      }
    }

    public Vector2 WorldToGridPosition(Vector2 position) {
      position += new Vector2(+Settings.WoldspaceMapWidth / 2, -Settings.WoldspaceMapHeight / 2);
      return new Vector2(Mathf.RoundToInt(position.x / XGridToWorld), Mathf.RoundToInt(-position.y / YGridToWorld)); 
    }

    public Vector2 GridToWorldPosition(Vector2 position) {
      var result = new Vector2(position.x * XGridToWorld, -position.y * YGridToWorld);
      result += new Vector2(-Settings.WoldspaceMapWidth / 2, +Settings.WoldspaceMapHeight / 2);
      return result;
    }

    public void Show(/*Transform parent*/) {
      for(int i = 0; i < Grid.GetLength(0); i++) {
        for(int j = 0; j < Grid.GetLength(1); j++) {
          var dot=Resources.Load<GameObject>("Prefabs/GridDot");
          //dot.transform.parent = parent;
          dot.transform.position = GridToWorldPosition(Grid[i, j].ToVector2());
          dot.GetComponent<SpriteRenderer>().color = Grid[i, j].IsObstacle ? Color.red : Color.green;
          Object.Instantiate(dot);
        }
      }
    }
  }
}
