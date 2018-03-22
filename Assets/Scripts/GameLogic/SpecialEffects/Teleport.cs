using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Scripts.GameLogic;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/Teleport")]
    public class Teleport : Effect
    {
        public bool IgnoreCollision;

        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            base.ApplyEffect(gm, user, guyHit, targetLocation);
            if(IgnoreCollision)
            {
                //DANGEROUS, use with care, can put you inside stuff
                gm.MoveUnit(guyHit, targetLocation, false);
            }
            else
            {
                if (gm.Passable(targetLocation, blockedByUnits: true))
                {
                    gm.MoveUnit(guyHit, targetLocation, false);
                }
            }
        }
    }
}
