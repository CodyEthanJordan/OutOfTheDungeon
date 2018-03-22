using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/FlankingAttack")]
    public class FlankingAttack : SpecialEffect
    {
        public Effect[] NormalEffects;
        public Effect[] FlankingEffects;

        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            var directionToTarget = guyHit.transform.position - user.transform.position;
            var flankingPosition = guyHit.transform.position + directionToTarget.normalized;

            if(gm.AllUnits.Where(u => u.transform.position == flankingPosition && UnitController.AlliedTo(u.Side, user.Side)).Count() > 0)
            {
                foreach (var effect in FlankingEffects)
                {
                    effect.ApplyEffect(gm, user, guyHit, targetLocation);
                }
            }
            else
            {
                foreach (var effect in NormalEffects)
                {
                    effect.ApplyEffect(gm, user, guyHit, targetLocation);
                }
            }
        }
    }
}
