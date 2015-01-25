namespace Engine.Input {
  using System;
  using System.Collections.Generic;
  using System.Linq;
  using System.Text;
  using UnityEngine;
  using Utils;


  public class Resize {

    public static event Action<float> OnResizeChanged;// +/- значение изменения

    public Touch Touch1 { get; private set; }
    public Touch Touch2 { get; private set; }

    public Resize(Touch touch1, Touch touch2) {
      Touch1 = touch1;
      Touch2 = touch2;
    }

    public void Update(Touch touch1, Touch touch2) {

      var preMagnitude = (Touch1.position - Touch2.position).magnitude;
      var newMagnitude = (touch1.position - touch2.position).magnitude;

      OnResizeChanged(newMagnitude - preMagnitude);

      Touch1 = touch1;
      Touch2 = touch2;
    }
  }
}