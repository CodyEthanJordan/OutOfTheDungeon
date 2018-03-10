using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitController : MonoBehaviour
    {
        public Unit UnitRepresented;

        private void Start()
        {
            UnitRepresented = new Unit();
        }
    }
}
