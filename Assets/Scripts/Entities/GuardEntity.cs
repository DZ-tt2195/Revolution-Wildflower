using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class GuardEntity : MovingEntity
{
    enum Alert { Patrol, Attack};

    [Foldout("Guard Entity", true)]
        [Tooltip("Tiles this is searching")] List<TileData> inDetection = new List<TileData>();
        [Tooltip("Pauses between movement")] float movePauseTime = 0.25f;
        [Tooltip("How far this can see")][SerializeField] int DetectionRangePatrol = 3;
        [Tooltip("Turns which this does nothing")] [ReadOnly] public int stunned = 0;
        [Tooltip("Times this attacks")] [ReadOnly] public int attacksPerTurn = 1;
        [Tooltip("Current number of attacks")][ReadOnly] int attacksLeft = 0;
        [Tooltip("Current Target to attack & persue")] PlayerEntity CurrentTarget;
        [Tooltip("State of a guard's alert")] Alert alertStatus = 0;
        [Tooltip("Guard Range")] int AttackRange = 1;
        [Tooltip("list of patrol positions")] public List<Vector2Int> PatrolPoints = new List<Vector2Int>();
        [Tooltip("current patrol target")] private int PatrolTarget = 0;
        [Tooltip("Line renderer for showing the guard is attacking")] LineRenderer AttackLine = new LineRenderer();
        [SerializeField] AudioClip footsteps;
        [SerializeField] AudioClip alertedSound;
        [SerializeField] AudioClip gunshot;
        public AudioClip stunSound;

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

        Vector2Int side = Vector2Int.RoundToInt(Vector3.Cross((Vector2)direction, Vector3.forward));
        for (int i = 0; i < DetectionRangePatrol; i++)
        {
            inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition + new Vector2Int(direction.x * i, direction.y * i)));
            if (i <= 1)
            {
                inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition + side + new Vector2Int(direction.x * i, direction.y * i)));
                inDetection.Add(NewManager.instance.FindTile(currentTile.gridPosition - side + new Vector2Int(direction.x * i, direction.y * i)));
            }
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
            if(alertStatus == Alert.Patrol)
                yield return Patrol();
            else
                yield return Attack(CurrentTarget);
        }
    }

    public void Alerted(PlayerEntity target)
    {
        alertStatus = Alert.Attack;
        CurrentTarget = target;
        SoundManager.instance.PlaySound(alertedSound);
        print("New target, player at " + target.currentTile.gridPosition);
    }

    IEnumerator Attack(PlayerEntity detectedPlayer)
    {
        RaycastHit hit;
        Vector3 shotDirection = detectedPlayer.transform.position - transform.position;
        if (Physics.Raycast(transform.position, shotDirection, out hit, Mathf.Infinity, 1 << 2))
        {
            if (hit.collider.gameObject.tag == "Player")
            {
                if (NewManager.instance.GetDistance(currentTile, detectedPlayer.currentTile) > AttackRange && movementLeft > 0)
                {
                    NewManager.instance.CalculatePathfinding(currentTile, detectedPlayer.currentTile, movementLeft, true);
                    MoveTile(NewManager.instance.CurrentAvailableMoveTarget);
                    movementLeft--;
                }
                if (NewManager.instance.GetDistance(currentTile, detectedPlayer.currentTile) <= AttackRange && attacksLeft > 0)
                {
                    NewManager.instance.ChangeHealth(detectedPlayer, -1);
                    SoundManager.instance.PlaySound(gunshot);
                    attacksLeft--;
                }
            }
            else
            {
                alertStatus = Alert.Patrol;
            }
        }
        else
        {
            alertStatus = Alert.Patrol;
        }

        //checking whether to end round or 
        float timer = 0;
        while (timer < movePauseTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        if (alertStatus == Alert.Attack)
        {
            if (NewManager.instance.GetDistance(currentTile, CurrentTarget.currentTile) > AttackRange && movementLeft > 0)
            {
                Attack(CurrentTarget);
                yield break;
            }
            else if (NewManager.instance.GetDistance(currentTile, CurrentTarget.currentTile) <= AttackRange && attacksLeft > 0)
            {
                Attack(CurrentTarget);
                yield break;
            }
        }
        else if (alertStatus == Alert.Patrol)
        {
            if (movementLeft > 0)
            {
                Patrol();
                yield break;
            }
        }

    }

    IEnumerator Patrol()
    {
        //print(currentTile.gridPosition + " has " + PatrolPoints.Count + " Patrol Points");
        if (PatrolPoints.Count < 2)
        {
            print("++++++++++++++++++++++");
            //print(currentTile.gridPosition + "is a stationary guard");
            movementLeft = 0;
            CheckForPlayer();
            if (alertStatus == Alert.Attack)
            {
                print("stationary at " + currentTile.gridPosition + " is finished, moving to attack");
                Attack(CurrentTarget);
            }
            print("stationary guard finished, end script");
            yield break;
        }
        else
        {
            //print(currentTile.gridPosition + "is a moving guard");
            TileData nextTile;
            if (currentTile == NewManager.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y])
            {
                PatrolTarget++;
                if (PatrolTarget >= PatrolPoints.Count)
                {
                    PatrolTarget = 0;
                }
            }
            print("++++++++++++++++++++++");
            print("current tile " + currentTile.gridPosition);
            print("Target tile " + PatrolPoints[PatrolTarget].x + "," + PatrolPoints[PatrolTarget].y);
            NewManager.instance.CalculatePathfinding(currentTile, NewManager.instance.listOfTiles[PatrolPoints[PatrolTarget].x, PatrolPoints[PatrolTarget].y], movementLeft, true);
            nextTile = NewManager.instance.CurrentAvailableMoveTarget;  //moves towards the next patrol point
            /*nextTile = NewManager.instance.FindTile(currentTile.gridPosition + direction); //find tile in the current direction
            while (nextTile == null || nextTile.myEntity != null) //if it can't
            {
                List<TileData> possibleTiles = new List<TileData>();
                for (int i = 0; i<currentTile.adjacentTiles.Count; i++)
                {
                    if (currentTile.adjacentTiles[i].myEntity == null) //find all adjacent tiles that this can move to
                        possibleTiles.Add(currentTile.adjacentTiles[i]);
                }
                nextTile = possibleTiles[Random.Range(0, possibleTiles.Count)]; //pick a random tile that's available
                direction = nextTile.gridPosition - currentTile.gridPosition; //change direction
            }
            */
            print("moving too " + nextTile.gridPosition);
            MoveTile(nextTile);//move to the tile
            SoundManager.instance.PlaySound(footsteps);
            yield return NewManager.Wait(movePauseTime);
            movementLeft--;


            CheckForPlayer();
            float timer = 0;
            while (timer < movePauseTime)
            {
                timer += Time.deltaTime;
                yield return null;
            }
            if (alertStatus == Alert.Attack)
            {
                if (NewManager.instance.GetDistance(currentTile, CurrentTarget.currentTile) > AttackRange && movementLeft > 0)
                {
                    Attack(CurrentTarget);
                    print(currentTile.gridPosition + " is Finished, moving on to attack");
                    yield break;
                }
                else if (NewManager.instance.GetDistance(currentTile, CurrentTarget.currentTile) <= AttackRange && attacksLeft > 0)
                {
                    Attack(CurrentTarget);
                    print(currentTile.gridPosition + " is Finished, moving on to attack");
                    yield break;
                }
            }
            else if (alertStatus == Alert.Patrol)
            {
                if (movementLeft > 0)
                {
                    Patrol();
                    print(currentTile.gridPosition + " is Finished, moving on to patrol");
                    yield break;
                }
            }
            print(currentTile.gridPosition + " is Finished, ending turn");
        }
    }
}
