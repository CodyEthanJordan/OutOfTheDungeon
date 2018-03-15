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
using System.Collections.ObjectModel;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public Tilemap Dungeon;
        public Tilemap UIHighlights;
        public Tilemap Dangerzones;
        public Tile MoveTile;
        public Tile TargetingTile;
        public Tile ThreatenedTile;

        public UnitController UnitClicked;
        public GameObject unitPrefab;

        public List<UnitController> AllUnits;

        public UIManager UI;

        private int _turnCounter = 0;
        private int turnCounter
        {
            get { return _turnCounter; }
            set
            {
                _turnCounter = value;
                if(UI != null)
                {
                    UI.UpdateTurn(_turnCounter);
                }
            }
        }

        private Vector3Int MouseoverPoint;
        private Vector3Int PreviousMouseoverPoint;

        private Animator TurnFSM;

        public UnitEvent UnitClickedEvent;
        public PositionEvent UIHighlightClickedEvent;

        internal void MoveUnit(UnitController unitController, Vector3Int destination)
        {
            unitController.CurrentMovement -= DistanceTo(Vector3Int.FloorToInt(unitController.transform.position), destination);
            unitController.moveTo(destination);
        }

        internal void ActivateAbility(UnitController unit, Vector3Int target)
        {
            //is the target a unit?
            var guyHit = AllUnits.Find(u => u.transform.position == target);
            if (guyHit != null)
            {
                guyHit.TakeDamage(1);
                var knockbackDirection = guyHit.transform.position - unit.transform.position;
                KnockBack(guyHit, Vector3Int.FloorToInt(knockbackDirection.normalized));
            }
        }

        private void KnockBack(UnitController guyHit, Vector3Int direction)
        {
            var destination = Vector3Int.FloorToInt(guyHit.transform.position) + direction;
            if (Passable(destination))
            {
                guyHit.transform.position = destination;
            }
            else
            {
                var unitInWay = AllUnits.Find(u => u.transform.position == destination);
                if (unitInWay != null)
                {
                    unitInWay.TakeDamage(1);
                    guyHit.TakeDamage(1);
                }
                else
                {
                    guyHit.TakeDamage(1);
                    //TODO: destroying tiles
                }
            }
        }


        // Use this for initialization
        void Awake()
        {
            TurnFSM = this.GetComponent<Animator>();

            AllUnits = new List<UnitController>();

            //TODO: horrible hack
            var knightObject = Instantiate(unitPrefab, 2 * Vector3.down, Quaternion.identity, this.transform);
            var knight = knightObject.GetComponent<UnitController>();
            knight.Side = UnitController.SideEnum.Player;
            AllUnits.Add(knight);
            var knightObject2 = Instantiate(unitPrefab, Vector3.up, Quaternion.identity, this.transform);
            knightObject2.GetComponent<SpriteRenderer>().color = Color.red;
            var knight2 = knightObject2.GetComponent<UnitController>();
            knight2.Side = UnitController.SideEnum.BadGuy;
            AllUnits.Add(knight2);



            UnitClickedEvent = new UnitEvent();
            UIHighlightClickedEvent = new PositionEvent();
        }

        private void Start()
        {
            turnCounter = -1;
            NewTurn();
        }

        public void TriggerTransition(string trigger)
        {
            TurnFSM.SetTrigger(trigger);
        }

        void Update()
        {
            //sort out mouseover stuff
            MouseoverPoint = Dungeon.WorldToCell(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if(MouseoverPoint != PreviousMouseoverPoint)
            {
                PreviousMouseoverPoint = MouseoverPoint;
                foreach (var unit in AllUnits)
                {
                    unit.DisableUI();
                }
                var unitUnderMouse = AllUnits.Find(u => u.transform.position == MouseoverPoint);
                var dungeonTileUnderMouse = (DungeonTile)Dungeon.GetTile(MouseoverPoint);

                UI.ShowMouseOverInfo(dungeonTileUnderMouse, unitUnderMouse);
            }


            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                TriggerTransition(GameStateTransitions.Deselect);
            }
            if (Input.GetMouseButtonDown(0))
            {
                RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                if (hit.collider != null)
                {
                    if (hit.collider.CompareTag("Unit"))
                    {
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
            if (!wall.Passable)
            {
                return false;
            }

            //TODO: pass through allies?
            if (AllUnits.Where(u => u.transform.position == pos).Count() > 0)
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
            RenderMovement(UnitClicked);
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
            TriggerTransition(GameStateTransitions.Deselect);
            //bad guys do stuff

            //new turn
            NewTurn();
        }

        private void NewTurn()
        {
            turnCounter = turnCounter + 1;

            foreach (var unit in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.Player))
            {
                unit.NewTurn();
            }

            //set up bad guy moves
            foreach (var bg in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.BadGuy))
            {
                var destinations = FindAllValidMoves(bg);
                List<Vector3Int> path = FindPathAdjacentToTarget(destinations);
                bg.moveTo(path.Last());
                var target = FindAdjacentTarget(path.Last());
                Dangerzones.SetTile(target, ThreatenedTile);
            }
        }

        public static readonly ReadOnlyCollection<Vector3Int> CardinalDirections = new ReadOnlyCollection<Vector3Int>(new[] {Vector3Int.up, Vector3Int.right, Vector3Int.left, Vector3Int.down });

        Vector3Int FindAdjacentTarget(Vector3Int position)
        {
            var targetUnits = AllUnits.FindAll(u => u.Side == UnitController.SideEnum.Player);
            List<Vector3Int> targets = new List<Vector3Int>();
            foreach (var direction in CardinalDirections)
            {
                var adjacentSquare = position + direction;
                if (targetUnits.Any(u => u.transform.position == adjacentSquare))
                {
                    targets.Add(adjacentSquare);
                }
            }

            int i = UnityEngine.Random.Range(0, targets.Count);
            return targets[i];
        }

        private List<Vector3Int> FindPathAdjacentToTarget(List<List<Vector3Int>> destinations)
        {
            var ends = destinations.Select(p => p.Last());
            List<Vector3Int> validEndpoints = new List<Vector3Int>();
            var targets = AllUnits.FindAll(u => u.Side == UnitController.SideEnum.Player);
            foreach (var destination in ends)
            {
                foreach (var direction in CardinalDirections)
                {
                    var adjacentSquare = destination + direction;
                    if(targets.Any(u => u.transform.position == adjacentSquare))
                    {
                        validEndpoints.Add(destination);
                    }
                }
            }

            int i = UnityEngine.Random.Range(0, validEndpoints.Count);
            var path = destinations.Find(p => p.Last() == validEndpoints[i]);
            return path;
        }

        public void DisplaySelectedUnitData()
        {
            UI.DisplayUnitInfo(UnitClicked);
        }

        internal void RenderAbility()
        {
            UIHighlights.SetTile(Vector3Int.FloorToInt(UnitClicked.transform.position) + Vector3Int.up, TargetingTile);
        }

        public void Ability1()
        {
            TriggerTransition(GameStateTransitions.TargetAbility);
        }
    }
}