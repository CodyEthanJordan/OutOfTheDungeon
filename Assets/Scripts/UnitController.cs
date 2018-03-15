using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class UnitController : MonoBehaviour
    {
        public enum SideEnum
        {
            Player,
            BadGuy
        }

        public int CurrentMovement = 5;
        public int MaxMovement = 5;
        public bool HasActed = false;
        public List<Ability> Abilities;
        public int MaxHP = 5;
        public int HP;
        public SideEnum Side;

        public List<Vector3Int> fak;

        private Canvas MouseoverUI;


        public void NewTurn()
        {
            HasActed = false;
            CurrentMovement = MaxMovement;
        }

        internal void TakeDamage(int dmg)
        {
            HP -= dmg;
            if (HP <= 0)
            {
                //TODO: make event handler, turn HP into field instead of property, make not suck
                Debug.Log("I'm ded");
            }
        }

        private void Awake()
        {
            Abilities = new List<Ability>();
            var a = new Ability
            {
                Name = "Slash"
            };
            Abilities.Add(a);
            HP = MaxHP;
            MouseoverUI = transform.GetChild(0).GetComponent<Canvas>();
        }

        public void EnableUI()
        {
            MouseoverUI.gameObject.SetActive(true);
        }

        public void DisableUI()
        {
            MouseoverUI.gameObject.SetActive(false);
        }

        private void Start()
        {
           
        }

        internal void moveTo(Vector3Int destination)
        {
            this.transform.position = destination;
        }
    }
}
