﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class MainMenuController : MonoBehaviour
    {
        public EncounterOutcomeData encounterData;

        private PlayableDirector pd;
        public PlayableDirector IntoSequence;
        public Text StartContinue;
        private void Awake()
        {
            pd = GetComponent<PlayableDirector>();
        }

        private void Start()
        {
            encounterData = GameObject.Find("DontDestroyEncounterOutcomeData").GetComponent<EncounterOutcomeData>();
            if (encounterData.GameJustLaunched)
            {
                IntoSequence.Play();
            }
            else
            {
                StartContinue.text = "Continue";
            }

            if (!encounterData.RoundWon)
            {
                Debug.LogError("you dead! GG");
            }
        }

        public void StartGame()
        {
                //TODO: generalize to more rooms and pickin rooms
            if (encounterData.GameJustLaunched)
            {
                encounterData.NextRoom = "Room1";
                encounterData.GameJustLaunched = false;
            }
            else if(encounterData.NextRoom == "Room1")
            {
                encounterData.NextRoom = "Room2";
            }
            else if (encounterData.NextRoom == "Room2")
            {
                encounterData.NextRoom = "Room3";
            }
            else if (encounterData.NextRoom == "Room3")
            {
                encounterData.NextRoom = "Room4";
            }

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
