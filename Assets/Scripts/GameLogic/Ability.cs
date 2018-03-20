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
        public Effect[] SelfEffects;

        public void ApplyEffects(GameManager gm, UnitController user, Vector3Int target)
        {
            //find everyone affected
            //TODO: be able to rotate to different facing directions
            foreach (var effect in Effects)
            {
                var affectedPosition = target + effect.TileAffected;
                var guyHit = gm.AllUnits.Find(u => u.transform.position == affectedPosition);
                if (guyHit != null)
                {
                    effect.ApplyEffect(gm, user, guyHit, target);
                }

                var tile = gm.Dungeon.GetTile<DungeonTile>(target);
                effect.ApplyTileEffect(gm, user, tile, target);
            }

            foreach (var selfEffect in SelfEffects)
            {
                selfEffect.ApplyEffect(gm, user, user, target);
            }
        }

        public enum RangeType
        {
            Melee,
            Ray,
            Mortar,
            Personal
        }
    }
}
