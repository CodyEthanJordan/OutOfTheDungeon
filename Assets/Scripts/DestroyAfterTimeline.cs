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

        private void Awake()
        {
            pd = GetComponent<PlayableDirector>();
            Destroy(gameObject, (float)pd.duration);
        }
    }
}
