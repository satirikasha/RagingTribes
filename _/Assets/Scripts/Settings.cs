using Engine.Utils;////////////////////qqqqqqqqqqqqqqqqqqqqqqq
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RagingTribes {
  public static class Settings {
    //Game
    public const float HeightToWidthRelation = 0.5f; // Основное отношение полуосей эллипсов в игре
    
    //Map
    public const float MapWidth = 150;
    public const float MapHeight = 85;
    public const float WoldspaceMapHeight = 866;
    public const float WoldspaceMapWidth = 1536/*999999*/;

    //Camera
    public const float PreferableSize = 300;
    public const float MaxSize = 350;
    public const float MinSize = 100;
    public const float BorderX = 550;
    public const float BorderY = 350;
    public const float CameraLerpTime = 0.5f;
    public const float CameraMoveNormCoef = 230; // нормировочный коэффициент для сдвига камеры
    public static Func<Vector2,Vector2> CameraMoveNormalize = _ => { // нормирующая формула для сдвига камеры
      return _ * (Camera.main.orthographicSize / Settings.CameraMoveNormCoef);
    };

    //Tribe
    public const float TribeWidth = 143;
    public const float TribeHeight = 72;
    public const float EllipseOffsetY = -5;
    public const float HealCooldown = 3;
    public static Color HostileColor = Color.red;
    public static Color NeutralColor = Color.gray;
    public static Color AlliedColor  = Color.green;
    public static Color SelectorColor = Color.white;

    //SelectorTool
    public const float SelectionLineWidth = 4;
    public const float DeltaAngle = 7 * Mathf.Deg2Rad;

    //Units
    public const float MeleeRangeWidth = 30;
    public const float CrowdWidth = 150;
    public const float CrowdSizeChangeSpeed = 5;
  }
}
