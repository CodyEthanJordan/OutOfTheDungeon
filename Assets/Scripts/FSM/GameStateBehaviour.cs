using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class GameStateBehaviour : StateMachineBehaviour
    {
        protected GameManager gm;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            gm = animator.gameObject.GetComponent<GameManager>();
        }
    }
}
