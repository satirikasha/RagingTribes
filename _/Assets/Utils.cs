using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RagingTribes {
  public static class Utils {
    public static T Construct<T>(this T obj, Action<T> init) where T:Component{
      init(obj);
      return obj;
    }
  }
}
