using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class UnitClickedBehaviour : GameStateBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            Debug.Log("welcome to the unit clicked zone");
            gm.RenderMovement();
        }
    }
}
