using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class OverlayTooltipUI : MonoBehaviour
    {
        private SpriteRenderer sr;
        private Effect effect;
        [SerializeField] private Canvas overlayCanvas;
        [SerializeField] private Text damageText;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
        }


        public void Setup(Effect effectToShow)
        {
            this.effect = effectToShow;
        }

        public void ShowTooltip()
        {
            sr.color = Color.yellow;
            sr.sortingOrder = 5;

            overlayCanvas.enabled = true;
            damageText.text = effect.Damage.ToString();
            switch(effect.DamageType)
            {
                case Effect.DamageTypes.Iron:
                    damageText.color = Color.grey;
                    break;
                case Effect.DamageTypes.Healing:
                    damageText.color = Color.green;
                    break;
                default:
                    break;
            }
        }

        public void HideTooltip()
        {
            sr.color = Color.red;
            sr.sortingOrder = 0;

            overlayCanvas.enabled = false;
        }
    }
}
