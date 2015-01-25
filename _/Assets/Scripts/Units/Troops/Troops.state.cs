namespace RagingTribes.Game.Units {
  using UnityEngine;
  using System.Collections;
  using Engine.Input;
  using Engine.Utils;


  public abstract partial class Troops {
    public enum TroopsState {
      FollowingCrowd,
      MovingToMainDestination,
      MovingToMinorDestination,
      Shooting
    }
  }
}