using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace Assets.Scripts.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public EncounterOutcomeData encounterData;

        private PlayableDirector pd;
        private void Awake()
        {
            pd = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            encounterData = GameObject.Find("DontDestroyEncounterOutcomeData").GetComponent<EncounterOutcomeData>();
        }

        public void StartGame()
        {
            encounterData.NextRoom = "Room2";
            pd.Play();
            StartCoroutine(SceneTransition());
        }

        private System.Collections.IEnumerator SceneTransition()
        {
            yield return new WaitForSeconds((float)pd.duration);
            SceneManager.LoadScene("DungeonScene", LoadSceneMode.Single);
        }
    }
}
