#if UNITY_ANDROID
namespace Engine.Input {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;
  using Utils;


  public class Swipe {


    public static event Action<Vector2> OnSwipeStart;// координаты нажатия
    public static event Action<Vector2> OnSwipeMoved;// вектор смещения
    public static event Action<Vector2,Vector2> OnSwipeEnd;//координаты конца нажатия и вектор смещения в конце

    public Touch Start { get; private set; }

    private Queue<Touch> _RecentTouchesHistory;

    public Swipe(Touch start) {
      Start = start;
      _RecentTouchesHistory = new Queue<Touch>();
      _RecentTouchesHistory.Enqueue(start);
      OnSwipeStart(start.position);
    }

    public void Update(Touch nextTouch) {
      _RecentTouchesHistory.Enqueue(nextTouch);//записываем следующее значение в историю
      
      {//удаляем слишком старую историю
        var recentTouchesTime = _RecentTouchesHistory.Sum(_ => _.deltaTime); //удаляем слишком старую историю
        while(recentTouchesTime > Settings.RecentTouchesHistoryTimeout) {
          recentTouchesTime -= _RecentTouchesHistory.Dequeue().deltaTime;
        }
      }

      if(nextTouch.phase == TouchPhase.Moved) {
        OnSwipeMoved(nextTouch.deltaPosition);
      }
      if(nextTouch.phase == TouchPhase.Ended) {
        var endVector = _RecentTouchesHistory.Select(_ => _.deltaPosition).AverageVector();
        endVector = endVector.magnitude > Settings.MinSwipeEndVectorLength ? endVector : Vector2.zero;
        OnSwipeEnd(nextTouch.position, endVector);
      }
    }
  }
}
#endif