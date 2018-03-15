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
            if (gm.UnitClicked.Side == UnitController.SideEnum.Player)
            {
                gm.RenderMovement();
                gm.UIHighlightClickedEvent.AddListener(MovementClick);
            }
            gm.DisplaySelectedUnitData();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            gm.UIHighlightClickedEvent.RemoveListener(MovementClick);
            gm.ClearOverlays();
        }

        private void MovementClick(Vector3Int destination)
        {
            gm.MoveUnit(gm.UnitClicked, destination);
            gm.TriggerTransition(GameStateTransitions.Deselect);
        }
    }
}
