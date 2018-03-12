using Assets.Scripts.GameLogic;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Tilemap Dungeon;
        public Tilemap UIHighlights;
        public TileBase MoveTile;

        private GameObject unitClicked;

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
                                RenderMovement(hit.collider.gameObject);
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
                                unitClicked.GetComponent<UnitController>().moveTo(destination);
                                CurrentState = GameState.BaseState;
                                ClearOverlays();
                            }
                        }
                        else
                        {
                            CurrentState = GameState.BaseState;
                            ClearOverlays();
                        }
                        break;
                    default:
                        break;

                }

            }

        }

        private void ClearOverlays()
        {
            UIHighlights.ClearAllTiles();
        }

        private void RenderMovement(GameObject clickedObject)
        {
            var unit = clickedObject.GetComponent<UnitController>().UnitRepresented;
            var moves = FindAllValidMoves(unit);

            foreach (var move in moves)
            {
                UIHighlights.SetTile(move, MoveTile);
            }
        }

        private List<Vector3Int> FindAllValidMoves(Unit unit)
        {
            var validMoves = new List<List<Vector3Int>>();
            var startingPath = new List<Vector3Int>() { unit.Position };
            Queue<List<Vector3Int>> PlacesToVisit = new Queue<List<Vector3Int>>();
            PlacesToVisit.Enqueue(new List<Vector3Int>() { unit.Position });

            while (PlacesToVisit.Count > 0)
            {
                var path = PlacesToVisit.Dequeue();
                var node = path.Last();
                validMoves.Add(path);

                if (path.Count >= unit.Movement)
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

            return validMoves.Select(m => m.Last()).ToList();
        }
    }
}