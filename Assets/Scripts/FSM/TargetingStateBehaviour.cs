using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class TargetingStateBehaviour : GameStateBehaviour
    {
        public override void OnStateEnter(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, animatorStateInfo, layerIndex);
            gm.UIHighlightClickedEvent.AddListener(UseAbility);
            Debug.Log("welcome to the target zone");
            gm.RenderAbility();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            gm.UIHighlightClickedEvent.RemoveListener(UseAbility);
            gm.ClearOverlays();
        }

        private void UseAbility(Vector3Int target)
        {
            gm.ActivateAbility(gm.UnitClicked, target);
            gm.TriggerTransition(GameStateTransitions.Deselect);
        }
    }
}
