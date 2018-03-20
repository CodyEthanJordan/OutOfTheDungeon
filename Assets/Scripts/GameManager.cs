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
using Assets.Scripts.UI;

namespace Assets.Scripts
{
    public class GameManager : MonoBehaviour
    {
        public float MovementAnimationSpeed;
        public Tilemap Dungeon;
        private GridInformation dungeonInfo;
        public Tilemap UIHighlights;
        //public Tilemap Dangerzones;
        public Tile MoveTile;
        public Tile TargetingTile;
        public Tile ThreatenedTile;
        public Loadout[] BadGuyLoadouts;

        public UnitController UnitClicked;
        public GameObject UnitPrefab;
        public GameObject SummoningPortalPrefab;

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
                Debug.Log("Hireling reached the exit");
                HirelingSaved.Invoke(_savedHirelings);
            }
        }
        public IntEvent HirelingSaved = new IntEvent();

        public List<UnitController> AllUnits;

        private List<Vector3Int> entranceLocations = new List<Vector3Int>();
        private List<Vector3Int> validSpawnLocations = new List<Vector3Int>();
        private List<GameObject> summoningCircles = new List<GameObject>();

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

        public UnitEvent UnitClickedEvent = new UnitEvent();
        public PositionEvent UIHighlightClickedEvent = new PositionEvent();

        private bool blockInputs = false;
        public int SelectedAbilityIndex { get; private set; }

        internal void ActivateAbility(UnitController unit, Ability ability, Vector3Int target)
        {
            Debug.Log(unit.Name + " uses " + ability.Name + " at " + target);
            if (unit.HasActed)
            {
                //already acted, ignore this
                Debug.LogWarning(unit.Name + "has already acted");
                return;
            }
            else
            {
                unit.HasActed = true;
                unit.CurrentMovement = 0;
            }

            ability.ApplyEffects(this, unit, target);


            //TODO: damage to tiles


        }

        public void KnockBack(UnitController guyHit, Vector3Int direction)
        {
            //push units who are hit in direction
            var destination = Vector3Int.FloorToInt(guyHit.transform.position) + direction;
            if (Passable(destination, true))
            {
                guyHit.moveTo(destination);
                var standingOnTile = (DungeonTile)Dungeon.GetTile(destination);
                if (standingOnTile.Slippery)
                {
                    //recursive call as it keeps sliding
                    Debug.Log(guyHit.Name + " keeps sliding along " + standingOnTile.Name);
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
                    Debug.Log(guyHit.Name + " crashes into " + unitInWay.Name);
                    unitInWay.TakeDamage(1, Effect.DamageTypes.KnockbackImpact);
                    guyHit.TakeDamage(1, Effect.DamageTypes.KnockbackImpact);
                }
                else
                {
                    var tile = Dungeon.GetTile<DungeonTile>(destination);

                    if (tile.Name == "Level Exit" && guyHit.Side == UnitController.SideEnum.Hireling)
                    {
                        Debug.Log(guyHit.Name + " knocked onto the stairs!");
                        savedHirelings++;
                        guyHit.Die();
                    }
                    else
                    {
                        Debug.Log(guyHit.Name + " hits something solid");
                        guyHit.TakeDamage(1, Effect.DamageTypes.KnockbackImpact);
                    }

                    //TODO: destroying tiles
                }
            }
        }

        void Awake()
        {
            TurnFSM = this.GetComponent<Animator>();
            dungeonInfo = Dungeon.gameObject.GetComponent<GridInformation>();
            AllUnits = new List<UnitController>();
        }

        private void SetupMap(int hirelings)
        {
            TurnFSM.SetTrigger(GameStateTransitions.Deselect);
            UI.InitializeUI();
            savedHirelings = 0;
            remainingHirelings = hirelings;

            foreach (var unit in AllUnits)
            {
                Destroy(unit.gameObject);
            }
            AllUnits.Clear();

            foreach (var summoningCircle in summoningCircles)
            {
                Destroy(summoningCircle.gameObject);
            }
            summoningCircles.Clear();

            ClearOverlays();

            SpawnUnit(new Vector3Int(-4, 0, 0), "Knight", UnitController.SideEnum.Player, "Knight");
            SpawnUnit(new Vector3Int(-3, 0, 0), "Gandalf", UnitController.SideEnum.Player, "Wizard");
            SpawnUnit(new Vector3Int(-4, -1, 0), "Knight", UnitController.SideEnum.Player, "Knight");
            SpawnUnit(new Vector3Int(0, 0, 0), "Goblin Archer", UnitController.SideEnum.BadGuy, "Goblin Archer");
            SpawnUnit(new Vector3Int(0, -2, 0), "Ooze", UnitController.SideEnum.BadGuy, "Ooze");
            turnCounter = 0;
            blockInputs = false;
            StartCoroutine(NewTurn());
        }

        private void Start()
        {
            savedHirelings = 0;
            remainingHirelings = 6;

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
                        else if (tile.Name == "Floor")
                        {
                            validSpawnLocations.Add(pos);
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
                        if (pathToExit.Contains(newLocation))
                        {
                            //already been here
                            continue;
                        }

                        var nextTile = Dungeon.GetTile<DungeonTile>(newLocation);
                        if (nextTile.Name == "Level Exit")
                        {
                            found = true;
                            dungeonInfo.SetPositionProperty(pathToExit.Last(), "RoadDirection", GameManager.CardinalDirections.IndexOf(dir));
                            break;
                        }
                        else if (nextTile.Name == "Road")
                        {
                            dungeonInfo.SetPositionProperty(pathToExit.Last(), "RoadDirection", GameManager.CardinalDirections.IndexOf(dir));
                            pathToExit.Add(newLocation); //found the next road segment
                            break;
                        }
                    }

                }
            }
            //TODO: horrible hack
            SetupMap(6);
        }

        private void SpawnUnit(Vector3Int position, string name, UnitController.SideEnum side, string loadoutName)
        {
            Debug.Log(name + "the " + loadoutName + " spawned at " + position + ", fighting for" + side);
            UnitController spawnedUnit;
            GameObject spawn = Instantiate(UnitPrefab, this.transform);
            spawnedUnit = spawn.GetComponent<UnitController>();
            spawnedUnit.SetupUnit(name, side, position, loadoutName);
            spawnedUnit.DeathEvent.AddListener(OnUnitDie);
            AllUnits.Add(spawnedUnit);
        }

        private void CheckVictoryConditions()
        {
            if (AllUnits.Where(u => u.Side == UnitController.SideEnum.Player).Count() == 0)
            {
                //everyone dead
                EndGame(victory: false);
            }
            else if (AllUnits.Where(u => u.Side == UnitController.SideEnum.Hireling).Count() == 0 && remainingHirelings == 0)
            {
                //all hirelings made it
                EndGame(victory: true);
            }
        }

        private void EndGame(bool victory)
        {
            blockInputs = true;
            UI.GameOver(victory, savedHirelings);
            StartCoroutine(RestartGame(3));
        }

        IEnumerator RestartGame(int secondsTillRestart)
        {
            yield return new WaitForSeconds(secondsTillRestart);
            SetupMap(6);
        }

        private void OnUnitDie(UnitController unit)
        {
            AllUnits.Remove(unit);
            CheckVictoryConditions();
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
                //hot keyboard shortcuts
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    TurnFSM.SetTrigger(GameStateTransitions.Deselect);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    if (UnitClicked != null && UnitClicked.MyLoadout.Abilities.Length > 0)
                    {
                        AbilityButtonClick(0);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    if (UnitClicked != null && UnitClicked.MyLoadout.Abilities.Length > 1)
                    {
                        AbilityButtonClick(1);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    if (UnitClicked != null && UnitClicked.MyLoadout.Abilities.Length > 2)
                    {
                        AbilityButtonClick(2);
                    }
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
                            //TODO: don't hardcode to first ability
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

        //private int DistanceTo(Vector3Int position, Vector3Int destination)
        //{
        //    //TODO horrible hack
        //    var moves = FindAllValidMoves(position, 20, destination);
        //    var validPath = moves.FirstOrDefault(m => m.Last() == destination);
        //    if (validPath != null)
        //    {
        //        return validPath.Count() - 1;
        //    }
        //    else
        //    {
        //        return -1;
        //    }
        //}

        public bool Passable(Vector3Int pos, bool blockedByUnits, Func<UnitController, bool> unitBlockingPredicate = null)
        {
            var wall = (DungeonTile)Dungeon.GetTile(pos);
            if (!wall.Passable)
            {
                return false;
            }

            if (blockedByUnits)
            {
                if (unitBlockingPredicate == null)
                {
                    //default predicate, blocked by all
                    if (AllUnits.Where(u => u.transform.position == pos).Count() > 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if (AllUnits.Where(u => u.transform.position == pos && unitBlockingPredicate(u)).Count() > 0)
                    {
                        return false;
                    }
                }
            }


            return true;
        }

        private List<List<Vector3Int>> FindAllValidMoves(Vector3Int position, int moves, bool blockedByUnits, Func<UnitController, bool> unitBlockingPredicate = null)
        {
            return FindAllValidMoves(position, moves, blockedByUnits, unitBlockingPredicate, new Vector3Int(10000, 10000, 1000));
        }

        private List<List<Vector3Int>> FindAllValidMoves(Vector3Int position, int moves, bool blockByUnits, Func<UnitController, bool> unitBlockingPredicate, Vector3Int shortCircuitGoalDesination)
        {
            var validMoves = new List<List<Vector3Int>>();
            Queue<List<Vector3Int>> PathsToCheck = new Queue<List<Vector3Int>>();
            PathsToCheck.Enqueue(new List<Vector3Int>() { position });

            while (PathsToCheck.Count > 0)
            {
                var path = PathsToCheck.Dequeue();
                var node = path.Last();
                validMoves.Add(path);

                if (node == shortCircuitGoalDesination)
                {
                    //found shortest target path, need no others
                    return new List<List<Vector3Int>>() { path };
                }

                if (path.Count > moves)
                {
                    continue; //don't go more if out of movement
                }

                foreach (var dir in GameManager.CardinalDirections)
                {
                    var nextPlace = node + dir;
                    if (Passable(nextPlace, blockByUnits, unitBlockingPredicate))
                    {
                        if (!validMoves.Select(m => m.Last()).Contains(nextPlace))
                        {
                            var copyPath = path.Select(m => new Vector3Int(m.x, m.y, m.z)).ToList();
                            copyPath.Add(nextPlace);
                            PathsToCheck.Enqueue(copyPath);
                        }
                    }
                }
            }

            //remove tiles which can't be landed on even if they can be passed through
            validMoves.RemoveAll(p => !Passable(p.Last(), true));
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
            ClearOverlays();
            var moves = FindAllValidSteps(unit).Select(m => m.Last());

            foreach (var move in moves)
            {
                UIHighlights.SetTile(move, MoveTile);
            }
        }

        private List<List<Vector3Int>> FindAllValidSteps(UnitController unit)
        {
            return FindAllValidMoves(Vector3Int.FloorToInt(unit.transform.position), unit.CurrentMovement, true, (UnitController u) => u.Side != unit.Side);
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
                        //TODO: horrible hack, make delegate pattern
                        targetedUnit.TakeDamage(badGuy.MyLoadout.Abilities[0].Effects[0].Damage, Effect.DamageTypes.Iron);
                    }
                }
                badGuy.ClearTargetOverlays();
            }

            StartCoroutine(NewTurn());
        }

        public IEnumerator MoveUnit(UnitController unit, List<Vector3Int> path, bool costsMovement)
        {
            blockInputs = true;
            yield return unit.Move(path, MovementAnimationSpeed);
            if (costsMovement)
            {
                unit.CurrentMovement -= path.Count() - 1;
            }
            var destination = path.Last();
            ApplyTileEffects(unit, destination);
            blockInputs = false;
        }

        public void MoveUnit(UnitController unit, Vector3Int destination, bool costsMovement)
        {
            var allPaths = FindAllValidMoves(Vector3Int.FloorToInt(unit.transform.position), 20, true,
                (UnitController u) => u.Side != unit.Side,
                destination);
            var path = allPaths.Find(p => p.Last() == destination);
            StartCoroutine(MoveUnit(unit, path, costsMovement));
        }



        private void ApplyTileEffects(UnitController unit, Vector3Int positon)
        {
            var tileLandedOn = (DungeonTile)Dungeon.GetTile(positon);

            if (tileLandedOn.Deadly)
            {
                Debug.Log(unit.Name + " stepped on a deadly " + tileLandedOn.Name);
                Kill(unit);
            }
        }

        private void Kill(UnitController unit)
        {
            unit.Die();
        }

        private IEnumerator NewTurn()
        {
            blockInputs = true;
            turnCounter = turnCounter + 1;
            Debug.Log("Turn: " + turnCounter + "\n---------");

            foreach (var unit in AllUnits)
            {
                unit.NewTurn();
            }

            //move hirlings
            foreach (var hireling in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.Hireling))
            {
                while (hireling.CurrentMovement > 0)
                {
                    Vector3Int startingPos = Vector3Int.FloorToInt(hireling.transform.position);
                    var tile = Dungeon.GetTile<DungeonTile>(startingPos);
                    if (tile.Name == "Road")
                    {
                        var nextPosition = startingPos + GameManager.CardinalDirections[dungeonInfo.GetPositionProperty(startingPos, "RoadDirection", -1)];
                        var nextTile = Dungeon.GetTile<DungeonTile>(nextPosition);
                        if (nextTile.Name == "Level Exit")
                        {
                            Kill(hireling);
                            savedHirelings++;
                            CheckVictoryConditions();
                            break;
                            //TODO: register as having made it
                        }

                        if (Passable(nextPosition, true))
                        {
                            List<Vector3Int> path = new List<Vector3Int>()
                            {
                                Vector3Int.FloorToInt(hireling.transform.position),
                                nextPosition
                            };
                            yield return MoveUnit(hireling, path, true);
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        //got knocke off path, stand still and panic
                        break;
                    }
                }
            }

            //spawn more hirelings
            foreach (var entrance in entranceLocations)
            {
                Vector3Int spawnLocation = Vector3Int.zero;
                bool validSpawnFound = false;
                foreach (var dir in GameManager.CardinalDirections)
                {
                    spawnLocation = entrance + dir;
                    if (Passable(spawnLocation, true))
                    {
                        validSpawnFound = true;
                        break;
                    }
                }

                if (validSpawnFound && remainingHirelings > 0)
                {
                    SpawnUnit(spawnLocation, "Hireling", UnitController.SideEnum.Hireling, "Hireling");
                    remainingHirelings = remainingHirelings - 1;
                }
            }

            //set up bad guy moves
            //TODO: different kinds of bad guy AI
            foreach (var bg in AllUnits.FindAll(u => u.Side == UnitController.SideEnum.BadGuy))
            {
                List<Vector3Int> path = new List<Vector3Int>();
                var destinations = FindAllValidSteps(bg);
                var attackRange = bg.MyLoadout.Abilities[0].Range;
                if (attackRange == Ability.RangeType.Melee)
                {
                    path = FindPathAdjacentToTarget(destinations, (UnitController u) => u.Side == UnitController.SideEnum.Hireling || u.Side == UnitController.SideEnum.Player);
                    if (path != null)
                    {
                        yield return MoveUnit(bg, path, true);
                        var target = FindAdjacentTarget(path.Last(), (UnitController u) => u.Side == UnitController.SideEnum.Hireling || u.Side == UnitController.SideEnum.Player);
                        bg.TargetTile(target);
                    }
                    else
                    {
                        // move to random location I guess
                        int i = UnityEngine.Random.Range(0, destinations.Count);
                        yield return MoveUnit(bg, destinations[i], true);
                    }
                }
                else if (attackRange == Ability.RangeType.Ray)
                {
                    path = FindPathToCriteria(destinations, p => HasCleanShotFromPositionOnTarget(p, u => u.Side != UnitController.SideEnum.BadGuy));
                    if (path != null)
                    {
                        yield return MoveUnit(bg, path, true);
                        var target = FindAdjacentTarget(path.Last(), (UnitController u) => u.Side == UnitController.SideEnum.Hireling || u.Side == UnitController.SideEnum.Player);
                        bg.TargetDirection(this, Vector3Int.left);
                    }
                    else
                    {
                        // move to random location I guess
                        int i = UnityEngine.Random.Range(0, destinations.Count);
                        yield return MoveUnit(bg, destinations[i], true);
                    }
                }

                //execute move

            }

            //spawn more bad guys
            List<GameObject> circlesToDestroy = new List<GameObject>();
            foreach (var summoningCircle in summoningCircles)
            {
                var unitOnCircle = AllUnits.Find(u => u.transform.position == summoningCircle.transform.position);
                if (unitOnCircle == null)
                {
                    circlesToDestroy.Add(summoningCircle);
                    int r = UnityEngine.Random.Range(0, BadGuyLoadouts.Length);
                    var guyToSpawn = BadGuyLoadouts[r];
                    SpawnUnit(Vector3Int.FloorToInt(summoningCircle.transform.position),
                        guyToSpawn.LoadoutName,
                        UnitController.SideEnum.BadGuy,
                        guyToSpawn.LoadoutName);
                }
                else
                {
                    Debug.Log("Summoning circle at " + summoningCircle.transform.position + " blocked by " + unitOnCircle.Name);
                    unitOnCircle.TakeDamage(1, Effect.DamageTypes.SummoningBlocked);
                }
            }
            for (int i = 0; i < circlesToDestroy.Count; i++)
            {
                summoningCircles.Remove(circlesToDestroy[i]);
                Destroy(circlesToDestroy[i]);
            }

            //create new summoning circles
            int numberToSpawn = 2;
            for (int i = 0; i < numberToSpawn; i++)
            {
                int r = UnityEngine.Random.Range(0, validSpawnLocations.Count);
                var spawnPosition = validSpawnLocations[r];
                while (summoningCircles.Where(s => s.transform.position == spawnPosition).Count() > 0)
                {
                    //don't put summoning circles on top of each other
                    r = UnityEngine.Random.Range(0, validSpawnLocations.Count);
                    spawnPosition = validSpawnLocations[r];
                }
                var newCircle = Instantiate(SummoningPortalPrefab, spawnPosition, Quaternion.identity, this.transform);
                summoningCircles.Add(newCircle);
            }

            blockInputs = false;
        }

        private List<Vector3Int> FindPathToCriteria(List<List<Vector3Int>> options, Func<Vector3Int, bool> criteria)
        {
            var x = options.Where(p => criteria(p.Last())).ToList();
            //TODO: add heuristics
            return x.FirstOrDefault();
        }

        private bool HasCleanShotFromPositionOnTarget(Vector3Int position, Func<UnitController, bool> unitPredicate)
        {
            foreach (var dir in GameManager.CardinalDirections)
            {
                if (RecursiveSearchFor(position, dir, unitPredicate) != null)
                {
                    return true;
                }
            }

            return false;
        }

        private UnitController RecursiveSearchFor(Vector3Int position, Vector3Int dir, Func<UnitController, bool> unitPredicate)
        {
            if (!Passable(position, false))
            {
                //hit a wall, nothing here
                return null;
            }
            var unit = AllUnits.Find(u => u.transform.position == position);
            if (unit != null)
            {
                if (unitPredicate(unit))
                {
                    return unit;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return RecursiveSearchFor(position + dir, dir, unitPredicate);
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
            UI.DisplayUnitInfo(this, UnitClicked);
        }

        public void AbilityButtonClick(int abilityIndex)
        {
            SelectedAbilityIndex = abilityIndex;
            TurnFSM.SetTrigger(GameStateTransitions.TargetAbility);
        }

        internal void RenderAbility(Ability ability)
        {
            Vector3Int basePosition;
            switch (ability.Range)
            {
                case Ability.RangeType.Melee:
                    foreach (var dir in GameManager.CardinalDirections)
                    {
                        UIHighlights.SetTile(Vector3Int.FloorToInt(UnitClicked.transform.position) + dir, TargetingTile);
                    }
                    break;
                case Ability.RangeType.DirectLOS:
                    basePosition = Vector3Int.FloorToInt(UnitClicked.transform.position);
                    foreach (var dir in GameManager.CardinalDirections)
                    {
                        RecursiveTargetUntilBlocked(basePosition, dir, blockedByUnits: true);
                    }
                    break;
                case Ability.RangeType.Mortar:
                    basePosition = Vector3Int.FloorToInt(UnitClicked.transform.position);
                    foreach (var dir in GameManager.CardinalDirections)
                    {
                        RecursiveTargetUntilBlocked(basePosition, dir, blockedByUnits: false);
                    }
                    break;
                case Ability.RangeType.Personal:
                    UIHighlights.SetTile(Vector3Int.FloorToInt(UnitClicked.transform.position), TargetingTile);
                    break;

                default:
                    Debug.LogError("not implemented, TODO");
                    break;
            }

        }

        private void RecursiveTargetUntilBlocked(Vector3Int basePosition, Vector3Int dir, bool blockedByUnits, Func<UnitController, bool> unitBlockingPredicate = null)
        {
            //TODO: add minimum range
            Vector3Int pos = basePosition + dir;
            while (Passable(pos, blockedByUnits, unitBlockingPredicate))
            {
                UIHighlights.SetTile(pos, TargetingTile);
                pos = pos + dir;
            }
            UIHighlights.SetTile(pos, TargetingTile);
        }
    }
}