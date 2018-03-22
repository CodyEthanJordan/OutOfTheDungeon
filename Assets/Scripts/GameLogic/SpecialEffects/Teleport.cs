using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Scripts.GameLogic;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/Teleport")]
    public class Teleport : SpecialEffect
    {
        public bool IgnoreCollision;

        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            if(IgnoreCollision)
            {
                //DANGEROUS, use with care, can put you inside stuff
                gm.TeleportUnit(guyHit, targetLocation);
            }
            else
            {
                if (gm.Passable(targetLocation, blockedByUnits: true))
                {
                    gm.TeleportUnit(guyHit, targetLocation);
                }
            }
        }
    }
}
