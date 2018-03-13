using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class UIManager : MonoBehaviour
    {
        public GameObject Ability1Button;
        public Text UnitMovement;

        public void DisplayUnitInfo(UnitController unit)
        {
            Ability1Button.SetActive(true);
            UnitMovement.gameObject.SetActive(true);

            Ability1Button.GetComponentInChildren<Text>().text = unit.Abilities[0].Name;
            //TODO make event listener in Unit to automatically update this info
            UnitMovement.text = "Move: " + unit.CurrentMovement + " / " + unit.MaxMovement;
        }

        public void HideUnitInfo()
        {
            Ability1Button.SetActive(false);
            UnitMovement.gameObject.SetActive(false);
        }
    }
}
