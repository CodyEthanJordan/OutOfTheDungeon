using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.GameLogic;
using UnityEngine;
using UnityEngine.Playables;

namespace Assets.Scripts
{
    class DestroyAfterTimeline : MonoBehaviour
    {
        private PlayableDirector pd;

        public float Duration { get; private set; }

        private float timeBetweenFrames = 0.03f;
        private float lerpStep = 0.1f;

        private void Awake()
        {
            pd = GetComponentInChildren<PlayableDirector>();
            Duration = (float)pd.duration;
        }

        public void Setup(Vector3 origin, Vector3 target, Ability.RangeType range)
        {
            if (range == Ability.RangeType.Ray || range == Ability.RangeType.Mortar)
            {
                Duration = Duration + timeBetweenFrames / lerpStep;
            }
            StartCoroutine(Run(origin, target, range));
        }

        System.Collections.IEnumerator Run(Vector3 origin, Vector3 target, Ability.RangeType range)
        {
            switch (range)
            {
                case Ability.RangeType.Mortar:
                case Ability.RangeType.Ray:
                    float t = 0;
                    if (origin != target)
                    {
                        while (t < 1)
                        {
                            this.transform.position = Vector3.Lerp(origin, target, t);
                            t = t + lerpStep;
                            yield return new WaitForSeconds(timeBetweenFrames);
                        }
                    }
                    break;

                default:
                    break;
            }

            pd.Play();
            Destroy(this.gameObject, (float)pd.duration);
        }
    }
}
