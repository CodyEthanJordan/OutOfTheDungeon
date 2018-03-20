using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        public GameObject Ability1Button;
        public Text UnitMovementText;
        public Text HPText;
        public Text TurnCounterText;
        public Text TileName;
        public Text TileDescription;
        public Text HirelingsSavedText;
        public Text HirelingsRemainingText;

        public void DisplayUnitInfo(UnitController unit)
        {
            if (unit.Side == UnitController.SideEnum.Player)
            {
                Ability1Button.SetActive(true);
                Ability1Button.GetComponentInChildren<Text>().text = unit.MyLoadout.Abilities[0].Name;
            }
            UnitMovementText.gameObject.SetActive(true);
            HPText.gameObject.SetActive(true);

            //TODO make event listener in Unit to automatically update this info
            UnitMovementText.text = "Move: " + unit.CurrentMovement + " / " + unit.MaxMovement;
            HPText.text = "HP: " + unit.HP + " / " + unit.MaxHP;
        }

        public void HideUnitInfo()
        {
            Ability1Button.SetActive(false);
            UnitMovementText.gameObject.SetActive(false);
            HPText.gameObject.SetActive(false);
        }

        internal void UpdateTurn(int turnCounter)
        {
            TurnCounterText.text = "Turn: " + turnCounter.ToString();
        }

        internal void ShowMouseOverInfo(DungeonTile dungeonTileUnderMouse, UnitController unitUnderMouse)
        {
            if (unitUnderMouse != null)
            {
                unitUnderMouse.EnableUI();
            }

            if (dungeonTileUnderMouse != null)
            {
                TileName.text = dungeonTileUnderMouse.Name;
                TileDescription.text = dungeonTileUnderMouse.Description;
            }
            else
            {
                TileName.text = "";
                TileDescription.text = "";
            }
        }

        public void UpdateHirelingsSaved(int h)
        {
            HirelingsSavedText.text = "Hirelings Saved: " + h;
        }

        public void UpdateHirelingsReamining(int h)
        {
            HirelingsRemainingText.text = "Hirelings Remaining: " + h;
        }
    }
}
