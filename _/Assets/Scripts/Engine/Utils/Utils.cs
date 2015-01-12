using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Engine.Utils {
  public static class Utils {
    public static T Construct<T>(this T obj, Action<T> init) where T:Component{
      init(obj);
      return obj;
    }

    public static Vector2 ToVector2(this Vector3 obj) {
      return new Vector2(obj.x, obj.y);
    }

    public static Vector3 ToVector3(this Vector2 obj) {
      return new Vector3(obj.x, obj.y, 0);
    }

    /// <summary>
    /// Среднеарифметический вектор
    /// </summary>
    public static Vector2 AverageVector(this IEnumerable<Vector2> obj) {
      var n = obj.Count();
      return new Vector2(obj.Sum(_ => _.x)/n, obj.Sum(_ => _.y)/n);
    }

    /// <summary>
    /// Возведение в квадрат
    /// </summary>
    public static float deg2(this float obj){
      return Mathf.Pow(obj, 2);
    }

    /// <summary>
    /// Квадратный корень
    /// </summary>
    public static float sqr2(this float obj) {
      return Mathf.Pow(obj, 0.5f);
    }
  }
}
