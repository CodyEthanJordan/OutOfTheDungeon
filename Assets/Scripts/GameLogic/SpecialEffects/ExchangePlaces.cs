using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.GameLogic.SpecialEffects
{
    [CreateAssetMenu(menuName = "Dungeon/SpecialEffects/ExchangePlaces")]
    public class ExchangePlaces : SpecialEffect
    {
        public override void ApplyEffect(GameManager gm, UnitController user, UnitController guyHit, Vector3Int targetLocation)
        {
            if (guyHit == null)
            {
                Debug.LogError("attempted to use ExchangePlaces on null unit, not supported");
            }

            Vector3Int userLoctaion = Vector3Int.FloorToInt(user.transform.position);
            Vector3Int targetLoctaion = Vector3Int.FloorToInt(guyHit.transform.position);
            gm.MoveUnit(user, targetLocation, costsMovement: false);
            gm.MoveUnit(guyHit, userLoctaion, costsMovement: false);

        }
    }
}
