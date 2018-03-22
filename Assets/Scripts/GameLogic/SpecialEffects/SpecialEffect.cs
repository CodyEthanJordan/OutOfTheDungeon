using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    public abstract class SpecialEffect : ScriptableObject
    {
        public abstract void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation);
    }
}
