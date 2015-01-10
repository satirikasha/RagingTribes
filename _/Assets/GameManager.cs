namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;

  public class GameManager: MonoBehaviour {

    private GameObject Enviroment;

    // Use this for initialization
    void Start() {
      PresetCamera();
      LoadEnviroment();
    }

    // Update is called once per frame
    void Update() {
      InputController.Current.UpdateInput();
      UpdateCamera();
    }

    private void PresetCamera() {
      Swipe.OnSwipeMoved += v => {
        Camera.main.transform.position -= new Vector3(v.x, v.y);
      };
      Swipe.OnSwipeEnd += (p, v) => {
        AddLerpCamera(v);
      };
      Swipe.OnSwipeStart += p => {
        _LerpCameraVector = Vector2.zero;
      };
      Resize.OnResizeChanged += q => {
        Camera.main.orthographicSize -= q;
      };
    }

    private void LoadEnviroment() {
      Enviroment = new GameObject("Enviroment");
      Enviroment.transform.localScale = new Vector3(Settings.MapWidth, Settings.MapHeight);
      var background = Resources.Load<Sprite>(ResourcePaths.Background);
      Enviroment.AddComponent<SpriteRenderer>().sprite = background;
    }

    private void UpdateCamera() {
      LerpCamera();
    }

    private void AddLerpCamera(Vector2 v) {
      _LerpCameraVector += v;
      _LerpCameraTimeLeft = Settings.LerpTime;
    }

    private void LerpCamera() {
      if(_LerpCameraTimeLeft > 0) {
        var fraction = _LerpCameraTimeLeft / Settings.LerpTime;
        Camera.main.transform.position -= Vector2.Lerp(Vector2.zero, _LerpCameraVector, fraction).ToVector3();
        _LerpCameraTimeLeft -= Time.deltaTime;
      }
      else {
        _LerpCameraTimeLeft = 0;
      }
    }
    private Vector2 _LerpCameraVector = Vector2.zero;
    private float _LerpCameraTimeLeft = 0;
  }
}