using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Engine {
  public static class Settings {
    //Input
    public const float RecentTouchesHistoryTimeout = 0.1f; // за склолько последних секунд хранить данные ввода
    public const float MinSwipeEndVectorLength = 2.5f; // минимальная дляна вектора при которой считается что Swipe имеет конечный вектор 
  }
}
