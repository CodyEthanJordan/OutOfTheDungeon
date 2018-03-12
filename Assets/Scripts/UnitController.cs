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

        private void Awake()
        {
            UnitRepresented = new Unit();
        }

        internal void moveTo(Vector3Int destination)
        {
            this.transform.position = destination;
            UnitRepresented.Position = destination;
        }
    }
}
