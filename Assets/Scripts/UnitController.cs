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
        public Sprite[] Graphics;

        public List<Vector3Int> TargetedTiles;

        private MouseoverUIManager MouseoverUI;
        private SpriteRenderer sr;

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

        public void SetupUnit(string name, SideEnum side, Vector3Int position)
        {
            //TODO: pass in data about unit class, probably scriptable object
            Name = name;
            this.transform.position = position;

            Side = side;
            switch (side)
            {
                case SideEnum.Player:
                    break;

                case SideEnum.BadGuy:
                    sr.sprite = Graphics[1];
                    sr.color = Color.red;
                    break;

                case SideEnum.Hireling:
                    sr.sprite = Graphics[2];
                    sr.color = Color.green;
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
