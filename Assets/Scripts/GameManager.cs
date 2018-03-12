using Assets.Scripts.GameLogic;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Tilemap Dungeon;
        public Tilemap UIHighlights;
        public Tile MoveTile;
        public Tile TargetingTile;

        private GameObject unitClicked;
        public GameObject unitPrefab;

        public List<GameObject> PlayerUnits;

        public GameObject Ability1Button;

        public GameState CurrentState = GameState.BaseState;

        public enum GameState
        {
            BaseState,
            UnitSelected,
            Targeting
        }

        // Use this for initialization
        void Awake()
        {
            PlayerUnits = new List<GameObject>();
            var knight = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity, this.transform);
            knight.GetComponent<UnitController>().UnitRepresented.Position = new Vector3Int(0, 0, 0);
            PlayerUnits.Add(knight);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                switch (CurrentState)
                {
                    case GameState.BaseState:
                        if (hit.collider != null)
                        {
                            if (hit.collider.CompareTag("Unit"))
                            {
                                CurrentState = GameState.UnitSelected;
                                unitClicked = hit.collider.gameObject;
                                var unit = unitClicked.GetComponent<UnitController>().UnitRepresented;
                                RenderMovement(unit);
                                ActivateButtons(unit);
                            }
                        }
                        break;
                    case GameState.UnitSelected:
                        if (hit.collider != null)
                        {
                            if (hit.collider.CompareTag("UI"))
                            {
                                //move unit
                                var destination = Dungeon.WorldToCell(hit.point);
                                var unitController = unitClicked.GetComponent<UnitController>();
                                unitController.UnitRepresented.CurrentMovement -= DistanceTo(unitController.UnitRepresented.Position, destination);
                                unitController.moveTo(destination);
                                CurrentState = GameState.BaseState;
                                ClearOverlays();
                                DeselectUnit();
                            }
                        }
                        else
                        {
                            //CurrentState = GameState.BaseState;
                            //ClearOverlays();
                            //DeselectUnit();
                        }
                        break;
                    default:
                        break;

                }

            }

        }

        private void DeselectUnit()
        {
            Ability1Button.SetActive(false);
        }

        private void ActivateButtons(Unit unit)
        {
            Ability1Button.GetComponentInChildren<Text>().text = unit.Abilities[0].Name;
            Ability1Button.SetActive(true);
        }

        private int DistanceTo(Vector3Int position, Vector3Int destination)
        {
            //TODO horrible hack
            var moves = FindAllValidMoves(position, 20);
            var validPath = moves.FirstOrDefault(m => m.Last() == destination);
            if (validPath != null)
            {
                return validPath.Count() - 1;
            }
            else
            {
                return -1;
            }

        }

        private List<List<Vector3Int>> FindAllValidMoves(Vector3Int Position, int moves)
        {
            var validMoves = new List<List<Vector3Int>>();
            var startingPath = new List<Vector3Int>() { Position };
            Queue<List<Vector3Int>> PlacesToVisit = new Queue<List<Vector3Int>>();
            PlacesToVisit.Enqueue(new List<Vector3Int>() { Position });

            while (PlacesToVisit.Count > 0)
            {
                var path = PlacesToVisit.Dequeue();
                var node = path.Last();
                validMoves.Add(path);

                if (path.Count >= moves)
                {
                    continue; //don't go more if out of movement
                }

                var north = node + Vector3Int.up;
                var northTile = (DungeonTile)Dungeon.GetTile(north);
                if (northTile.Passable)
                {
                    //make sure we haven't already been here
                    if (!validMoves.Select(m => m.Last()).Contains(north))
                    {
                        var nextPlace = path.Select(m => new Vector3Int(m.x, m.y, m.z)).ToList();
                        nextPlace.Add(north);
                        PlacesToVisit.Enqueue(nextPlace);
                    }
                }

                var east = node + Vector3Int.right;
                var eastTile = (DungeonTile)Dungeon.GetTile(east);
                if (eastTile.Passable)
                {
                    //make sure we haven't already been here
                    if (!validMoves.Select(m => m.Last()).Contains(east))
                    {
                        var nextPlace = path.Select(m => new Vector3Int(m.x, m.y, m.z)).ToList();
                        nextPlace.Add(east);
                        PlacesToVisit.Enqueue(nextPlace);
                    }
                }

                var west = node + Vector3Int.left;
                var westTile = (DungeonTile)Dungeon.GetTile(west);
                if (westTile.Passable)
                {
                    //make sure we haven't already been here
                    if (!validMoves.Select(m => m.Last()).Contains(west))
                    {
                        var nextPlace = path.Select(m => new Vector3Int(m.x, m.y, m.z)).ToList();
                        nextPlace.Add(west);
                        PlacesToVisit.Enqueue(nextPlace);
                    }
                }

                var south = node + Vector3Int.down;
                var southTile = (DungeonTile)Dungeon.GetTile(south);
                if (southTile.Passable)
                {
                    //make sure we haven't already been here
                    if (!validMoves.Select(m => m.Last()).Contains(south))
                    {
                        var nextPlace = path.Select(m => new Vector3Int(m.x, m.y, m.z)).ToList();
                        nextPlace.Add(south);
                        PlacesToVisit.Enqueue(nextPlace);
                    }
                }
            }

            return validMoves;
        }

        private void ClearOverlays()
        {
            UIHighlights.ClearAllTiles();
        }

        private void RenderMovement(Unit unit)
        {
            var moves = FindAllValidMoves(unit).Select(m => m.Last());

            foreach (var move in moves)
            {
                UIHighlights.SetTile(move, MoveTile);
            }
        }

        private List<List<Vector3Int>> FindAllValidMoves(Unit unit)
        {
            return FindAllValidMoves(unit.Position, unit.CurrentMovement);
        }

        public void EndTurn()
        {
            //bad guys do stuff

            //new turn
            NewTurn();
        }

        private void NewTurn()
        {
            foreach (var go in PlayerUnits)
            {
                go.GetComponent<UnitController>().UnitRepresented.NewTurn();
            }
        }

        public void Ability1()
        {
            CurrentState = GameState.Targeting;
            ClearOverlays();
            UIHighlights.SetTile(unitClicked.GetComponent<UnitController>().UnitRepresented.Position, TargetingTile);
        }
    }
}