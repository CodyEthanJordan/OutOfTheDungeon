﻿using Assets.Scripts.Events;
using Assets.Scripts.GameLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Scripts
{
    public class UnitController : MonoBehaviour
    {
        public GameObject DangerzoneUI;
        private List<GameObject> TargetedTileOverlays = new List<GameObject>();

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

        public Loadout[] AllLoadouts;
        public Loadout MyLoadout;

        public int CurrentMovement = 5;
        public int MaxMovement = 5;
        private bool _hasActed;
        public bool HasActed
        {
            get { return _hasActed; }
            set
            {
                _hasActed = value;
                if(_hasActed)
                {
                    sr.color = Color.Lerp(baseColor, Color.black, 0.2f);
                }
                else
                {
                    sr.color = baseColor;
                }
            }
        }
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

        public List<Vector3Int> TargetedTiles;
        public UnitEvent DeathEvent = new UnitEvent();

        private MouseoverUIManager MouseoverUI;
        private SpriteRenderer sr;
        private Color baseColor;

        private void Awake()
        {
            MouseoverUI = transform.GetChild(0).GetComponent<MouseoverUIManager>();
            HP = MaxHP;
            TargetedTiles = new List<Vector3Int>();
            sr = this.GetComponent<SpriteRenderer>();
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
                Die();
            }
        }

        public void EnableUI()
        {
            MouseoverUI.EnableUI();
            foreach (var dangerzone in TargetedTileOverlays)
            {
                dangerzone.GetComponent<SpriteRenderer>().color = Color.yellow;
            }
        }

        public void DisableUI()
        {
            MouseoverUI.DisableUI();
            foreach (var dangerzone in TargetedTileOverlays)
            {
                dangerzone.GetComponent<SpriteRenderer>().color = Color.red;
            }
        }

        internal void moveTo(Vector3Int destination)
        {
            var offset = destination - Vector3Int.FloorToInt(this.transform.position);
            for (int i = 0; i < TargetedTiles.Count; i++)
            {
                TargetedTiles[i] = TargetedTiles[i] + offset;
                TargetedTileOverlays[i].transform.position = TargetedTileOverlays[i].transform.position + offset;
            }
            this.transform.position = destination;
        }

        internal void TargetTile(Vector3Int target)
        {
            TargetedTiles.Add(target);
            var overlay = Instantiate(DangerzoneUI, target, Quaternion.identity);
            overlay.GetComponent<SpriteRenderer>().color = Color.red;
            TargetedTileOverlays.Add(overlay);
        }

        public void ClearTargetOverlays()
        {
            foreach (var overlay in TargetedTileOverlays)
            {
                Destroy(overlay);
            }
            TargetedTileOverlays.Clear();
            TargetedTiles.Clear();
        }

        public void SetupUnit(string name, SideEnum side, Vector3Int position, string LoadoutName)
        {
            MyLoadout = AllLoadouts.FirstOrDefault(l => l.LoadoutName == LoadoutName);
            if(MyLoadout == null)
            {
                Debug.LogError("Can't find loadout called " + LoadoutName);
            }

            MaxHP = MyLoadout.MaxHP;
            MaxMovement = MyLoadout.BaseMovement;
            HP = MaxHP;
            CurrentMovement = MaxMovement;
            Name = name;
            this.transform.position = position;
            sr.sprite = MyLoadout.Image;

            Side = side;
            switch (side)
            {
                case SideEnum.Player:
                    baseColor = Color.white;
                    break;

                case SideEnum.BadGuy:
                    sr.color = Color.red;
                    baseColor = Color.red;
                    break;

                case SideEnum.Hireling:
                    sr.color = Color.green;
                    baseColor = Color.green;
                    break;

                default:
                    break;
            }

            DisableUI();
        }

        public enum SideEnum
        {
            Player,
            BadGuy,
            Hireling,
        }

        internal void Die()
        {
            DeathEvent.Invoke(this);
            foreach (var tile in TargetedTileOverlays)
            {
                Destroy(tile);
            }
            Destroy(this.gameObject);
        }

        internal void Move(List<Vector3Int> path, float animationSpeed, Func<bool> completionCallback)
        {
            StartCoroutine(LerpMove(path, animationSpeed, completionCallback));
        }

        private System.Collections.IEnumerator LerpMove(List<Vector3Int> path, float animationSpeed, Func<bool> completionCallback)
        {
            float l = 0;
            Vector3 previousPosition = this.transform.position;
            foreach (var step in path)
            {
                while (l < 1)
                {
                    this.transform.position = Vector3.Lerp(previousPosition, step, l);
                    l += 1 / animationSpeed;
                    yield return new WaitForSeconds(animationSpeed);
                }
                l = 0;
                previousPosition = step;
            }
            //TODO: unify with moveTo logic
            this.transform.position = path.Last();
            completionCallback();
        }
    }
}
