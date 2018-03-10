using Assets.Scripts.GameLogic;
using System;
using System.Collections;
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
                            if(hit.collider.CompareTag("UI"))
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
            var validMoves = new List<Vector3Int>();

            RecursiveFloodFillMovement(unit.Movement, unit.Position, Dungeon, validMoves);

            return validMoves;
        }

        private void RecursiveFloodFillMovement(int movement, Vector3Int pos, Tilemap dungeon, List<Vector3Int> validMoves)
        {
            if(movement <= 0)
            {
                return;
            }
            //check neighbors
            var north = pos + Vector3Int.up;
            var northTile = (DungeonTile)Dungeon.GetTile(north);
            if(northTile.Passable && !validMoves.Contains(north))
            {
                validMoves.Add(north);
                RecursiveFloodFillMovement(movement - 1, north, Dungeon, validMoves);
            }

            var east = pos + Vector3Int.right;
            var eastTile = (DungeonTile)Dungeon.GetTile(east);
            if (eastTile.Passable && !validMoves.Contains(east))
            {
                validMoves.Add(east);
                RecursiveFloodFillMovement(movement - 1, east, Dungeon, validMoves);
            }

            var west = pos + Vector3Int.left;
            var westTile = (DungeonTile)Dungeon.GetTile(west);
            if (westTile.Passable && !validMoves.Contains(west))
            {
                validMoves.Add(west);
                RecursiveFloodFillMovement(movement - 1, west, Dungeon, validMoves);
            }

            var south = pos + Vector3Int.down;
            var southTile = (DungeonTile)Dungeon.GetTile(south);
            if (southTile.Passable && !validMoves.Contains(south))
            {
                validMoves.Add(south);
                RecursiveFloodFillMovement(movement - 1, south, Dungeon, validMoves);
            }
        }
    }
}