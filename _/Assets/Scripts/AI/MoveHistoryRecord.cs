namespace RagingTribes.Game.AI {
  using UnityEngine;
  using System;
  using System.Collections;
  using System.Collections.Generic;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using RagingTribes.Game.Units;


  public class MoveHistoryRecord {
    public Tribe From        { get; private set; }
    public Tribe To          { get; private set; }
    public float Power       { get; private set; }
    public Type  TroopsType  { get; private set; }

    public MoveHistoryRecord(Tribe from, Tribe to, Crowd crowd) {
      From = from;
      To = to;
      TroopsType = crowd.Type;
      Power = crowd.Troops.Sum(_ => _.DamagePoints * _.HealthPoints);
    }
  }
}