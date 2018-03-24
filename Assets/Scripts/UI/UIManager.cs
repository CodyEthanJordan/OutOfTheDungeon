using Assets.Scripts.GameLogic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class UIManager : MonoBehaviour
    {
        public GameObject UnitDetailsPanel;
        public GameObject AbilityButtonPrefab;
        public Text UnitMovementText;
        public Text UnitNameText;
        public Text HPText;
        public Text TurnCounterText;
        public GameObject TileInfoPanel;
        public GameObject InfoBoxPrefab;
        public Text HirelingsSavedText;
        public Text HirelingsRemainingText;
        public Text VictoryText;
        public GameObject ResolutionPopup;
        public GameObject ResolutionTextPrefab;
        public Button ReturnToMenu;
        public GameObject IntroOverlay;
        public Text IntroDescriptionText;

        private List<GameObject> abilityButtons = new List<GameObject>();

        public void InitializeUI()
        {
            VictoryText.gameObject.SetActive(false);
            HideUnitInfo();
        }

        public void DisplayUnitInfo(GameManager gm, UnitController unit)
        {
            int i = 0;
            if (unit.Side == UnitController.SideEnum.Player)
            {
                foreach (var ability in unit.MyLoadout.Abilities)
                {
                    int abilityIndex = i;
                    var button = Instantiate(AbilityButtonPrefab, UnitDetailsPanel.transform);
                    abilityButtons.Add(button);
                    button.transform.GetChild(0).GetComponent<Text>().text = ability.Name;
                    button.GetComponent<Button>().onClick.AddListener(() => gm.AbilityButtonClick(abilityIndex));
                    button.GetComponent<MouseoverPopup>().Details = ability.Description;
                    i++;
                }
            }
            UnitMovementText.gameObject.SetActive(true);
            HPText.gameObject.SetActive(true);
            UnitNameText.gameObject.SetActive(true);

            //TODO make event listener in Unit to automatically update this info
            UnitMovementText.text = "Move: " + unit.CurrentMovement + " / " + unit.MaxMovement;
            HPText.text = "HP: " + unit.HP + " / " + unit.MaxHP;
            UnitNameText.text = unit.Name;
        }

        public void HideUnitInfo()
        {
            UnitMovementText.gameObject.SetActive(false);
            HPText.gameObject.SetActive(false);
            UnitNameText.gameObject.SetActive(false);
            foreach (var button in abilityButtons)
            {
                Destroy(button);
            }
            abilityButtons.Clear();
        }

        internal void UpdateTurn(int turnCounter)
        {
            TurnCounterText.text = "Turn: " + turnCounter.ToString();
        }

        internal void ShowMouseOverInfo(DungeonTile dungeonTileUnderMouse, UnitController unitUnderMouse, List<DangerzoneController> dangerzonesUnderMouse)
        {
            if (unitUnderMouse != null)
            {
                unitUnderMouse.EnableUI();
            }

            foreach (Transform t in TileInfoPanel.transform)
            {
                Destroy(t.gameObject);
            }

            if (dungeonTileUnderMouse != null)
            {
                var tileInfo = Instantiate(InfoBoxPrefab, TileInfoPanel.transform).GetComponent<InfoBox>();
                tileInfo.Setup(dungeonTileUnderMouse.Name, dungeonTileUnderMouse.Description);
            }

            foreach (var dz in dangerzonesUnderMouse)
            {
                var dangerzoneInfo = Instantiate(InfoBoxPrefab, TileInfoPanel.transform).GetComponent<InfoBox>();
                dangerzoneInfo.Setup(dz.Title, dz.Description);
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

        public void GameOver(bool win, int hirelingsSaved)
        {
            ReturnToMenu.gameObject.SetActive(true);
            VictoryText.gameObject.SetActive(true);
            if (win)
            {
                VictoryText.text = "Onto the next room...\nHirelings saved: " + hirelingsSaved;
                VictoryText.color = Color.green;
            }
            else
            {
                VictoryText.text = "Defeated!";
                VictoryText.color = Color.red;
            }
        }

        internal void ShowTurnResolution(List<string> resolutionInfo)
        {
            ResolutionPopup.SetActive(true);
            foreach (Transform oldTextBox in ResolutionPopup.transform)
            {
                Destroy(oldTextBox.gameObject);
            }
            int i = 1;
            foreach (var text in resolutionInfo)
            {
                var newTextBox = Instantiate(ResolutionTextPrefab, ResolutionPopup.transform);
                var textComponent = newTextBox.GetComponent<Text>();
                textComponent.text = i + " " + text;
                i++;
            }
        }

        internal void HideTurnResolution()
        {
            ResolutionPopup.SetActive(false);
        }


        public void ShowIntroduction(string roomInfoTitle, string roomInfoIntoDescription)
        {
            IntroOverlay.SetActive(true);
            IntroDescriptionText.text = roomInfoIntoDescription;
        }
    }
}
