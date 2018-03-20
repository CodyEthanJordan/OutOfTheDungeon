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
        public Vector3Int TileAffected;
        public int Damage;
        public bool Knockback;

        public void ApplyEffect(GameManager gm, UnitController unit, Vector3Int dirFromOrigin)
        {
            unit.TakeDamage(Damage);

            if(Knockback)
            {
                gm.KnockBack(unit, dirFromOrigin);
            }
        }
    }
}
