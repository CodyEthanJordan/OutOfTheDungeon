using Assets.Scripts.GameLogic.SpecialEffects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    [System.Serializable]
    public class Effect
    {
        public enum DamageTypes
        {
            Iron,
            Magic,
            KnockbackImpact,
            SummoningBlocked,
            Healing,
            Fire
        }

        public Vector3Int TileAffected;
        public int Damage;
        public DamageTypes DamageType;
        public SpecialEffect[] SpecialEffects;

        public void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            guyHit.TakeDamage(Damage, DamageType);
            foreach (var se in SpecialEffects)
            {
                se.ApplyEffect(gm, user, guyHit, targetLocation);
            }
        }

        internal void ApplyTileEffect(GameManager gm, UnitController user, DungeonTile tile, Vector3Int target)
        {
            //TODO: melting and shit, tile effects
        }
    }
}
