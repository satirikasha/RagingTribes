namespace RagingTribes.Game {
  using UnityEngine;
  using System;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;



  public class Tribe: MonoBehaviour {

    public static event Action<Tribe> OnSelected;
    public static event Action<Tribe> OnDiselected;

    public Ellipse Container { get; private set; }

    //private LineRenderer _Line;
    
    public bool Selected {
      get {
        return _Selection.activeSelf;
      }
      set {
        _Selection.SetActive(value);
        //_Line.enabled = value;
        if(value) {
          OnSelected(this);
        }
        else {
          OnDiselected(this);
          //ClearLine();
        }
      }
    }
    private GameObject _Selection;

    // Use this for initialization
    void Start() {
      _Selection = this.transform.FindChild("Selection").gameObject;

      Container = new Ellipse(this.transform.position.x, this.transform.position.y + Settings.EllipseOffsetY, Settings.TribeWidth, Settings.TribeHeight);

      Swipe.OnSwipeStart += p => {
        if(Container.Contains(Camera.main.ScreenToWorldPoint(p).ToVector2()))
          Selected = true;
      };
      Swipe.OnSwipeEnd += (p, v) => {
        Selected = false;
      };
    }
  }
}