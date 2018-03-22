using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts
{
    class EncounterOutcomeData : MonoBehaviour
    {
        public string RoomToGoTo;
        public int HirelingsSaved;

        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }
}
