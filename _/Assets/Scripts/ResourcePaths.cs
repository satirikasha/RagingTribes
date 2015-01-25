using System;
using System.Text;
using RagingTribes.Game.Units;

namespace RagingTribes.Menu {

  public static class ResourcePaths {
    private const string _MainFolder = "Sprites/Menu/";

    public const string Background = _MainFolder + "MenuBackground";
  }
}

namespace RagingTribes.Game {
  public static class ResourcePaths {
    private const string _SpritesFolder = "Sprites/Game/";
    private const string _WallsFolder = _SpritesFolder + "Walls/";
    private const string _BarracksFolder = _SpritesFolder + "Barracks/";
    private const string _WeaponaryFolder = _SpritesFolder + "Weaponary/";
    private const string _HousesFolder = _SpritesFolder + "Houses/";
    private const string _GroundsFolder = _SpritesFolder + "Grounds/";

    private const string _PrefabsFolder = "Prefabs/";



    public const string Background = _SpritesFolder + "Background";

    public const string SliderWidget = _PrefabsFolder + "SliderWidget";

    public const string Archer   = _PrefabsFolder + "Archer";
    public const string Warrior  = _PrefabsFolder + "Warrior";
    public const string Cavalier = _PrefabsFolder + "Cavalier";
    public const string Giant    = _PrefabsFolder + "Giant";

    public static string GetWall(Race race, int level) {
      switch(race) {
        case Race.Viking: return _WallsFolder + "Wall_V_" + level;
        case Race.Evil: return _WallsFolder + "Wall_E_" + level;
        case Race.Indian: return _WallsFolder + "Wall_I_" + level;
        case Race.Neutral: return _WallsFolder + "Wall_N_" + level;
        default: throw new Exception("Bad race");
      }
    }

    public static string GetBarrack(Race race, int level) {
      switch(race) {
        case Race.Viking: return _BarracksFolder + "Barrack_V_" + level;
        case Race.Evil: return _BarracksFolder + "Barrack_E_" + level;
        case Race.Indian: return _BarracksFolder + "Barrack_I_" + level;
        case Race.Neutral: return _BarracksFolder + "Barrack_N_" + level;
        default: throw new Exception("Bad race");
      }
    }

    public static string GetHouse(Race race, int level) {
      switch(race) {
        case Race.Viking: return _HousesFolder + "House_V_" + level;
        case Race.Evil: return _HousesFolder + "House_E_" + level;
        case Race.Indian: return _HousesFolder + "House_I_" + level;
        case Race.Neutral: return _HousesFolder + "House_N_" + level;
        default: throw new Exception("Bad race");
      }
    }

    public static string GetWeaponary(Race race, TroopsType type) {
      string strType;
      switch(type) {
        case TroopsType.Archer: strType = "A"; break;
        case TroopsType.Warrior: strType = "W"; break;
        case TroopsType.Cavalier: strType = "C"; break;
        case TroopsType.Giant: strType = "G"; break;
        default: throw new Exception("Bad TroopsType");
      }
      switch(race) {
        case Race.Viking: return _WeaponaryFolder + "Weaponary_V_" + strType;
        case Race.Evil: return _WeaponaryFolder + "Weaponary_E_" + strType;
        case Race.Indian: return _WeaponaryFolder + "Weaponary_I_" + strType;
        case Race.Neutral: return _WeaponaryFolder + "Weaponary_N_" + strType;
        default: throw new Exception("Bad race");
      }
    }

    public static string GetGround(Race race) {
      switch(race) {
        case Race.Viking: return _GroundsFolder + "Ground_V";
        case Race.Evil: return _GroundsFolder + "Ground_E";
        case Race.Indian: return _GroundsFolder + "Ground_I";
        case Race.Neutral: return _GroundsFolder + "Ground_N";
        default: throw new Exception("Bad race");
      }
    }
  }
}

