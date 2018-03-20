using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    [CreateAssetMenu(menuName = "Dungeon/Ability")]
    public class Ability : ScriptableObject
    {
        public string Name;
        public string Description;
        public RangeType Range;
        public Effect[] Effects;

        public enum RangeType
        {
            Melee,
            Ray,
            Mortar
        }
    }
}
