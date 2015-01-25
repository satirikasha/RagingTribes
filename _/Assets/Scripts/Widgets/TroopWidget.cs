namespace RagingTribes.Game {
  using UnityEngine;
  using System.Collections;
  using System.Linq;
  using Engine.Input;
  using Engine.Utils;
  using RagingTribes.Game.Units;
using System;

  public class TroopWidget: MonoBehaviour {

    private Tribe _Parent;

    private TextMesh _Count;
    private SpriteRenderer _ColorPanel;

    private int _ArcherCount = 0;
    private int _WarriorCount = 0;
    private int _CavalierCount = 0;
    private int _GiantCount = 0;

    void Start() {
      _Parent = this.gameObject.GetComponentInParent<Tribe>();
      _ColorPanel = this.transform.FindChild("TroopWidget.Left").GetComponent<SpriteRenderer>();
      _Count = this.transform.FindChild("Text").GetComponent<TextMesh>();

      _Parent.Factory.OnTroopsAdded += m => {
        foreach(Troops t in m) {
          var type = t.GetType();
          if(type == typeof(Archer))
            _ArcherCount++; 
          if(type == typeof(Warrior))
            _WarriorCount++;
          if(type == typeof(Cavalier))
            _CavalierCount++;
          if(type == typeof(Giant))
            _GiantCount++;
        }
        if(_ArcherCount != 0)
          _ColorPanel.material.SetFloat("_Ammount1", _ArcherCount);
        if(_WarriorCount != 0)
          _ColorPanel.material.SetFloat("_Ammount2", _WarriorCount);
        if(_CavalierCount != 0)
          _ColorPanel.material.SetFloat("_Ammount3", _CavalierCount);
        if(_GiantCount != 0)
          _ColorPanel.material.SetFloat("_Ammount4", _GiantCount);

        RefreshCount();
      };

      _Parent.Factory.OnTroopsRemoved += m => {
        var archerCountChanged = false;
        var warriorCountChanged = false;
        var cavalierCountChanged = false;
        var giantCountChanged = false;
        foreach(Troops t in m) {
          var type = t.GetType();
          if(type == typeof(Archer)) {
            _ArcherCount--;
            archerCountChanged = true;
          }
          if(type == typeof(Warrior)) {
            _WarriorCount--;
            warriorCountChanged = true;
          }
          if(type == typeof(Cavalier)) {
            _CavalierCount--;
            cavalierCountChanged = true;
          }
          if(type == typeof(Giant)) {
            _GiantCount--;
            giantCountChanged = true;
          }
        }
        if(archerCountChanged)
          _ColorPanel.material.SetFloat("_Ammount1", _ArcherCount);
        if(warriorCountChanged)
          _ColorPanel.material.SetFloat("_Ammount2", _WarriorCount);
        if(cavalierCountChanged)
          _ColorPanel.material.SetFloat("_Ammount3", _CavalierCount);
        if(giantCountChanged)
          _ColorPanel.material.SetFloat("_Ammount4", _GiantCount);

        RefreshCount();
      };

      _Parent.Factory.OnTroopsRequalified += m => {
        var productionType = _Parent.Factory.GetProductionType();
        var archerCountChanged = false;
        var warriorCountChanged = false;
        var cavalierCountChanged = false;
        var giantCountChanged = false;
        foreach(Troops t in m) {
          var type = t.GetType();
          if(type == typeof(Archer)) {
            _ArcherCount--;
            archerCountChanged = true;
          }
          if(type == typeof(Warrior)) {
            _WarriorCount--;
            warriorCountChanged = true;
          }
          if(type == typeof(Cavalier)) {
            _CavalierCount--;
            cavalierCountChanged = true;
          }
          if(type == typeof(Giant)) {
            _GiantCount--;
            giantCountChanged = true;
          }

          if(productionType == typeof(Archer)) {
            _ArcherCount++;
            archerCountChanged = true;
          }
          if(productionType == typeof(Warrior)) {
            _WarriorCount++;
            warriorCountChanged = true;
          }
          if(productionType == typeof(Cavalier)) {
            _CavalierCount++;
            cavalierCountChanged = true;
          }
          if(productionType == typeof(Giant)) {
            _GiantCount++;
            giantCountChanged = true;
          }
        }
        if(archerCountChanged)
          _ColorPanel.material.SetFloat("_Ammount1", _ArcherCount);
        if(warriorCountChanged)
          _ColorPanel.material.SetFloat("_Ammount2", _WarriorCount);
        if(cavalierCountChanged)
          _ColorPanel.material.SetFloat("_Ammount3", _CavalierCount);
        if(giantCountChanged)
          _ColorPanel.material.SetFloat("_Ammount4", _GiantCount);

        RefreshCount();
      };
    }

    private void RefreshCount() {
      _Count.text = (_ArcherCount + _WarriorCount + _CavalierCount + _GiantCount).ToString();
    }
  }
}