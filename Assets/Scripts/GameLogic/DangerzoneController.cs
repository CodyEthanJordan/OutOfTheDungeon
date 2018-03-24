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

        public System.Collections.IEnumerator ApplyEndOfTurnEffects(GameManager gm, UnitController unitStepping)
        {
            float waitTime = 0;
            if (AnimationEffect != null)
            {
                var animObject = Instantiate(AnimationEffect, this.transform.position, Quaternion.identity);
                var anim = animObject.GetComponent<DestroyAfterTimeline>();
                anim.Setup(this.transform.position, this.transform.position, Ability.RangeType.Personal);
                waitTime = anim.Duration;
            }
            yield return new WaitForSeconds(waitTime);

            foreach (var endOfTurnEffect in EndOfTurnEffects)
            {
                endOfTurnEffect.ApplyEffect(gm, null, unitStepping, Vector3Int.FloorToInt(this.transform.position));
            }
        }


    }
}
