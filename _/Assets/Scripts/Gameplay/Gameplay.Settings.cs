using RagingTribes.Game.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace RagingTribes.Game {
  public static class GameplaySettings {
    //Tribe production
    public static Dictionary<int,float> BarracksProduction = new Dictionary<int, float>() { 
      {0, 1.5f},
      {1, 1.3f},
      {2, 1.1f},
      {3, 0.9f},
      {4, 0.7f},
      {5, 0.5f}
    };

    //Tribe capacity restriction
    public static Dictionary<int,float> HousingRestriction = new Dictionary<int, float>() { 
      {0, 30 },
      {1, 50 },
      {2, 80 },
      {3, 120},
      {4, 240},
      {5, 500}
    };

    //Tribe damage adsorbation
    public static Dictionary<int,float> WallsAdsorbation = new Dictionary<int, float>() { 
      {0, 0   },
      {1, 0.2f},
      {2, 0.4f},
      {3, 0.6f},
      {4, 0.7f},
      {5, 0.8f}
    };

    //Troops train cost
    public static Dictionary<Type,int> TrainCost = new Dictionary<Type, int>() { 
      {typeof(Archer),   1},
      {typeof(Warrior),  1},
      {typeof(Cavalier), 3},
      {typeof(Giant),    5}
    };

    //После этого значения Troops.Count/HousingRestriction появляется вероятность не получить нового воина
    public const float RunningOutOfHouses = 0.8f;
    //После этого значения Troops.Count/HousingRestriction вероятность смерти воина становится 1
    public const float CriticalyOutOfHouses = 5f;

    //Troops velocities
    public static Dictionary<Type,float> Velocities = new Dictionary<Type, float>() { 
      {typeof(Archer),   1f  },
      {typeof(Warrior),  1f  },
      {typeof(Cavalier), 2f  },
      {typeof(Giant),    0.5f}
    };

    //Troops HP
    public static Dictionary<Type,float> Health = new Dictionary<Type, float>() { 
      {typeof(Archer),   1.5f},
      {typeof(Warrior),  2f  },
      {typeof(Cavalier), 2f  },
      {typeof(Giant),    4f }
    };

    //Troops Damage
    public static Dictionary<Type,float> Damage = new Dictionary<Type, float>() { 
      {typeof(Archer),   0.5f},
      {typeof(Warrior),  1f  },
      {typeof(Cavalier), 3f  },
      {typeof(Giant),    5f  }
    };

    //Troops Range Width
    public static Dictionary<Type,float> RangeWidth = new Dictionary<Type, float>() { 
      {typeof(Archer),   300},
      {typeof(Warrior),  100},
      {typeof(Cavalier), 100},
      {typeof(Giant),    100}
    };

    public static Dictionary<Type,bool> IsMelee = new Dictionary<Type, bool>() { 
      {typeof(Archer),   false},
      {typeof(Warrior),  true},
      {typeof(Cavalier), true},
      {typeof(Giant),    true}
    };

    //Ranged cooldown seconds
    public const float ArcherCooldown = 0.7f;
  }
}
