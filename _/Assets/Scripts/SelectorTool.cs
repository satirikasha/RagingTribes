namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;

  public class SelectorTool: MonoBehaviour {

    private LineRenderer _Line;

    private Tribe _SelectedTribe;

    // Use this for initialization
    void Start() {
      _Line = this.gameObject.AddComponent<LineRenderer>();
      _Line.Construct(_ => {
        _.enabled = false;
        _.SetWidth(Settings.SelectionLineWidth, Settings.SelectionLineWidth);
        _.material = new Material(Shader.Find("Sprites/Default"));
        _.sortingOrder = 1;
      });

      Swipe.OnSwipeMoved += (p, v) => {
        if(_SelectedTribe != null) {
          var point = Camera.main.ScreenToWorldPoint(p).ToVector2();
          if(!_SelectedTribe.Container.Contains(point)) {
            _Line.SetPosition(0, _SelectedTribe.Container.GetRadialPoint(point));
            _Line.SetPosition(1, point);
          }
          else {
            ClearLine();
          }
        }
      };

      Tribe.OnSelected += t => {
        _SelectedTribe = t;
      };
      Tribe.OnDiselected += t => {
        _SelectedTribe = null;
      };
    }

    private void ClearLine() {
      _Line.SetPosition(0, Vector3.zero);
      _Line.SetPosition(1, Vector3.zero);
    }
  }
}