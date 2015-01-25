namespace Engine.Input {
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


  public class InputController {
    public static InputController Current {
      get {
        if(_Current == null)
          _Current = new InputController();
        return _Current;
      }
    }
    private static InputController _Current;

    public Swipe CurrentSwipe { get; private set; }
    public Resize CurrentResize { get; private set; }


    public void UpdateInput() {
      if(Input.touchCount == 1) {
        var touch = Input.GetTouch(0);
        switch(touch.phase) {
          case TouchPhase.Began: { CurrentSwipe = new Swipe(touch); } break;
          case TouchPhase.Moved: if(CurrentSwipe != null) { CurrentSwipe.Update(touch); } break;
          case TouchPhase.Ended: if(CurrentSwipe != null) { CurrentSwipe.Update(touch); CurrentSwipe = null; } break;
        }
      }
      if(Input.touchCount == 2) {
        var touch1 = Input.GetTouch(0);
        var touch2 = Input.GetTouch(1);

        if(touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began) {
          CurrentResize = new Resize(touch1, touch2);
        }

        if(touch1.phase == TouchPhase.Ended || touch2.phase == TouchPhase.Ended) {
          CurrentResize = null;
        }

        if(CurrentResize != null) {
          CurrentResize.Update(touch1, touch2);
        }
      }
      foreach(Touch touch in Input.touches) {
        switch(touch.phase) {
          case TouchPhase.Began: { Tap.Start(touch.position); } break;
          case TouchPhase.Ended: { Tap.End(touch.position); } break;
        }
      }
    }
  }
}
