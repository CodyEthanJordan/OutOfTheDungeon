using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    public class Unit
    {
        public int Movement = 5;
        public Vector3Int Position;

        public Unit()
        {
            Position = new Vector3Int(0,0,0);
        }
    }
}
