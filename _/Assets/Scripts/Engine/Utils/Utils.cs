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

    /// <summary>
    /// Принадлежит от 0 до 1
    /// </summary>
    public static bool IsBetweenOneAndZero(this float value) {
      return value >= 0 && value <= 1;
    }

    /// <summary>
    /// X и Y принадлежат от 0 до 1
    /// </summary>
    public static bool IsBetweenOneAndZero(this Vector2 value) {
      return IsBetweenOneAndZero(value.x) && IsBetweenOneAndZero(value.y);
    }

    /// <summary>
    /// X и Y принадлежат от 0 до 1
    /// </summary>
    public static bool IsBetweenOneAndZero(this Vector3 value) {
      return IsBetweenOneAndZero(value.ToVector2());
    }
 
    public static bool HasHappened(float value){
      if(!value.IsBetweenOneAndZero()) {
        return value < 0 ? false : true;
      }
      else {
        return value >= UnityEngine.Random.Range(0.00001f, 1) ? true : false;
      }
    }

    public static TValue GetValue<TKey,TValue>(this Dictionary<TKey,TValue> dictionary, TKey key){
      TValue value;
      if(dictionary.TryGetValue(key, out value))
        return value;
      throw new Exception("Value not found");
    }

    public static TValue GetValueOrDefault<TValue>(this TValue[,] array, int i, int j) {
      return (i < array.GetLength(0) && i >= 0 && j < array.GetLength(1) && j >= 0) ? array[i, j] : default(TValue); 
    }

    public static Vector4 Combine(Vector2 v1, Vector2 v2){
      return new Vector4(v1.x, v1.y, v2.x, v2.y);
    }
  }
}
