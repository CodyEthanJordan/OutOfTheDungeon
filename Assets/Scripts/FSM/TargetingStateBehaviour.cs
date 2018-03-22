using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts.FSM
{
    public class TargetingStateBehaviour : GameStateBehaviour
    {
        public override void OnStateEnter(UnityEngine.Animator animator, UnityEngine.AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, animatorStateInfo, layerIndex);
            gm.UIHighlightClickedEvent.AddListener(UseAbility);
            gm.RenderAbility(gm.UnitClicked.MyLoadout.Abilities[gm.SelectedAbilityIndex]);
            gm.MouseoverChangedEvent.AddListener(UpdateAbilityOverlay);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo animatorStateInfo, int layerIndex)
        {
            gm.UIHighlightClickedEvent.RemoveListener(UseAbility);
            gm.MouseoverChangedEvent.RemoveListener(UpdateAbilityOverlay);
            gm.ClearOverlays();
        }

        private void UpdateAbilityOverlay(Vector3Int pos)
        {
            if (gm.TileBeingTargeted(pos))
            {
                var ability = gm.UnitClicked.MyLoadout.Abilities[gm.SelectedAbilityIndex];
                gm.RenderAbilityInfoAt(ability, pos);
            }
            else
            {
                gm.ClearAbilityInfo();
            }
        }

        private void UseAbility(Vector3Int target)
        {
            gm.ActivateAbility(gm.UnitClicked, gm.UnitClicked.MyLoadout.Abilities[gm.SelectedAbilityIndex], target);
            gm.TriggerTransition(GameStateTransitions.Deselect);
        }
    }
}
