using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Collections;
using UnityEngine.Playables;

namespace Assets.Scripts
{
    class DestroyAfterTimeline : MonoBehaviour
    {
        private PlayableDirector pd;

        public float Duration { get; private set; }

        private void Awake()
        {
            pd = GetComponentInChildren<PlayableDirector>();
            Duration = (float)pd.duration;
            Destroy(gameObject, (float)pd.duration);
        }
    }
}
