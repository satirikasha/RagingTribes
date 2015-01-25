namespace Engine.Input {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;
  using Utils;


  public static class Tap {
    public static event Action<Vector2> OnTapStart;// координаты нажатия
    public static event Action<Vector2> OnTapEnd;//координаты конца нажатия

    public static void Start(Vector2 position) {
      try {
        OnTapStart(position);
      }
      catch { }
    }

    public static void End(Vector2 position) {
      try {
        OnTapEnd(position);
      }
      catch { }
    }
  }
}
