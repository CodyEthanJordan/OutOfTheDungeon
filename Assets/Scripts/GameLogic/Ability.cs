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
        public bool OnlyTargetUnits;
        public bool FreeAction;
        public Effect[] Effects;
        public Effect[] SelfEffects;

        public static Vector3Int RotateEffectTarget(Vector3Int target, Vector3Int newDirection)
        {
            int directionIndex = GameManager.CardinalDirections.IndexOf(newDirection);
            switch(directionIndex)
            {
                case 0: //north
                    return new Vector3Int(target.y,target.x,0);
                case 1: //south
                    return new Vector3Int(-target.y,-target.x,0);
                case 2: //east
                    return target;
                case 3: //west
                    return new Vector3Int(-target.x, target.y, target.z);

                default:
                    Debug.LogError("newDirection not cardinal direction!");
                    throw new NotImplementedException();
            }

        }

        public void ApplyEffects(GameManager gm, UnitController user, Vector3Int target)
        {
            //find everyone affected
            foreach (var effect in Effects)
            {
                var cardinalDirectionToTarget = GameManager.CardinalDirectionTo(Vector3Int.FloorToInt(user.transform.position), target);
                Vector3Int affectedPosition = target + RotateEffectTarget(effect.TileAffected, cardinalDirectionToTarget);

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
