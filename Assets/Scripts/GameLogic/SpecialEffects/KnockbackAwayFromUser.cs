using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/KnockbackAwayFromUser")]
    public class KnockbackAwayFromUser : SpecialEffect
    {
        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            var directionToTarget = guyHit.transform.position - user.transform.position;
            Vector3Int knockbackDirection = Vector3Int.zero;
            if (Math.Abs(directionToTarget.x) > Math.Abs(directionToTarget.y))
            {
                knockbackDirection = new Vector3Int((int)directionToTarget.x, 0, 0);
            }
            else
            {
                knockbackDirection = new Vector3Int(0, (int)directionToTarget.y, 0);
            }
            gm.KnockBack(guyHit, knockbackDirection);
        }
    }
}
