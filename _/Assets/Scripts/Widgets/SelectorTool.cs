namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using RagingTribes.Game.Units;

  public class SelectorTool: MonoBehaviour {

    private LineRenderer _Line;
    private SliderWidget _SliderWidget;

    private Tribe _TargetTribe = null;
    private Tribe[] _AllTribes;

    public Tribe SelectedTribe { get; set; }

    public Tribe TargetTribe {
      get {
        return _TargetTribe;
      }
      private set {
        if(!ReferenceEquals(_TargetTribe, value)) {
          if(_TargetTribe != null) {
            _TargetTribe.HighlightTarget(false);
            _TargetTribe = value;
            _Line.SetColors(Settings.SelectorColor, Settings.SelectorColor);
          }
          if(value != null) {
            value.HighlightTarget(true);
            _TargetTribe = value; 
            _Line.SetColors(Settings.SelectorColor, value.GetRelationColor());
          }
        }
        _TargetTribe = value;
      }
    }

    // Use this for initialization
    void Start() {
      _Line = this.gameObject.AddComponent<LineRenderer>();
      _Line.Construct(_ => {
        _.enabled = false;
        _.SetWidth(Settings.SelectionLineWidth, Settings.SelectionLineWidth);
        _.material = new Material(Shader.Find("Sprites/Default"));
        _.sortingOrder = 1;
      });

      _SliderWidget = Instantiate(Resources.Load<SliderWidget>(ResourcePaths.SliderWidget)) as SliderWidget;
      _SliderWidget.Hide();
      _SliderWidget.transform.parent = this.transform;

      RefreshAllTribes();

      Swipe.OnSwipeMoved += (p, v) => {
        if(SelectedTribe != null) {
          var point = Camera.main.ScreenToWorldPoint(p).ToVector2();
          if(!SelectedTribe.Container.Contains(point)) {
            TargetTribe = FindClosestTribe(point);
            if(TargetTribe != null) {
              var p0 = SelectedTribe.Container.GetRadialPoint(TargetTribe.Container.Center);
              var p1 = TargetTribe.Container.GetRadialPoint(SelectedTribe.Container.Center);
              _Line.SetPosition(0, p0);
              _Line.SetPosition(1, p1);
              var sliderPos = FindClosestLinePosition(point, p0, p1);
              _SliderWidget.SetPosition(sliderPos);
              _SliderWidget.Value = Mathf.RoundToInt(SelectedTribe.Factory.GetTroopsCount() * ((sliderPos - p0).magnitude / (p1 - p0).magnitude));
              _SliderWidget.Show();
            }
            else {
              _SliderWidget.Hide();
              _Line.SetPosition(0, SelectedTribe.Container.GetRadialPoint(point));
              _Line.SetPosition(1, point);
            }
          }
          else {
            ClearLine();
          }
        }
      };
      Swipe.OnSwipeEnd += (p, v) => {
        if(SelectedTribe != null) {
          if(TargetTribe != null) {
            SelectedTribe.SendTroops(TargetTribe, _SliderWidget.Value);
          }
          TargetTribe = null;
          SelectedTribe.Selected = false;
          _SliderWidget.Hide();
        }
      };

      Tribe.OnSelected += t => {
        SelectedTribe = t;
        _Line.enabled = true;
      };
      Tribe.OnDiselected += t => {
        SelectedTribe = null;
        _Line.enabled = false;
        ClearLine();
      };
    }

    private void ClearLine() {
      _Line.SetPosition(0, Vector3.zero);
      _Line.SetPosition(1, Vector3.zero);
    }

    /// <summary>
    /// если в направлении точки есть племена, возвращает ближайшее, инче null 
    /// </summary>
    private Tribe FindClosestTribe(Vector2 point) {
      //основная прямая y = k * x + b
      var k = (point.y - SelectedTribe.Container.Center.y) / (point.x - SelectedTribe.Container.Center.x);
      var b = SelectedTribe.Container.Center.y - k * SelectedTribe.Container.Center.x;

      var tgDeltaAngle = Mathf.Tan(Settings.DeltaAngle);
      //верхняя прямая области погрешности  y = k1 * x + b1
      var k1 = (k + tgDeltaAngle) / (1 - k * tgDeltaAngle);
      var b1 = SelectedTribe.Container.Center.y - k1 * SelectedTribe.Container.Center.x;
      //нижняя прямая области погрешности  y = k2 * x + b2
      var k2 = (k - tgDeltaAngle) / (1 + k * tgDeltaAngle);
      var b2 = SelectedTribe.Container.Center.y - k2 * SelectedTribe.Container.Center.x;

      Tribe[] foundTribes;
      var sourceTribes = _AllTribes.Where(_ => !ReferenceEquals(_,SelectedTribe) && Camera.main.WorldToViewportPoint(_.Container.Center).IsBetweenOneAndZero());
      if(k1 < 0 && k2 > 0) {
        if(point.y > SelectedTribe.Container.Center.y) {
          foundTribes = sourceTribes
            .Where(_ => _.Container.Center.y > k1 * _.Container.Center.x + b1 && _.Container.Center.y > k2 * _.Container.Center.x + b2)
            .ToArray();
        }
        else {
          foundTribes = sourceTribes
            .Where(_ => _.Container.Center.y < k1 * _.Container.Center.x + b1 && _.Container.Center.y < k2 * _.Container.Center.x + b2)
            .ToArray();
        }
      }
      else {
        if(point.x > SelectedTribe.Container.Center.x) {
          foundTribes = sourceTribes
            .Where(_ => _.Container.Center.y < k1 * _.Container.Center.x + b1 && _.Container.Center.y > k2 * _.Container.Center.x + b2)
            .ToArray();
        }
        else {
          foundTribes = sourceTribes
            .Where(_ => _.Container.Center.y > k1 * _.Container.Center.x + b1 && _.Container.Center.y < k2 * _.Container.Center.x + b2)
            .ToArray();
        }
      }

      float minVal = float.MaxValue;
      Tribe minObj = null;
      foreach(Tribe _ in foundTribes) {
        var val = Mathf.Abs(k * _.Container.Center.x - _.Container.Center.y + b) / (k.deg2() + 1).sqr2();
        if(minVal > val) {
          minVal = val;
          minObj = _;
        }
      }
      return minObj;
    }

    private Vector2 FindClosestLinePosition(Vector2 point, Vector2 p0, Vector2 p1) {
      var k = (p0.y - p1.y) / (p0.x - p1.x);
      var b = p0.y - k * p0.x;
      var k_ort = -1 / k;
      var b_ort = point.y - k_ort * point.x;
      var x = (b_ort - b) / (k - k_ort);
      var y = k * x + b;
      Vector2 result = new Vector2(x, y);

      if((result - p0).normalized != (p1 - p0).normalized)
        return p0;

      if((p1 - p0).magnitude < (result - p0).magnitude)
        return p1;
      
      return result;
    }

    private void RefreshAllTribes() {
      _AllTribes = GameObject.FindObjectsOfType<Tribe>();
    }
  }
}