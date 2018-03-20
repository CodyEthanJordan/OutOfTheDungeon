using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    [CreateAssetMenu(menuName = "Dungeon/Effect")]
    public class Effect : ScriptableObject
    {
        public enum DamageTypes
        {
            Iron,
            Magic,
            KnockbackImpact,
            SummoningBlocked,
        }

        public Vector3Int TileAffected;
        public int Damage;
        public DamageTypes DamageType;
        public bool Knockback;

        public virtual void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            guyHit.TakeDamage(Damage, DamageType);
            if (Knockback)
            {
                Vector3Int knockbackDirection;
                if (TileAffected == Vector3Int.zero)
                {
                    knockbackDirection = Vector3Int.FloorToInt(Vector3.Normalize(guyHit.transform.position - user.transform.position));
                }
                else
                {
                    //knock back away from origin point
                    knockbackDirection = Vector3Int.FloorToInt(Vector3.Normalize(TileAffected));
                }

                gm.KnockBack(guyHit, knockbackDirection);
            }
        }

        internal void ApplyTileEffect(GameManager gm, UnitController user, DungeonTile tile, Vector3Int target)
        {
            //TODO: melting and shit, tile effects
            throw new NotImplementedException();
        }
    }
}
