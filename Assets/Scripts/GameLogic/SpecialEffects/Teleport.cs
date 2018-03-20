using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Scripts.GameLogic;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    public class Teleport : Effect
    {
        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            if(gm.Passable(targetLocation))
            {
                guyHit.moveTo(targetLocation);
            }
        }
    }
}
