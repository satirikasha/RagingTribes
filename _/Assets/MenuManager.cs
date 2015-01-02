﻿namespace RagingTribes.Menu {
  using UnityEngine;
  using System.Collections;

  public class MenuManager: MonoBehaviour {

    private GUITexture _Background;

    // Use this for initialization
    void Start() {
      Load();   
    }

    // Update is called once per frame
    void Update() {

    }

    private void Load() {
      _Background = this.gameObject.AddComponent<GUITexture>();
      _Background.texture = Resources.Load<Texture>(ResourcePaths.Background);
      _Background.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
      _Background.enabled = true;
    }

    public void Play() {
      Application.LoadLevelAsync("Game");
    }
  }
}