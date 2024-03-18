using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class GuardEntity : MovingEntity
{
    public enum Alert { Patrol, Attack, Persue };

    [Foldout("Guard Entity",true)]
    [Header("Attacking")]
        [Tooltip("Times this attacks")] [ReadOnly] public int attacksPerTurn = 1;
        [Tooltip("Current number of attacks")] [ReadOnly] int attacksLeft = 0;
        [Tooltip("Current Target to attack & persue")] [ReadOnly] public PlayerEntity CurrentTarget;
        [Tooltip("Guard Range")] int AttackRange = 1;

    [Header("Detection")]
        [Tooltip("Tiles this is searching")] List<TileData> inDetection = new List<TileData>();
        [Tooltip("Pauses between movement")] protected float movePauseTime = 0.25f;
        [Tooltip("How far this can see")] [SerializeField] public int DetectionRangePatrol = 3;
        public int DetectionRangeMax = 3;
        [Tooltip("half their field of view for detection (MUST BE A MULTIPLE OF 5)")] [SerializeField] int DetectionAngle = 30;
        [Tooltip("State of a guard's alert")] public Alert alertStatus;

    [Header("Patrol")]
        [Tooltip("list of patrol positions")] public List<Vector2Int> PatrolPoints = new List<Vector2Int>();
        [Tooltip("current patrol target")] public int PatrolTarget = 1;


    [Header("Distraction")]
        [Tooltip("List of distraction positions")] public List<Vector2Int> DistractionPoints = new List<Vector2Int>();
        [Tooltip("Object used for the distraction alert for the guard")] [SerializeField] GameObject distractionNotif;
        [Tooltip("offset used to spawn distraction notifications")] [SerializeField] float NotifOffset = 1;

    [Header("Colors")]
        [Tooltip("Line renderer for showing the guard is attacking")] LineRenderer AttackLine = new LineRenderer();
        [Tooltip("color for when the guard is chasing")] [SerializeField] Material chaseColor;
        [Tooltip("color for when the guard is chasing")] [SerializeField] Material attackColor;
        [Tooltip("duration of color switch when attacking")] [SerializeField] float attackEffectDuration = 0.2f;
        bool attackEffect = false;

    [Header("Sounds")]
        [SerializeField] AK.Wwise.Event footsteps;
        [SerializeField] AK.Wwise.Event alertedSound;
        [SerializeField] AK.Wwise.Event meleeHit;
        [SerializeField] AK.Wwise.Event investigateSound;
        public AK.Wwise.Event stunSound;

    private void Awake()
    {
        AttackLine = GetComponent<LineRenderer>();
        DetectionRangeMax = DetectionRangePatrol;
        alertStatus = Alert.Patrol;
        PatrolTarget = 1;
    }

    public override string HoverBoxText()
    {
        string answer = "";
        if (stunned > 0)
            answer += $"Stunned for {stunned} turns\n";
        answer += $"current position {currentTile.gridPosition}\n";
        return answer;
    }

    private void FixedUpdate()
    {
        if (alertStatus == Alert.Attack && stunned == 0)
        {
            
            AttackLine.enabled = true;
            AttackLine.SetPositions(new Vector3[] { transform.position, CurrentTarget.transform.position });
            if (attackEffect)
            {
                //print("attack color");
                AttackLine.material = attackColor;
            }
            else
            {
                AttackLine.material = chaseColor;
            }
        }
        else
        {
            AttackLine.enabled = false;
        }
    }

    public void resetAlert()
    {
        alertStatus = Alert.Patrol;
        CurrentTarget = null;
    }
    IEnumerator attackEffectRoutine()
    {
        attackEffect = true;
        yield return new WaitForSeconds (attackEffectDuration);
        attackEffect = false;
    }

    //calculates the tiles this guard is looking at
    public override void CalculateTiles()
    {
        //clears the last list of visible tiles. Turns off the "surveillanceState" tag if no other guard is looking at that tile (stops it from looking red)
        for (int i = 0; i < inDetection.Count; i++)
        {
            bool Overlap = false;
            for (int j = 0; j < LevelGenerator.instance.listOfGuards.Count; j++)
            {
                if (LevelGenerator.instance.listOfGuards[j] != this && LevelGenerator.instance.listOfGuards[j].inDetection.Contains(inDetection[i]))
                {
                    Overlap = true;
                }
            }
            if(!Overlap) inDetection[i].SurveillanceState(false);
        }
        inDetection.Clear();

        List<HashSet<Vector2Int>> DetectLines = new List<HashSet<Vector2Int>>();
        float baseAngle = Mathf.Atan2(direction.y, direction.x);
        HashSet<Vector2Int> SpacesToCheck = new HashSet<Vector2Int>();

        //creating each of the lines of sight (seperated by 5 degrees) and adding them to a list of lines
        for (int i = -DetectionAngle; i < DetectionAngle; i += 5)
        {
            float lineAngle = baseAngle + (i * Mathf.Deg2Rad);
            Vector2 newVector = new Vector2(Mathf.Cos(lineAngle), Mathf.Sin(lineAngle));
            DetectLines.Add(Pathfinder.instance.line(currentTile.gridPosition,currentTile.gridPosition + Vector2Int.RoundToInt(newVector.normalized * DetectionRangePatrol)));
        }

        //running through each of the lines in the list, seeing how far they can look 
        for (int i = 0; i < DetectLines.Count; i++)
        {
            foreach (Vector2Int point in DetectLines[i])
            {
                TileData TileToAdd = LevelGenerator.instance.FindTile(point);
                if (TileToAdd == null)
                {
                    break;
                }
                if (TileToAdd.myEntity != null)
                {
                    if (TileToAdd.myEntity.Occlusion && point != currentTile.gridPosition)
                    {
                        break;
                    }
                }
                SpacesToCheck.Add(point);
            }
        }
        foreach (Vector2Int point in SpacesToCheck)
        {
            inDetection.Add(LevelGenerator.instance.FindTile(point));
        }
        if (DetectionRangePatrol > 0)
        {
            //checks all the tiles directly adjacent and ahead to the guard if they still have vision
            List<TileData> peripheralTile = new()
            {
                LevelGenerator.instance.FindTile(new Vector2Int(currentTile.gridPosition.x + direction.y, currentTile.gridPosition.y + direction.x)),
                LevelGenerator.instance.FindTile(new Vector2Int(currentTile.gridPosition.x - direction.y, currentTile.gridPosition.y - direction.x)),
                LevelGenerator.instance.FindTile(new Vector2Int(currentTile.gridPosition.x + direction.y + direction.x, currentTile.gridPosition.y + direction.x + direction.y)),
                LevelGenerator.instance.FindTile(new Vector2Int(currentTile.gridPosition.x - direction.y + direction.x, currentTile.gridPosition.y - direction.x + direction.y))
            };
            foreach (TileData currentTile in peripheralTile)
            {
                if (currentTile != null)
                {
                    if (currentTile.myEntity == null)
                        inDetection.Add(currentTile);
                    else if (currentTile.myEntity.Occlusion == false)
                        inDetection.Add(currentTile);
                }
            }
        }
    }

    public void addDistraction(Vector2Int position)
    {
        //print("distraction added");
        DistractionPoints.Add(position);
        investigateSound.Post(gameObject);
        GameObject notification = Instantiate(distractionNotif, transform);
        notification.transform.position = new Vector3(transform.position.x, transform.position.y + NotifOffset, transform.position.z);
        for (int i = 0; i < DistractionPoints.Count; i++)
        {
            LevelGenerator.instance.FindTile(DistractionPoints[i]).currentGuardTarget = false;
        }
        LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = true;
    }

    public void CheckForPlayer()
    {
        List<PlayerEntity> newTargets = new List<PlayerEntity>();
        for (int i = 0; i < inDetection.Count; i++)
        {
            //print("guard: " + currentTile.gridPosition + " Looking at " + inDetection[i].gridPosition);
            if (inDetection[i].myEntity != null)
            {
                if(inDetection[i].myEntity.CompareTag("Player"))
                {
                    if (inDetection[i].myEntity.GetComponent<PlayerEntity>().hidden == 0)
                    {
                        //print("found player");
                        newTargets.Add(inDetection[i].myEntity.GetComponent<PlayerEntity>());
                    }
                }
            }
        }
        if (newTargets.Count == 1)
        {
            Alerted(newTargets[0]);
        }
        else if (newTargets.Count > 1)
        {
            int minDistance = 1000;
            for (int i = 0;i < newTargets.Count;i++) 
            {
                if (Pathfinder.instance.GetDistance(currentTile.gridPosition, newTargets[i].currentTile.gridPosition) < minDistance)
                {
                    minDistance = Pathfinder.instance.GetDistance(currentTile.gridPosition, newTargets[i].currentTile.gridPosition);
                    Alerted(newTargets[i]);
                }
            }
        }
    }

    public override IEnumerator EndOfTurn()
    {
        print("start of turn");
        if (stunned > 0)
        {
            stunChange(-1);
            CurrentTarget = null;
        }
        else
        {
            DetectionRangePatrol = DetectionRangeMax;
            //movementLeft = movesPerTurn;
            attacksLeft = attacksPerTurn;
            alertStatus = Alert.Patrol;
            CheckForPlayer();
            if (DistractionPoints.Count > 0)
            {
                print("End turn setting to persue");
                alertStatus = Alert.Persue;
            }
            if (alertStatus == Alert.Patrol)
            {
                print("starting Patrol");
                yield return Patrol();
            }
            else if (alertStatus == Alert.Attack)
            {
                print("Starting attack");
                yield return Attack(CurrentTarget);
            }
            else if (alertStatus == Alert.Persue)
            {
                if (DistractionPoints.Count > 0)
                {
                    LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = true;
                }
                print("End of Turn persue");
                yield return persue();
            }

        }
    }

    public void Alerted(PlayerEntity target)
    {
        if (alertStatus != Alert.Attack)
        {
            alertStatus = Alert.Attack;
            alertedSound.Post(gameObject);
            foreach (Vector2Int pos in DistractionPoints)
            {
                TileData tile = LevelGenerator.instance.FindTile(pos);
                tile.currentGuardTarget = false;
            }
        }
        CurrentTarget = target;
        //print("New target, player at " + target.currentTile.gridPosition);
    }

    IEnumerator persue()
    {
        print(DistractionPoints.Count);
        print(currentTile.gridPosition + "checking distraction");
        if (DistractionPoints.Count == 0)
        {
            print("False Distraction");
            yield return (newAction());
            yield break;
        }
        if (currentTile.gridPosition == DistractionPoints[^1])
        {
            print("on distraction point");
            LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = false;
            DistractionPoints.RemoveAt(DistractionPoints.Count - 1);
            if (DistractionPoints.Count == 0)
            {
                print("no more distractions");
                alertStatus = Alert.Patrol;
                CheckForPlayer();
                if (alertStatus == Alert.Attack)
                {
                    if (attacksLeft > 0 || movementLeft > 0)
                    {
                        yield return Attack(CurrentTarget);
                    }
                    else
                        yield break;
                }
                else if (alertStatus == Alert.Patrol)
                {
                    if (movementLeft > 0)
                    {
                        yield return Patrol();
                    }
                    else
                        yield break;
                }
            }
            else if (currentTile.gridPosition == DistractionPoints[DistractionPoints.Count - 1])
            {
                print("restarting persuit");
                yield return persue();
            }
        }
        if (movementLeft > 0)
        {
            //print(movementLeft);
            TileData nextTile;
            Pathfinder.instance.CalculatePathfinding(currentTile, LevelGenerator.instance.FindTile(DistractionPoints[^1]), movementLeft, true, true);
            nextTile = Pathfinder.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
            Vector2Int nextDirection = nextTile.gridPosition - currentTile.gridPosition;

            if (nextDirection != direction)
            {
                direction = nextDirection;
                foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
                {
                    guard.CalculateTiles();
                }
            }
            else
            {
                //print("moving too " + nextTile.gridPosition);
                if (nextTile.myEntity == null)
                {
                    StartCoroutine(MoveTile(nextTile)); //footsteps.Post(gameObject);
                }
                movementLeft--;
            }

            yield return new WaitForSeconds(movePauseTime);
            yield return newAction();
        }
    }

    protected IEnumerator newAction()
    {
        alertStatus = Alert.Patrol;
        if (DistractionPoints.Count > 0)
        {
            alertStatus = Alert.Persue;
            LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = true;
        }
        foreach (GuardEntity guard in LevelGenerator.instance.listOfGuards)
            guard.CheckForPlayer();

        if (alertStatus == Alert.Attack)
        {
            if (movementLeft > 0 || attacksLeft > 0)
            {
                yield return Attack(CurrentTarget);
            }
        }
        else if (alertStatus == Alert.Persue)
        {
            if (movementLeft > 0)
            {
                print("New Action starting persue");
                yield return persue();
            }
        }
        else if (alertStatus == Alert.Patrol)
        {
            if (movementLeft > 0)
            {
                yield return Patrol();
            }
        }
    }

    IEnumerator Attack(PlayerEntity detectedPlayer)
    {
        print($"{currentTile.gridPosition} attacking player at {detectedPlayer.currentTile.gridPosition}");
        HashSet<Vector2Int> lineToPlayer = Pathfinder.instance.line(currentTile.gridPosition, detectedPlayer.currentTile.gridPosition);
        bool inSight = true;
        int distance = Pathfinder.instance.GetDistance(currentTile.gridPosition, detectedPlayer.currentTile.gridPosition);

        foreach (Vector2Int entry in lineToPlayer)
        {
            print("line of sight " + entry + " To target");
            TileData TileToCheck = LevelGenerator.instance.FindTile(entry);
            if (TileToCheck != null)
            {
                if (TileToCheck.myEntity != false)
                {
                    if (TileToCheck.myEntity != this && TileToCheck.myEntity != detectedPlayer)
                    {
                        if (TileToCheck.myEntity.Occlusion == true)
                        {
                            print("broken line of sight, occlusion");
                            inSight = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                print("broken line of sight, gap");
                inSight = false;
                break;
            }
        }
        if (inSight)
        {
            if (distance <= AttackRange)
            {
                print("within range, attacking");
                attacksLeft--;
                StartCoroutine(detectedPlayer.TakeDamage(1));
                StartCoroutine(attackEffectRoutine());
                meleeHit.Post(gameObject);
                yield return new WaitForSeconds(movePauseTime);

            }
            else
            {
                print("Out of range, persuing");
                if (movementLeft > 0)
                {
                    TileData nextTile;
                    Pathfinder.instance.CalculatePathfinding(currentTile, detectedPlayer.currentTile, movementLeft, true,true);
                    nextTile = Pathfinder.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
                    Vector2Int nextDirection = nextTile.gridPosition - currentTile.gridPosition;

                    if (nextDirection != direction)
                    {
                        direction = nextDirection;
                        CalculateTiles();
                    }
                    else
                    {
                        //print("moving too " + nextTile.gridPosition);
                        if (nextTile.myEntity == null)
                        {
                            StartCoroutine(MoveTile(nextTile));
                            //footsteps.Post(gameObject);
                        }
                        movementLeft--;
                    }
                }
                else
                    yield break;


                yield return new WaitForSeconds(movePauseTime);
                distance = Pathfinder.instance.GetDistance(currentTile.gridPosition, detectedPlayer.currentTile.gridPosition);
                if (distance < AttackRange)
                {
                    if (attacksLeft > 0)
                    {
                        yield return Attack(detectedPlayer);
                    }
                }
                else if (movementLeft > 0)
                {
                    yield return Attack(detectedPlayer);
                }
            }
        }
        else
        {
            print("moving to distraction persuit");
            DistractionPoints.Add(detectedPlayer.currentTile.gridPosition);
            alertStatus = Alert.Persue;
            LevelGenerator.instance.FindTile(DistractionPoints[^1]).currentGuardTarget = true;
            if (movementLeft > 0)
            {
                print("Persuing from attack");
                yield return persue();
            }
        }
    }

    public virtual IEnumerator Patrol()
    {
        //print(currentTile.gridPosition + "Patrolling");
        TileData nextTile;
        if (currentTile == LevelGenerator.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y])
        {
            PatrolTarget++;
            if (PatrolTarget >= PatrolPoints.Count)
            {
                PatrolTarget = 0;
            }
        }
        Pathfinder.instance.CalculatePathfinding(currentTile, LevelGenerator.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y], movementLeft, true, true);
        nextTile = Pathfinder.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
        Vector2Int nextDirection = nextTile.gridPosition - currentTile.gridPosition;

        if (nextDirection != direction)
        {
            direction = nextDirection;
            CalculateTiles();
        }
        else
        {
            //print("moving too " + nextTile.gridPosition);
            if (nextTile.myEntity == null)
            {
                StartCoroutine(MoveTile(nextTile));
                //footsteps.Post(gameObject);
            }
            movementLeft--;
        }


        yield return new WaitForSeconds(movePauseTime);
        //print("Checking New Action");
        yield return newAction();
    }
}
