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
        //public Tilemap Dangerzones;
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
                if (UI != null)
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
            //push units who are hit in direction
            var destination = Vector3Int.FloorToInt(guyHit.transform.position) + direction;
            if (Passable(destination))
            {
                guyHit.moveTo(destination);
                var standingOnTile = (DungeonTile)Dungeon.GetTile(destination);
                if (standingOnTile.Slippery)
                {
                    //recursive call as it keeps sliding
                    KnockBack(guyHit, direction);
                }
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

            GameObject spawn = Instantiate(unitPrefab, this.transform);
            spawn.GetComponent<UnitController>().SetupUnit("Knight", UnitController.SideEnum.Player, new Vector3Int(-4, -1, 0));
            spawn = Instantiate(unitPrefab, this.transform);
            spawn.GetComponent<UnitController>().SetupUnit("Knight", UnitController.SideEnum.Player, new Vector3Int(-4, 0, 0));
            spawn = Instantiate(unitPrefab, this.transform);
            spawn.GetComponent<UnitController>().SetupUnit("Hireling", UnitController.SideEnum.Hireling, new Vector3Int(-5, 1, 0));
            spawn = Instantiate(unitPrefab, this.transform);
            spawn.GetComponent<UnitController>().SetupUnit("Hireling", UnitController.SideEnum.Hireling, new Vector3Int(-5, -3, 0));
            spawn = Instantiate(unitPrefab, this.transform);
            spawn.GetComponent<UnitController>().SetupUnit("Ooze", UnitController.SideEnum.BadGuy, new Vector3Int(-1, 1, 0));
            spawn = Instantiate(unitPrefab, this.transform);
            spawn.GetComponent<UnitController>().SetupUnit("Ooze", UnitController.SideEnum.BadGuy, new Vector3Int(1, 3, 0));




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
            if (MouseoverPoint != PreviousMouseoverPoint)
            {
                PreviousMouseoverPoint = MouseoverPoint;
                foreach (var unit in AllUnits)
                {
                    unit.DisableUI();
                }
                var unitUnderMouse = AllUnits.Find(u => u.transform.position == MouseoverPoint);
                var dungeonTileUnderMouse = (DungeonTile)Dungeon.GetTile(MouseoverPoint);
                unitUnderMouse.EnableUI();
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

                //TODO: refactor to use cardinal directions
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
            //TODO: different kinds of bad guy UI
            foreach (var badGuy in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.BadGuy))
            {
                foreach (var target in badGuy.TargetedTiles)
                {
                    var targetedUnit = AllUnits.Find(u => u.transform.position == target);
                    if (targetedUnit != null)
                    {
                        targetedUnit.TakeDamage(badGuy.Damage);
                    }
                }
                badGuy.ClearTargetOverlays();
            }
            //Dangerzones.ClearAllTiles();

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
            //TODO: different kinds of bad guy AI
            foreach (var bg in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.BadGuy))
            {
                var destinations = FindAllValidMoves(bg);
                List<Vector3Int> path = FindPathAdjacentToTarget(destinations);
                if (path != null)
                {
                    bg.moveTo(path.Last());
                    var target = FindAdjacentTarget(path.Last());
                    bg.TargetTile(target);
                }
                else
                {
                    // move to random location I guess
                    int i = UnityEngine.Random.Range(0, destinations.Count);
                    var move = destinations[i].Last();
                    bg.moveTo(move);
                }
            }
        }

        public static readonly ReadOnlyCollection<Vector3Int> CardinalDirections = new ReadOnlyCollection<Vector3Int>(new[] { Vector3Int.up, Vector3Int.right, Vector3Int.left, Vector3Int.down });

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

            if (targets.Count == 0)
            {
                return new Vector3Int(100, 100, 100);
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
                    if (targets.Any(u => u.transform.position == adjacentSquare))
                    {
                        validEndpoints.Add(destination);
                    }
                }
            }

            if (validEndpoints.Count == 0)
            {
                return null;
            }

            int i = UnityEngine.Random.Range(0, validEndpoints.Count);
            var path = destinations.Find(p => p.Last() == validEndpoints[i]);
            return path;
        }

        public void DisplaySelectedUnitData()
        {
            UI.DisplayUnitInfo(UnitClicked);
        }

        internal void RenderAbility(Ability ability)
        {
            //TODO: generalize, melee only
            switch (ability.Type)
            {
                case Ability.AbilityType.Melee:
                    foreach (var dir in GameManager.CardinalDirections)
                    {
                        UIHighlights.SetTile(Vector3Int.FloorToInt(UnitClicked.transform.position) + dir, TargetingTile);
                    }
                    break;

                default:
                    Debug.LogError("not implemented, TODO");
                    break;
            }

        }

        public void Ability1()
        {
            TriggerTransition(GameStateTransitions.TargetAbility);
        }
    }
}