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
        AddDeltaPositionIfAcсeptable(-Settings.CameraMoveNormalize(v).ToVector3());
      };
      Swipe.OnSwipeEnd += (p, v) => {
        AddLerpCamera(Settings.CameraMoveNormalize(v));
      };
      Swipe.OnSwipeStart += p => {
        _LerpCameraVector = Vector2.zero;
      };
      Resize.OnResizeChanged += q => {
        AddSizeIfAcсeptable(-q);
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
      _LerpCameraTimeLeft = Settings.CameraLerpTime;
    }

    private void LerpCamera() {
      if(_LerpCameraTimeLeft > 0) {
        var fraction = _LerpCameraTimeLeft / Settings.CameraLerpTime;
        AddDeltaPositionIfAcсeptable(-Vector2.Lerp(Vector2.zero, _LerpCameraVector, fraction).ToVector3());
        _LerpCameraTimeLeft -= Time.deltaTime;
      }
      else {
        _LerpCameraTimeLeft = 0;
      }
    }
    private Vector2 _LerpCameraVector = Vector2.zero;
    private float _LerpCameraTimeLeft = 0;

    private void AddDeltaPositionIfAcсeptable(Vector3 value) {
      var sum = Camera.main.transform.position + value;
      if(Mathf.Abs(sum.x) < Settings.BorderX && Mathf.Abs(sum.y) < Settings.BorderY)
        Camera.main.transform.position = sum;
    }

    private void AddSizeIfAcсeptable(float value) {
      var sum = Camera.main.orthographicSize + value;
      if(sum > Settings.MinSize && sum < Settings.MaxSize)
        Camera.main.orthographicSize = sum;
    }
  }
}