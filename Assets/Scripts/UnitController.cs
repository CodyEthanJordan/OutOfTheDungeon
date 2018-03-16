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

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                this.gameObject.name = _name;
            }
        }

        public int CurrentMovement = 5;
        public int MaxMovement = 5;
        public bool HasActed = false;
        public List<Ability> Abilities;
        public int MaxHP = 5;
        private int _hp;
        public int HP
        {
            get { return _hp; }
            set
            {
                _hp = value;
                MouseoverUI.UpdateHPText(_hp, MaxHP);
            }
        }
        public SideEnum Side;
        public int Damage = 1;

        public List<Vector3Int> TargetedTiles;

        private MouseoverUIManager MouseoverUI;

        private void Awake()
        {
            MouseoverUI = transform.GetChild(0).GetComponent<MouseoverUIManager>();
            Abilities = new List<Ability>();
            var a = new Ability
            {
                Name = "Slash"
            };
            Abilities.Add(a);
            HP = MaxHP;
            TargetedTiles = new List<Vector3Int>();
        }


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
       
        public void EnableUI()
        {
            MouseoverUI.EnableUI();
        }

        public void DisableUI()
        {
            MouseoverUI.DisableUI();
        }

        internal void moveTo(Vector3Int destination)
        {
            this.transform.position = destination;
        }
    }
}
