﻿namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;
  using System;

  public class Archer_GO : MonoBehaviour {

    public Archer Entity;

    void Update() {
      Entity.Update();
      this.transform.position = Entity.Position;
    }
  }
}