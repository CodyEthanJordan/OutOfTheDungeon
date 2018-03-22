using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts
{
    public class EncounterOutcomeData : MonoBehaviour
    {
        public string NextRoom;
        public int HirelingsSaved;
        public Loadout[] Heros;
        public Loadout[] DebugSquad;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }

        public void SetupDebugDefaults()
        {
            NextRoom = "Room1";
            HirelingsSaved = 6;
            Heros = DebugSquad;
        
        }
    }
}
