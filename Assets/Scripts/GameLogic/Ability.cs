using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts.GameLogic
{
    public class Ability
    {
        public string Name;
        public AbilityType Type = AbilityType.Melee; //TODO: don't hardcode

        public enum AbilityType
        {
            Melee,
            Ray,
            Mortar
        }
    }
}
