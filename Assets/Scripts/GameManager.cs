using Assets.Scripts.GameLogic;
using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
using Assets.Scripts.Events;
using UnityEngine.Events;
using Assets.Scripts.FSM;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Tilemap Dungeon;
        public Tilemap UIHighlights;
        public Tile MoveTile;
        public Tile TargetingTile;

        public GameObject UnitClicked;
        public GameObject unitPrefab;

        public List<UnitController> PlayerUnits;
        public List<UnitController> AllUnits;

        public UIManager UI;

        private Animator TurnFSM;

        public UnitEvent UnitClickedEvent;
        public PositionEvent UIHighlightClickedEvent;

        internal void MoveUnit(UnitController unitController, Vector3Int destination)
        {
            unitController.CurrentMovement -= DistanceTo(Vector3Int.FloorToInt(unitController.transform.position), destination);
            unitController.moveTo(destination);
        }

     

        internal void ActivateAbility(GameObject unitClicked, Vector3Int target)
        {
            //is the target a unit?
            var unit = unitClicked.GetComponent<UnitController>();
            var guyHit = AllUnits.Find(u => u.transform.position == target);
            if(guyHit != null)
            {
                guyHit.TakeDamage(1);
                KnockBack(guyHit);
            }
        }

        private void KnockBack(UnitController guyHit)
        {
            throw new NotImplementedException();
        }


        // Use this for initialization
        void Awake()
        {
            TurnFSM = this.GetComponent<Animator>();

            PlayerUnits = new List<UnitController>();
            AllUnits = new List<UnitController>();


            //TODO: horrible hack
            var knightObject = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity, this.transform);
            var knight = knightObject.GetComponent<UnitController>();
            PlayerUnits.Add(knight.GetComponent<UnitController>());
            AllUnits.Add(knight);
            var knightObject2 = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity, this.transform);
            var knight2 = knightObject2.GetComponent<UnitController>();
            PlayerUnits.Add(knight2.GetComponent<UnitController>());
            AllUnits.Add(knight2);



            UnitClickedEvent = new UnitEvent();
            UIHighlightClickedEvent = new PositionEvent();
        }

        public void TriggerTransition(string trigger)
        {
            TurnFSM.SetTrigger(trigger);
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Unit"))
                    {
                        UnitClicked = hit.collider.gameObject;
                        UnitClickedEvent.Invoke(hit.collider.GetComponent<UnitController>());
                    }
                    else if (hit.collider.CompareTag("UIHighlights"))
                    {
                        var destination = Dungeon.WorldToCell(hit.point);
                        UIHighlightClickedEvent.Invoke(destination);
                    }
                }
                //switch (CurrentState)
                //{
                //    case GameState.BaseState:
                //        if (hit.collider != null)
                //        {
                //            if (hit.collider.CompareTag("Unit"))
                //            {
                //                CurrentState = GameState.UnitSelected;
                //                TurnFSM.SetTrigger("UnitClicked");
                //                unitClicked = hit.collider.gameObject;
                //                var unit = unitClicked.GetComponent<UnitController>().UnitRepresented;
                //                RenderMovement(unit);
                //                ActivateButtons(unit);
                //            }
                //        }
                //        break;
                //    case GameState.UnitSelected:
                //        if (hit.collider != null)
                //        {
                //            if (hit.collider.CompareTag("UI"))
                //            {
                //                //move unit
                //                var destination = Dungeon.WorldToCell(hit.point);
                //                var unitController = unitClicked.GetComponent<UnitController>();
                //                unitController.UnitRepresented.CurrentMovement -= DistanceTo(unitController.UnitRepresented.Position, destination);
                //                unitController.moveTo(destination);
                //                CurrentState = GameState.BaseState;
                //                TurnFSM.SetTrigger("Deselect");
                //                ClearOverlays();
                //                DeselectUnit();
                //            }
                //        }
                //        else
                //        {
                //            //CurrentState = GameState.BaseState;
                //            //ClearOverlays();
                //            //DeselectUnit();
                //        }
                //        break;
                //    default:
                //        break;

                //}

            }

        }

        public void DeselectUnit()
        {
            UnitClicked = null;
            UI.HideUnitInfo();
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

        public bool Passable(Vector3Int pos)
        {
            var wall = (DungeonTile)Dungeon.GetTile(pos);
            if(!wall.Passable)
            {
                return false;
            }

            //TODO: pass through allies?
            if(AllUnits.Where(u => u.transform.position == pos).Count() > 0)
            {
                return false;
            }

            return true;
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
                if (Passable(north))
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
                if (Passable(east))
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
                if (Passable(west))
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
                if (Passable(south))
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

        public void ClearOverlays()
        {
            UIHighlights.ClearAllTiles();
        }

        public void RenderMovement()
        {
            RenderMovement(UnitClicked.GetComponent<UnitController>());
        }

        private void RenderMovement(UnitController unit)
        {
            var moves = FindAllValidMoves(unit).Select(m => m.Last());

            foreach (var move in moves)
            {
                UIHighlights.SetTile(move, MoveTile);
            }
        }

        private List<List<Vector3Int>> FindAllValidMoves(UnitController unit)
        {
            return FindAllValidMoves(Vector3Int.FloorToInt(unit.transform.position), unit.CurrentMovement);
        }

        public void EndTurn()
        {
            //bad guys do stuff

            //new turn
            NewTurn();
        }

        private void NewTurn()
        {
            foreach (var unit in PlayerUnits)
            {
                unit.NewTurn();
            }
        }

        public void DisplaySelectedUnitData()
        {
            UI.DisplayUnitInfo(UnitClicked.GetComponent<UnitController>());
        }

        internal void RenderAbility()
        {
            UIHighlights.SetTile(Vector3Int.FloorToInt(UnitClicked.GetComponent<UnitController>().transform.position) + Vector3Int.up, TargetingTile);
        }

        public void Ability1()
        {
            TriggerTransition(GameStateTransitions.TargetAbility);
        }
    }
}