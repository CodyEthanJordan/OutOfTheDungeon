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
        public GameObject AnimationEffectPrefab;

        public static Vector3Int RotateEffectTarget(Vector3Int offset, Vector3Int facingDirection)
        {
            int directionIndex = GameManager.CardinalDirections.IndexOf(facingDirection);
            switch(directionIndex)
            {
                case 0: //north
                    return new Vector3Int(offset.y,offset.x,0);
                case 1: //south
                    return new Vector3Int(-offset.y,-offset.x,0);
                case 2: //east
                    return offset;
                case 3: //west
                    return new Vector3Int(-offset.x, offset.y, offset.z);

                default:
                    Debug.LogError("newDirection not cardinal direction!");
                    throw new NotImplementedException();
            }

        }



        public System.Collections.IEnumerator ApplyEffects(GameManager gm, UnitController user, Vector3Int target)
        {
            float waitTime = 0;
            if (AnimationEffectPrefab != null)
            {
                var animObject = Instantiate(AnimationEffectPrefab, target, Quaternion.identity);
                var anim = animObject.GetComponent<DestroyAfterTimeline>();
                anim.Setup(user.transform.position, target, Range);
                waitTime = anim.Duration;
            }

            yield return new WaitForSeconds(waitTime);

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
