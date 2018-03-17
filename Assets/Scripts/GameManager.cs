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
        private GridInformation dungeonInfo;
        public Tilemap UIHighlights;
        //public Tilemap Dangerzones;
        public Tile MoveTile;
        public Tile TargetingTile;
        public Tile ThreatenedTile;

        public UnitController UnitClicked;
        public GameObject unitPrefab;

        private int _remainingHirelings;
        private int remainingHirelings
        {
            get { return _remainingHirelings; }
            set
            {
                _remainingHirelings = value;
                HirelingSpawned.Invoke(_remainingHirelings);
            }
        }
        public IntEvent HirelingSpawned = new IntEvent();
        private int _savedHirelings = 0;
        private int savedHirelings
        {
            get { return _savedHirelings; }
            set
            {
                _savedHirelings = value;
                HirelingSaved.Invoke(_savedHirelings);
            }
        }
        public IntEvent HirelingSaved = new IntEvent();

        public List<UnitController> AllUnits;

        private List<Vector3Int> entranceLocations;

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

        private bool blockInputs = false;
        private float MOVEMENT_SPEED = 0.1f;

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
                else
                {
                    ApplyTileEffects(guyHit, Vector3Int.FloorToInt(guyHit.transform.position));
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

        void Awake()
        {
            TurnFSM = this.GetComponent<Animator>();
            dungeonInfo = Dungeon.gameObject.GetComponent<GridInformation>();
            AllUnits = new List<UnitController>();
            UnitClickedEvent = new UnitEvent();
            UIHighlightClickedEvent = new PositionEvent();

        }

        private void Start()
        {
            savedHirelings = 0;
            remainingHirelings = 6;
            entranceLocations = new List<Vector3Int>();
            for (int i = Dungeon.cellBounds.xMin; i < Dungeon.cellBounds.xMax; i++)
            {
                for (int j = Dungeon.cellBounds.yMin; j < Dungeon.cellBounds.yMax; j++)
                {
                    Vector3Int pos = new Vector3Int(i, j, (int)Dungeon.transform.position.z);
                    if (Dungeon.HasTile(pos))
                    {
                        var tile = Dungeon.GetTile<DungeonTile>(pos);
                        if (tile.Name == "Entrance")
                        {
                            entranceLocations.Add(pos);
                        }
                    }
                }
            }

            foreach (var entrance in entranceLocations)
            {
                var pathToExit = new List<Vector3Int>();
                pathToExit.Add(entrance);
                bool found = false;
                while (!found)
                {
                    foreach (var dir in GameManager.CardinalDirections)
                    {
                        var newLocation = pathToExit.Last() + dir;
                        if(pathToExit.Contains(newLocation))
                        {
                            //already been here
                            continue;
                        }

                        var nextTile = Dungeon.GetTile<DungeonTile>(newLocation);
                        if(nextTile.Name == "Level Exit")
                        {
                            found = true;
                            dungeonInfo.SetPositionProperty(pathToExit.Last(), "RoadDirection", GameManager.CardinalDirections.IndexOf(dir));
                            break;
                        }
                        else if(nextTile.Name == "Road")
                        {
                            dungeonInfo.SetPositionProperty(pathToExit.Last(), "RoadDirection", GameManager.CardinalDirections.IndexOf(dir));
                            pathToExit.Add(newLocation); //found the next road segment
                            break;
                        }
                    }

                }
            }
            //TODO: horrible hack
            SpawnUnit(new Vector3Int(-4, 0, 0), "Knight", UnitController.SideEnum.Player);
            SpawnUnit(new Vector3Int(0, 0, 0), "Ooze", UnitController.SideEnum.BadGuy);
            SpawnUnit(new Vector3Int(0, -2, 0), "Ooze", UnitController.SideEnum.BadGuy);
            SpawnUnit(new Vector3Int(-5, 1, 0), "Hireling", UnitController.SideEnum.Hireling);
            turnCounter = -1;
            NewTurn();
            remainingHirelings = 6;
        }

        private void SpawnUnit(Vector3Int position, string name, UnitController.SideEnum side)
        {
            UnitController spawnedUnit;
            GameObject spawn = Instantiate(unitPrefab, this.transform);
            spawnedUnit = spawn.GetComponent<UnitController>();
            spawnedUnit.SetupUnit(name, side, position);
            AllUnits.Add(spawnedUnit);
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
                if (unitUnderMouse != null)
                {
                    unitUnderMouse.EnableUI();
                }
                UI.ShowMouseOverInfo(dungeonTileUnderMouse, unitUnderMouse);
            }

            if (!blockInputs)
            {

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

        public void MoveUnit(UnitController unit, Vector3Int destination)
        {
            //TODO: animate?, also unify with other one
            unit.CurrentMovement -= DistanceTo(Vector3Int.FloorToInt(unit.transform.position), destination);
            unit.moveTo(destination);
            ApplyTileEffects(unit, destination);
        }

        private void MoveUnit(UnitController unit, List<Vector3Int> path, float animationSpeed)
        {
            blockInputs = true;
            unit.Move(path, animationSpeed, () => blockInputs = false);
            // apply tile effects
            var destination = path.Last();
            ApplyTileEffects(unit, destination);
            blockInputs = false;
        }

        private void ApplyTileEffects(UnitController unit, Vector3Int positon)
        {
            var tileLandedOn = (DungeonTile)Dungeon.GetTile(positon);

            if (tileLandedOn.Deadly)
            {
                Kill(unit);
            }
        }

        private void Kill(UnitController unit)
        {
            AllUnits.Remove(unit);
            unit.Die();
            //TODO: victory/loss conditions if no Players exist
        }

        private void NewTurn()
        {
            turnCounter = turnCounter + 1;

            foreach (var unit in AllUnits)
            {
                unit.NewTurn();
            }

            //move hirlings
            foreach (var hireling in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.Hireling))
            {
                while(hireling.CurrentMovement > 0)
                {
                    Vector3Int startingPos = Vector3Int.FloorToInt(hireling.transform.position);
                    var tile = Dungeon.GetTile<DungeonTile>(startingPos);
                    if (tile.Name == "Road")
                    {
                        var nextPosition = startingPos + GameManager.CardinalDirections[dungeonInfo.GetPositionProperty(startingPos, "RoadDirection", -1)];
                        var nextTile = Dungeon.GetTile<DungeonTile>(nextPosition);
                        if(nextTile.Name == "Level Exit")
                        {
                            Kill(hireling);
                            savedHirelings++;
                            break;
                            //TODO: register as having made it
                        }
                        MoveUnit(hireling, nextPosition);
                    }
                }
            }



            //set up bad guy moves
            //TODO: different kinds of bad guy AI
            foreach (var bg in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.BadGuy))
            {
                var destinations = FindAllValidMoves(bg);
                List<Vector3Int> path = FindPathAdjacentToTarget(destinations, (UnitController u) => u.Side == UnitController.SideEnum.Hireling || u.Side == UnitController.SideEnum.Player);
                if (path != null)
                {
                    //MoveUnit(bg, path, MOVEMENT_SPEED);
                    MoveUnit(bg, path.Last());
                    var target = FindAdjacentTarget(path.Last(), (UnitController u) => u.Side == UnitController.SideEnum.Hireling || u.Side == UnitController.SideEnum.Player);
                    bg.TargetTile(target);
                }
                else
                {
                    // move to random location I guess
                    int i = UnityEngine.Random.Range(0, destinations.Count);
                    var move = destinations[i].Last();
                    //MoveUnit(bg, destinations[i], MOVEMENT_SPEED);
                    //bg.moveTo(move);
                    MoveUnit(bg, move);
                }
            }
        }

        public static readonly ReadOnlyCollection<Vector3Int> CardinalDirections = new ReadOnlyCollection<Vector3Int>(new[] { Vector3Int.up, Vector3Int.right, Vector3Int.left, Vector3Int.down });

        Vector3Int FindAdjacentTarget(Vector3Int position, Predicate<UnitController> targetPredicate)
        {
            var targetUnits = AllUnits.FindAll(targetPredicate);
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

        private List<Vector3Int> FindPathAdjacentToTarget(List<List<Vector3Int>> destinations, Predicate<UnitController> targetPredicate)
        {
            var ends = destinations.Select(p => p.Last());
            List<Vector3Int> validEndpoints = new List<Vector3Int>();
            var targets = AllUnits.FindAll(targetPredicate);
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