namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;

  public class SliderWidget: MonoBehaviour {

    private TextMesh _Counter;

    public int Value{
      get{
        return _Value;
      }
      set{
        _Counter.text = value.ToString();
        _Value = value;
      }
    }
    private int _Value;

    // Use this for initialization
    void OnEnable() {
      if(_Counter == null)
        _Counter = this.gameObject.GetComponentInChildren<TextMesh>();
    }

    public void Show() {
      this.gameObject.SetActive(true);
    }

    public void Hide() {
      this.gameObject.SetActive(false);
    }

    public void SetPosition(Vector2 position) {
      this.transform.position = position;
    }
  }
}
