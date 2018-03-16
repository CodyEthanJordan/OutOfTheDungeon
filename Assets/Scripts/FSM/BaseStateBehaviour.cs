using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.FSM
{
    public class BaseStateBehaviour : GameStateBehaviour
    {
        override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            gm.UnitClickedEvent.AddListener(OnUnitClick);
            gm.ClearOverlays();
            gm.DeselectUnit();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            gm.UnitClickedEvent.RemoveListener(OnUnitClick);
        }

        private void OnUnitClick(UnitController u)
        {
            gm.UnitClicked = u;
            gm.TriggerTransition(GameStateTransitions.UnitClicked);
        }
    }
}
