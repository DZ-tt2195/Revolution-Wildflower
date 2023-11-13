using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class GuardEntity : MovingEntity
{
    enum Alert { Patrol, Attack, Persue};

    [Foldout("Guard Entity", true)]
        [Tooltip("Tiles this is searching")] List<TileData> inDetection = new List<TileData>();
        [Tooltip("Pauses between movement")] float movePauseTime = 0.25f;
        [Tooltip("How far this can see")][SerializeField] int DetectionRangePatrol = 3;
    [Tooltip("half their field of view for detection")] [SerializeField] float DetectionAngle = 30;
        [Tooltip("Turns which this does nothing")] [ReadOnly] public int stunned = 0;
        [Tooltip("Times this attacks")] [ReadOnly] public int attacksPerTurn = 1;
        [Tooltip("Current number of attacks")][ReadOnly] int attacksLeft = 0;
        [Tooltip("Current Target to attack & persue")] [ReadOnly] public PlayerEntity CurrentTarget;
        [Tooltip("State of a guard's alert")] Alert alertStatus = 0;
        [Tooltip("Guard Range")] int AttackRange = 1;
        [Tooltip("list of patrol positions")] public List<Vector2Int> PatrolPoints = new List<Vector2Int>();
        [Tooltip("current patrol target")] private int PatrolTarget = 0;
        [Tooltip("List of distraction positions")] public List<Vector2Int> DistractionPoints = new List<Vector2Int>();
        [Tooltip("Line renderer for showing the guard is attacking")] LineRenderer AttackLine = new LineRenderer();
        [SerializeField] AK.Wwise.Event footsteps;
        [SerializeField] AK.Wwise.Event alertedSound;
        [SerializeField] AK.Wwise.Event gunshot;
        public AK.Wwise.Event stunSound;

    private void Awake()
    {
        AttackLine = GetComponent<LineRenderer>();
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
        }
        else
        {
            AttackLine.enabled = false;
        }
    }

    public override void CalculateTiles()
    {
        for (int i = 0; i<inDetection.Count; i++)
            inDetection[i].SurveillanceState(false);
        inDetection.Clear();

        //creating vectors
        float angle = Mathf.Atan2(direction.y, direction.x);
        float PosAngle = angle + (DetectionAngle * Mathf.Deg2Rad);
        float NegAngle = angle - (DetectionAngle * Mathf.Deg2Rad);
        Vector2 PosMaxVector = new Vector2(Mathf.Cos(PosAngle), Mathf.Sin(PosAngle));
        Vector2 PosMinVector = new Vector2(Mathf.Cos(NegAngle), Mathf.Sin(NegAngle));

        //drawing each line
        HashSet<Vector2Int> MaxLine = NewManager.instance.line(currentTile.gridPosition, currentTile.gridPosition + Vector2Int.RoundToInt(PosMaxVector.normalized * DetectionRangePatrol));
        HashSet<Vector2Int> MinLine = NewManager.instance.line(currentTile.gridPosition, currentTile.gridPosition +  Vector2Int.RoundToInt(PosMinVector.normalized * DetectionRangePatrol));
        HashSet<Vector2Int> CenterLine = NewManager.instance.line(currentTile.gridPosition, currentTile.gridPosition + (direction * DetectionRangePatrol));
        HashSet<Vector2Int> SpacesToCheck = new HashSet<Vector2Int>();

        Vector2Int lastVisMax = currentTile.gridPosition;
        Vector2Int lastVisMin = currentTile.gridPosition;
        Vector2Int lastVisMid = currentTile.gridPosition;


        //validating each line and drawing their ends
        foreach (Vector2Int point in MaxLine)
        {

            TileData TileToAdd = NewManager.instance.FindTile(point);
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
            lastVisMax = point;
            SpacesToCheck.Add(point);
        }
        foreach (Vector2Int point in MinLine)
        {

            TileData TileToAdd = NewManager.instance.FindTile(point);
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
            lastVisMin = point;
            SpacesToCheck.Add(point);
        }
        foreach (Vector2Int point in CenterLine)
        {
             
            TileData TileToAdd = NewManager.instance.FindTile(point);
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
            lastVisMid = point;
            SpacesToCheck.Add(point);
        }

        //filling the cone (+ validating fill, wildly inefficient but it works for now)
        HashSet<Vector2Int> MinMidConnection = NewManager.instance.line(lastVisMin, currentTile.gridPosition + (direction * DetectionRangePatrol));
        HashSet<Vector2Int> MaxMidConnection = NewManager.instance.line(lastVisMax, currentTile.gridPosition + (direction * DetectionRangePatrol));
        HashSet<Vector2Int> MidMinConnection = NewManager.instance.line(lastVisMid, currentTile.gridPosition + Vector2Int.RoundToInt(PosMinVector.normalized * DetectionRangePatrol));
        HashSet<Vector2Int> MidMaxConnection = NewManager.instance.line(lastVisMid, currentTile.gridPosition + Vector2Int.RoundToInt(PosMaxVector.normalized * DetectionRangePatrol));
        foreach (Vector2Int point in MinMidConnection)
        {

            TileData TileToAdd = NewManager.instance.FindTile(point);
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
        foreach (Vector2Int point in MidMinConnection)
        {

            TileData TileToAdd = NewManager.instance.FindTile(point);
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
        foreach (Vector2Int point in MaxMidConnection)
        {

            TileData TileToAdd = NewManager.instance.FindTile(point);
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
        foreach (Vector2Int point in MidMaxConnection)
        {

            TileData TileToAdd = NewManager.instance.FindTile(point);
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

        foreach (Vector2Int point in SpacesToCheck)
        {
            inDetection.Add(NewManager.instance.FindTile(point));
        }
        inDetection.RemoveAll(item => item == null); //delete all tiles that are null
        for (int i = 0; i < inDetection.Count; i++)
            inDetection[i].SurveillanceState(true);
    }

    public void CheckForPlayer()
    {
        for (int i = 0; i < inDetection.Count; i++)
        {
            //print("guard: " + currentTile.gridPosition + " Looking at " + inDetection[i].gridPosition);
            if (inDetection[i].myEntity != null)
            {
                if(inDetection[i].myEntity.CompareTag("Player"))
                {
                    if (inDetection[i].myEntity.GetComponent<PlayerEntity>().hidden == 0)
                    {
                        print("found player");
                        Alerted(inDetection[i].myEntity.GetComponent<PlayerEntity>());
                        break;
                    }
                }
            }
        }

    }

    public override IEnumerator EndOfTurn()
    {
        if (stunned > 0)
        {
            stunned--;
        }
        else
        {
            movementLeft = movesPerTurn;
            attacksLeft = attacksPerTurn;
            CheckForPlayer();
            print(DistractionPoints.Count);
            if (DistractionPoints.Count > 0)
                alertStatus = Alert.Persue;
            if (alertStatus == Alert.Patrol)
                yield return Patrol();
            else if (alertStatus == Alert.Attack)
                yield return Attack(CurrentTarget);
            else if (alertStatus == Alert.Persue)
                yield return persue();
        }
    }

    public void Alerted(PlayerEntity target)
    {
        alertStatus = Alert.Attack;
        CurrentTarget = target;
        alertedSound.Post(gameObject);
        print("New target, player at " + target.currentTile.gridPosition);
    }

    IEnumerator persue()
    {
        print(currentTile.gridPosition + "checking distraction");
        if (currentTile.gridPosition == DistractionPoints[DistractionPoints.Count - 1])
        {
            DistractionPoints.RemoveAt(DistractionPoints.Count - 1);
            if (DistractionPoints.Count == 0)
            {
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
        }
        if (movementLeft > 0)
        {
            print(movementLeft);
            TileData nextTile;
            NewManager.instance.CalculatePathfinding(currentTile, NewManager.instance.FindTile(DistractionPoints[DistractionPoints.Count - 1]), movementLeft, true);
            nextTile = NewManager.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
            direction = nextTile.gridPosition - currentTile.gridPosition;
            print("moving too " + nextTile.gridPosition);
            MoveTile(nextTile);//move to the tile
            footsteps.Post(gameObject);
            movementLeft--;

            yield return NewManager.Wait(movePauseTime);
            alertStatus = Alert.Patrol;
            if(DistractionPoints.Count > 0)
            {
                alertStatus = Alert.Persue;
            }
            CheckForPlayer();
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
        else
            yield break;
        
    }

    IEnumerator Attack(PlayerEntity detectedPlayer)
    {

        print(currentTile.gridPosition + " attacking player at " + detectedPlayer.currentTile.gridPosition);
        HashSet<Vector2Int> lineToPlayer = NewManager.instance.line(currentTile.gridPosition, detectedPlayer.currentTile.gridPosition);
        bool inSight = true;
        int distance = NewManager.instance.GetDistance(currentTile.gridPosition, detectedPlayer.currentTile.gridPosition);
        foreach (Vector2Int entry in lineToPlayer)
        {
            print("line of sight " + entry + " To target");
            TileData TileToCheck = NewManager.instance.FindTile(entry);
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
                detectedPlayer.health--;
                gunshot.Post(gameObject);
                yield break;

            }
            else
            {
                print("Out of range, persuing");
                if (movementLeft > 0)
                {
                    TileData nextTile;
                    NewManager.instance.CalculatePathfinding(currentTile, detectedPlayer.currentTile, movementLeft, true);
                    nextTile = NewManager.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
                    direction = nextTile.gridPosition - currentTile.gridPosition;
                    print("moving too " + nextTile.gridPosition);
                    MoveTile(nextTile);//move to the tile
                    footsteps.Post(gameObject);
                    movementLeft--;
                }
                else
                    yield break;


                yield return NewManager.Wait(movePauseTime);
                distance = NewManager.instance.GetDistance(currentTile.gridPosition, detectedPlayer.currentTile.gridPosition);
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
            if (movementLeft > 0)
            {
                yield return persue();
            }
        }
    }

    IEnumerator Patrol()
    {
        //print(currentTile.gridPosition + "Patrolling");
        TileData nextTile;
        if (currentTile == NewManager.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y])
        {
            PatrolTarget++;
            if (PatrolTarget >= PatrolPoints.Count)
            {
                PatrolTarget = 0;
            }
        }
        NewManager.instance.CalculatePathfinding(currentTile, NewManager.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y], movementLeft, true);
        nextTile = NewManager.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
        direction = nextTile.gridPosition - currentTile.gridPosition;
        //print("moving too " + nextTile.gridPosition);
        MoveTile(nextTile);//move to the tile
        footsteps.Post(gameObject);
        movementLeft--;


        CheckForPlayer();
        yield return NewManager.Wait(movePauseTime);
        if (movementLeft > 0)
        {
            if (alertStatus == Alert.Attack)
            {
                yield return Attack(CurrentTarget);
            }
            if (alertStatus == Alert.Patrol)
            {
                yield return Patrol();

            }
        }
    }
}
