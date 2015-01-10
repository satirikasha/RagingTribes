using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RagingTribes {
  public static class Settings {
    //Map
    public const float MapWidth = 100;
    public const float MapHeight = 60;

    //Camera
    public const float MaxSize = 350;
    public const float MinSize = 100;
    public const float BorderX = 550;
    public const float BorderY = 350;
    public const float CameraLerpTime = 0.8f;
    public const float CameraMoveNormCoef = 230; // нормировочный коэффициент для сдвига камеры
    public static Func<Vector2,Vector2> CameraMoveNormalize = _ => { // нормирующая формула для сдвига камеры
      return _ * (Camera.main.orthographicSize / Settings.CameraMoveNormCoef);
    };
  }
}
