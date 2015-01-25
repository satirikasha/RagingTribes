namespace RagingTribes.Game {
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using RagingTribes.Game.Units;
  using Engine.Interface;



  public partial class Tribe: MonoBehaviour, IHasPosition, IHasContainer {

    public Vector2 Position {
      get {
        return Container.Center;
      }
    }

    public static event Action<Tribe> OnSelected;
    public static event Action<Tribe> OnDiselected;

    public static event Action<Tribe,Tribe,Crowd> OnSendTroops;
    public static event Action<Tribe,Race,Race> OnCaptured;

    public Ellipse Container { get; private set; }

    public Race Race;

    public Relations Relation { get; set; }
    public TroopsType ProductionType;

    public TroopsFactory Factory { get; private set; }

    public int BarracksLevel;
    public int HousingLevel;
    public int WallsLevel;

    public int InitialTroops;
    
    public bool Selected {
      get {
        return _Selection.activeSelf;
      }
      set {
        if(Relation == Relations.Allied) {
          _Selection.SetActive(value);
          if(value) {
            OnSelected(this);
          }
          else {
            OnDiselected(this);
          }
        }
      }
    }
    private GameObject _Selection;

    // Use this for initialization
    void Awake() {
      _Selection = this.transform.FindChild("Selection").gameObject;
      Container = new Ellipse(this.transform.position.x, this.transform.position.y + Settings.EllipseOffsetY, Settings.TribeWidth, Settings.TribeHeight);
      var a = new TroopsFactory(this, InitialTroops);
      Factory = a;

      Troops.OnMainDestinationReached += (t, p) => {
        if(ReferenceEquals(this,p)){
          if(t.Race == Race) {
            Factory.AddTroops(new[] { t });
            Factory.StartRequalification();
            t.Heal();
            t.Hide();
          }
          else {
            if(Factory.ApplyDamage(t.HealthPoints * t.DamagePoints *(1 - GameplaySettings.WallsAdsorbation.GetValue(WallsLevel)))){
              OnCaptured(this, Race, t.Race);
              Race = t.Race;
              Relation = GameManager.Current.GetRelation(Race);
              Factory.AddTroops(new[] { t });
              Factory.StartRequalification();
              t.Heal();
              t.Hide();
              RefreshTextures();
            }
            else {
              t.Hide();
            }
          }
        }
      };

      Swipe.OnSwipeStart += p => {
        if(Container.Contains(Camera.main.ScreenToWorldPoint(p).ToVector2())) // для того, чтобы не искать по всем племенам в SelectorTool
          Selected = true;
      };
    }

    void Start() {
      RefreshTextures();
    }

    void Update() {
      Factory.Update();
    }

    public void HighlightTarget(bool value) {
      _Selection.GetComponent<SpriteRenderer>().color = value ? GetRelationColor() : Settings.SelectorColor;
      _Selection.SetActive(value);
    }

    public void SendTroops(Tribe target, int count) {
      var troops = Factory.RequestTroops(count);
      Type[] types = troops.Select(_ => _.GetType()).Distinct().ToArray();
      foreach(Type t in types){
        Crowd crowd = new Crowd(troops.Where(_=> _.GetType() == t).ToList(), Race, Position, t);
        crowd.GoTo(this,
        target,
        (obj, point) => {
          return new Ellipse((obj as Tribe).Position,
            Settings.CrowdWidth + Settings.TribeWidth,
            (Settings.CrowdWidth + Settings.TribeWidth) * Settings.HeightToWidthRelation)
            .Contains(point);
        });
        OnSendTroops(this, target, crowd);
      }
    }

    public Color GetRelationColor() {
      switch(Relation) {
        case Relations.Allied: return Settings.AlliedColor;
        case Relations.Neutral: return Settings.NeutralColor;
        case Relations.Hostile: return Settings.HostileColor;
        default: return Settings.SelectorColor;
      }
    }

    private void RefreshTextures() {
      this.transform.FindChild("Wall").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(ResourcePaths.GetWall(Race, WallsLevel));
      this.transform.FindChild("Weaponary").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(ResourcePaths.GetWeaponary(Race, ProductionType));
      this.transform.FindChild("Barrack").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(ResourcePaths.GetBarrack(Race, BarracksLevel));
      this.transform.FindChild("House").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(ResourcePaths.GetHouse(Race, HousingLevel));
      this.transform.FindChild("TribeGround").GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>(ResourcePaths.GetGround(Race));
    }

    IContainer IHasContainer.Container {
      get { return Container; }
    }
  }
}