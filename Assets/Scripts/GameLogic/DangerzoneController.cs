using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using UnityEngine;

namespace Assets.Scripts.GameLogic
{
    public class DangerzoneController : MonoBehaviour
    {
        public string Title;
        public string Description;
        public Effect[] EndOfTurnEffects;
        public Effect[] PassingThroughEffects;
        public Effect[] SteppedOnEffects;
        public bool TriggerOnceThenDestroy;
        public GameObject AnimationEffect;

        public void ApplyEndOfTurnEffects(GameManager gm, UnitController unitStepping)
        {
            if (AnimationEffect != null)
            {
                Instantiate(AnimationEffect, this.transform.position, Quaternion.identity);
            }

            if (unitStepping != null)
            {
                foreach (var endOfTurnEffect in EndOfTurnEffects)
                {
                    endOfTurnEffect.ApplyEffect(gm, null, unitStepping, Vector3Int.FloorToInt(this.transform.position));
                }
            }
        }


    }
}
