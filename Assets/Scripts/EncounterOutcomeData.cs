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

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
