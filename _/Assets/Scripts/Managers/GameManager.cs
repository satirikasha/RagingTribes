namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using RagingTribes.Game.Units;
  using System.Collections.Generic;
  using RagingTribes.Pathfinding;
  using Engine.Pathfinding;
  using RagingTribes.Game.AI;
  using System;

  public class GameManager: MonoBehaviour {

    public static GameManager Current {
      get {
        if(_Current == null)
          _Current = GameObject.FindObjectOfType<GameManager>();
        return _Current;
      }
    }
    private static GameManager _Current;

    public Race PlayerRace;

    public int PlayersCount { get; private set; }

    [Range(0f,1f)]
    public float VikingAI;
    [Range(0f, 1f)]
    public float IndianAI;
    [Range(0f, 1f)]
    public float EvilAI;

    public bool ShowPathfindingGrid;

    public GameObject Enviroment { get; private set; }

    public GameObject SelectorTool { get; private set; }

    public bool CameraControlEnabled { get; private set; }
    private int _CameraBusinesCounter = 0;

    public Tribe[] AllTribesOnMap { get; private set; }

    public List<Crowd> AllCrowds { get; set; }

    public Pathfinder Pathfinder { get; private set; }
    public NodeGrid PathfindingGrid { get; private set; }
       
    // Use this for initialization
    void Start() {
      PresetCamera();
      LoadEnviroment();
      LoadSelectorTool();
      RefreshAllTribesOnMap();
      PresetTribes();
      PresetCrowds();
      PresetPathfinding();
      LoadAI();
    }

    // Update is called once per frame
    void Update() {
      InputController.Current.UpdateInput();
      UpdateCamera();
      UpdateCrowds();
    }

    private void PresetCamera() {
      CameraControlEnabled = true;

      Tribe.OnSelected += t => {
        if(_CameraBusinesCounter == 0) {
          _CameraBusinesCounter++;
          CameraControlEnabled = false;
        }
      };
      Tribe.OnDiselected += t => {
        if(_CameraBusinesCounter > 0) {
          CameraControlEnabled = (--_CameraBusinesCounter == 0);
        }
      };

      Swipe.OnSwipeMoved += (p, v) => {
        if(CameraControlEnabled)
          AddDeltaPositionIfAcсeptable(-Settings.CameraMoveNormalize(v).ToVector3());
      };
      Swipe.OnSwipeEnd += (p, v) => {
        if(CameraControlEnabled)
          AddLerpCamera(Settings.CameraMoveNormalize(v));
      };
      Swipe.OnSwipeStart += p => {
        _LerpCameraVector = Vector2.zero;
      };
      Resize.OnResizeChanged += q => {
        if(CameraControlEnabled)
          AddSizeIfAcсeptable(-q);
      };
    }

    private void PresetTribes() {
      foreach(Tribe t in AllTribesOnMap) {
        t.Relation = GetRelation(t.Race);
      }
    }

    private void PresetCrowds() {
      AllCrowds = new List<Crowd>();
    }

    private void LoadEnviroment() {
      Enviroment = new GameObject("Enviroment");
      Enviroment.transform.localScale = new Vector3(Settings.MapWidth, Settings.MapHeight);
      var background = Resources.Load<Sprite>(ResourcePaths.Background);
      var spriteRenderer = Enviroment.AddComponent<SpriteRenderer>();
      spriteRenderer.sprite = background;
      spriteRenderer.sortingOrder = -300;
      Enviroment.isStatic = true;
    }

    private void LoadSelectorTool() {
      SelectorTool = new GameObject("SelectorTool");
      SelectorTool.AddComponent<SelectorTool>();
    }

    private void LoadAI() {
      PlayersCount = 0;
      foreach(Race r in AllTribesOnMap.Where(_ => _.Race != PlayerRace && _.Race != Race.Neutral).Select(_ => _.Race).Distinct()) {
        new GameObject(r.ToString() + "_AI").AddComponent<AI_Player>().PlayerRace = r;
        PlayersCount++;
      }
    }

    private void PresetPathfinding() {
      var map = Resources.Load<Sprite>("Sprites/Game/PathFindingTestMap");
      PathfindingGrid = NodeGrid.GetFromSprite(map.texture, 2f, Settings.MapWidth, Settings.MapHeight);
      Pathfinder = new Pathfinder(PathfindingGrid, AllTribesOnMap);
      if(ShowPathfindingGrid)
        Pathfinder.Grid.Show();
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

    public Tribe.Relations GetRelation(Race race) {
      if(race == PlayerRace) {
        return Tribe.Relations.Allied;
      }
      else {
        if(race == Race.Neutral) {
          return Tribe.Relations.Neutral;
        }
        else {
          return Tribe.Relations.Hostile;
        }
      }
    }

    private void RefreshAllTribesOnMap() {
      AllTribesOnMap = GameObject.FindObjectsOfType<Tribe>();
    }

    private void UpdateCrowds() {
      AllCrowds.ForEach(_ => _.Update());
    }

    public float GetCoeffAI(Race race) {
      switch(race) {
        case Race.Viking : return VikingAI;
        case Race.Indian : return IndianAI;
        case Race.Evil   : return EvilAI;
        default: throw new Exception("Bad Race");
      }
    }

    public void Print(object obj) {
      print(obj);
    }
  }
}