using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    public class Unit
    {
        public int CurrentMovement = 5;
        public int MaxMovement = 5;
        public Vector3Int Position;
        public bool HasActed = false;

        public Unit()
        {
            Position = new Vector3Int(0,0,0);
        }

        public void NewTurn()
        {
            HasActed = false;
            CurrentMovement = MaxMovement;
        }
    }
}
