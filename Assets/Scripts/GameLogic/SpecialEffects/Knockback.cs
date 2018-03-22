using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/Knockback")]
    public class Knockback : SpecialEffect
    {
        public bool AwayFromTargetLocation;

        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            Vector3Int knockbackDirection;
            if (AwayFromTargetLocation)
            {
                knockbackDirection = GameManager.CardinalDirectionTo(targetLocation, Vector3Int.FloorToInt(guyHit.transform.position));
            }
            else
            {
                var directionToTarget = guyHit.transform.position - user.transform.position;
                if (Math.Abs(directionToTarget.x) > Math.Abs(directionToTarget.y))
                {
                    knockbackDirection = new Vector3Int((int)directionToTarget.x, 0, 0);
                }
                else
                {
                    knockbackDirection = new Vector3Int(0, (int)directionToTarget.y, 0);
                }
            }

            gm.KnockBack(guyHit, knockbackDirection);
        }
    }
}
