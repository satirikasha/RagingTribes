namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using System;
  using Engine.Interface;
  using Engine.Pathfinding;

  public class Crowd: IHasPosition {

    public Vector2 Position {
      get {
        return Container.Center;
      }
      set {
        Container.Center = value;
      }
    }

    public Type Type { get; private set; }

    public Ellipse Container { get; private set; }
    public Ellipse DetectionRange { get; private set; }
    public float Velocity { get; private set; }
    public Race Race { get; private set; }
    public Tribe From { get; private set; }

    public List<Troops> Troops { get; private set; }
    public List<Troops> NearEnemies { get; private set; }

    public List<Vector2> CurrentObstacles { get; private set; }

    public bool GoesToMainDestination { get; private set; }

    private IHasPosition _MainDestination; // если null? стоим на месте
    private LinkedList<Vector2> _MainPath;
    private Vector2 _CurrentPathElement;
    private Vector2 _MinorDestination;
    private bool _UninstantiatedLeft = true;

    private Func<object, Vector2, bool> _DetectMainDestinationReached;

    public Crowd(List<Troops> troops, Race race, Vector2 position , Type type) {
      NearEnemies = new List<Units.Troops>();
      Troops = troops;
      Type = type;
      Velocity = GameplaySettings.Velocities.GetValue(Type);
      Race = race;
      Container = new Ellipse(position, Settings.CrowdWidth, Settings.CrowdWidth * Settings.HeightToWidthRelation);
      DetectionRange = new Ellipse(position, Settings.CrowdWidth + GameplaySettings.RangeWidth.GetValue(type), (Settings.CrowdWidth + GameplaySettings.RangeWidth.GetValue(type)) * Settings.HeightToWidthRelation);
      GameManager.Current.AllCrowds.Add(this);
      Troops.ForEach(_ => {
        _.Parent = this;
        _.MainDeltaPos = _.Position - Position;
      });
    }

    private void MoveToMainDestination() {
      if((Position - _CurrentPathElement).magnitude < Velocity) {
        _MainPath.RemoveFirst();
        _CurrentPathElement = _MainPath.First.Value;
      }
      Position += (_CurrentPathElement - Position).normalized * Velocity;
    }

    private void MoveToMinorDestination() {
      if((_MinorDestination - Position).magnitude >= Velocity)
        Position += (_MinorDestination - Position).normalized * Velocity;
    }

    private void UpdateSize() {
      var predictedEllipse = new Ellipse(Position, Container.Width + Settings.CrowdSizeChangeSpeed, Container.Height + Settings.CrowdSizeChangeSpeed * Settings.HeightToWidthRelation);
      var predictedObstacles = GameManager.Current.Pathfinder.ObstacleWorldPositions.Where(_ => predictedEllipse.Contains(_) && !From.Container.Contains(_)).ToList();
      if(predictedObstacles.Count == 0 && Container.Width < Settings.CrowdWidth - Settings.CrowdSizeChangeSpeed) {
        float relativeDelta = (Container.Width + Settings.CrowdSizeChangeSpeed) / Container.Width;
        Troops.ForEach(_ => _.OnParentSizeChanged(new Vector2(relativeDelta, relativeDelta)));
        Container.Width += Settings.CrowdSizeChangeSpeed;
        Container.Height += Settings.CrowdSizeChangeSpeed * Settings.HeightToWidthRelation;
      }
      else {
        CurrentObstacles = predictedObstacles.Where(_ => Container.Contains(_) && !From.Container.Contains(_)).ToList();
        if(CurrentObstacles.Count != 0 && Container.Width > Settings.CrowdSizeChangeSpeed) {
          var relativeDelta = (Container.Width - Settings.CrowdSizeChangeSpeed) / Container.Width;
          Troops.ForEach(_ => _.OnParentSizeChanged(new Vector2(relativeDelta, relativeDelta)));
          Container.Width -= Settings.CrowdSizeChangeSpeed;
          Container.Height -= Settings.CrowdSizeChangeSpeed * Settings.HeightToWidthRelation;
        }
      }
    }

    private void UpdateInstantiate() {
      _UninstantiatedLeft = false;
      Troops.ForEach(_ => {
        if(!_.HasInstance) {
          _UninstantiatedLeft = true;
          if(!From.Container.Contains(_.MainDeltaPos + Position)) {
            _.Position = _.MainDeltaPos + Position;
            _.Instantiate();
          }
        }
      });
    }

    /// <summary>
    /// Проверяет на наличие врагов поблизости и выполняет необходимые действия
    /// </summary>
    private void DetectEnemies() {
      DetectionRange.Center = Position;
      NearEnemies = GameManager.Current.AllCrowds.SelectMany(_ => _.Troops).Where(_ => _.Race != Race && DetectionRange.Contains(_.Position)).ToList();
      if(NearEnemies.Count != 0 && GoesToMainDestination) {
        GoesToMainDestination = false;
        if(GameplaySettings.IsMelee.GetValue(Type)){
          var averageVector = Vector2.zero;
          NearEnemies.ForEach(_ => { averageVector += _.Position; });
          _MinorDestination = averageVector / NearEnemies.Count;
        }
      }
      if(NearEnemies.Count == 0 && !GoesToMainDestination) {
        while(_MainPath.Count >= 2 && (_MainPath.First.Value - Position).magnitude > (_MainPath.First.Next.Value - Position).magnitude) {
            _MainPath.RemoveFirst();
            _CurrentPathElement = _MainPath.First.Value;
        }
        GoesToMainDestination = true;
      }
    }

    /// <summary>
    /// Основной метод отправки войск
    /// </summary>
    public virtual void GoTo<L,D>(L location, D destination, Func<object, Vector2, bool> detectDestinationReached) where D: IHasPosition {
        if(_MainDestination != null)
          throw new Exception("MainDestination можно задать только один раз");
        GoesToMainDestination = true;
        _MainDestination = destination;
        _DetectMainDestinationReached = detectDestinationReached;
        Troops.ForEach(_ => _.FollowCrowdTo(destination));
        From = location as Tribe;
        if(typeof(L) == typeof(Tribe) && typeof(D) == typeof(Tribe))
          _MainPath = GameManager.Current.Pathfinder.Find(location as Tribe, destination as Tribe);
        _CurrentPathElement = _MainPath.First.Value;
    }

    public void ContinueToMainDestination() {
      GoesToMainDestination = true;
    }

    public void Update() {
      if(Troops.Count == 0)
        GameManager.Current.AllCrowds.Remove(this);
      if(_UninstantiatedLeft)
        UpdateInstantiate();
      // Обновляем позиции преград и размеры
      UpdateSize();
      // Передвижение
      if(GoesToMainDestination && _MainDestination != null) {

        MoveToMainDestination();

        if(_DetectMainDestinationReached(_MainDestination, Position)) {
          Troops.ForEach(_ => _.ContinueToMain((obj, point) => { return (obj as Tribe).Container.Contains(point); }));
          GameManager.Current.AllCrowds.Remove(this); // Здесь необходимо удалить все ссылки
        }
      }
      if(!GoesToMainDestination && _MinorDestination != Vector2.zero) {

        MoveToMinorDestination();

      }
      // Атака враждебных юнитов
      {
        DetectEnemies();
      }
    }
  }
}
