using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {

        public Tilemap UIHighlights;
        public TileBase MoveTile;

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
                                RenderMovement(hit.collider.gameObject);
                            }
                        }
                        break;
                    case GameState.UnitSelected:
                        if (hit.collider != null)
                        {
                            //try movement
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
            var pos = new Vector3Int(unit.Position.x, unit.Position.y, 0);
            UIHighlights.SetTile(pos, MoveTile);
        }

        private List<Vector3Int> FindAllValidMoves()
        {
            return null;
        }
    }
}