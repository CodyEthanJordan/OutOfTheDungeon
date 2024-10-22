﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class MouseoverUIManager : MonoBehaviour
    {
        private Canvas canvas;
        public Text HPText;
        [SerializeField] private HPBar hpBar;

        private void Awake()
        {
            canvas = this.GetComponent<Canvas>();
        }

        public void UpdateHPText(int HP, int maxHP)
        {
            HPText.text = "HP: " + HP + "/" + maxHP;
        }

        internal void DisableUI()
        {
            HPText.enabled = false;
        }

        internal void EnableUI()
        {
            HPText.enabled = true;
        }
    }
}
