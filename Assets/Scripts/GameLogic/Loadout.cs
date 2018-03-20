using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    [CreateAssetMenu(menuName = "Dungeon/Loadout")]
    public class Loadout : ScriptableObject
    {
        public string LoadoutName;
        public Sprite Image;
        public int MaxHP;
        public int BaseMovement;
        public Ability[] Abilities;
    }
}
