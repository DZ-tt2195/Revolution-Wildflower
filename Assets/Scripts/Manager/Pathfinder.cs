using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;

public class AStarNode
{
    public TileData ATileData;
    public AStarNode Parent;

    public int GCost; //travel distance from node to start node
    public int HCost; //travel distance from node to target node
    public int FCost => GCost + HCost; //Astar value of this tile, the lower it is, the better for the pathfinder.
}

public class Pathfinder : MonoBehaviour
{
    public static Pathfinder instance;
    [Tooltip("Quick reference to current movable tile")][ReadOnly] public TileData CurrentAvailableMoveTarget;
    [Tooltip("The last path traced. This only is filled if (singleMovement) is disabled")][ReadOnly] public List<TileData> FullPath = new();

    private void Awake()
    {
        instance = this;
    }

    public HashSet<Vector2Int> line(Vector2Int p1, Vector2Int p2)
    {
        HashSet<Vector2Int> points = new HashSet<Vector2Int>();
        float distance = GetDistance(p1, p2);
        for (float step = 0; step <= distance; step++)
        {
            float t = step / distance;
            Vector2 midPointRaw = Vector2.Lerp(p1, p2, t);
            Vector2Int midPoint = new Vector2Int(Mathf.RoundToInt(midPointRaw.x), Mathf.RoundToInt(midPointRaw.y));
            points.Add(midPoint);
        }
        return points;
    }

    //gets the distance (in gridspaces) between two gridspaces
    public int GetDistance(Vector2Int a, Vector2Int b)
    {
        int distX = Mathf.Abs(a.x - b.x);
        int distY = Mathf.Abs(a.y - b.y);
        return distY + distX;
    }

    //find all grids that can be moved to
    public List<TileData> CalculateReachableGrids(TileData startLocation, int movementSpeed, bool considerEntities)
    {
        List<TileData> reachableGrids = new List<TileData>();

        //First in first out
        Queue<(TileData, int)> queue = new Queue<(TileData, int)>();

        //HashSet is a simple collection without orders
        HashSet<TileData> visited = new HashSet<TileData>();

        queue.Enqueue((startLocation, 0));
        visited.Add(startLocation);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            TileData SelectTile = current.Item1;
            int cost = current.Item2;

            reachableGrids.Remove(startLocation);

            if (cost <= movementSpeed)
            {
                reachableGrids.Add(SelectTile); ;
                //FindAdjacent(SelectTile);
                foreach (TileData neighbor in SelectTile.adjacentTiles)
                {
                    int newCost;
                    if (neighbor.myEntity != null && considerEntities)
                    {
                        newCost = cost + neighbor.myEntity.MoveCost;
                    }
                    else
                    {
                        newCost = cost + 1;
                    }

                    if (!visited.Contains(neighbor) && newCost <= movementSpeed)
                    {
                        if (neighbor.myEntity == null || considerEntities)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }
                        else if (neighbor.myEntity.MoveCost! >= 999)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }

                    }
                }
            }
        }
        reachableGrids.Remove(startLocation);
        return reachableGrids;
    }

    //used for determining how sound moves, uses different move cost limits than regular directions
    public List<TileData> CalculateIntensity(TileData startLocation, int movementSpeed, bool considerEntities)
    {
        //print("calculating intensity");
        List<TileData> reachableGrids = new List<TileData>();

        //First in first out
        Queue<(TileData, int)> queue = new Queue<(TileData, int)>();

        //HashSet is a simple collection without orders
        HashSet<TileData> visited = new HashSet<TileData>();

        queue.Enqueue((startLocation, 0));
        visited.Add(startLocation);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            TileData SelectTile = current.Item1;
            int cost = current.Item2;
            //print("cost: " + cost);
            //print("current: " + current.Item1.gridPosition);
            if (cost <= movementSpeed)
            {
                reachableGrids.Add(SelectTile); ;
                //FindAdjacent(SelectTile);
                foreach (TileData neighbor in SelectTile.adjacentTiles)
                {
                    int newCost;
                    if (neighbor.myEntity != null && considerEntities)
                    {
                        //print("moving over " + neighbor.myEntity.tag);
                        newCost = cost + neighbor.myEntity.SoundCost;
                    }
                    else
                    {
                        newCost = cost + 1;
                    }

                    if (!visited.Contains(neighbor) && newCost <= movementSpeed)
                    {
                        if (neighbor.myEntity == null || considerEntities)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }
                        else if (neighbor.myEntity.SoundCost! >= 999)
                        {
                            queue.Enqueue((neighbor, newCost));
                            visited.Add(neighbor);
                        }

                    }
                }
            }
        }
        //print("finished calculating intensity");
        return reachableGrids;
    }

    //find fastest way to get from one point to another
    //startLocation - the tile the entity is starting from
    //targetLocation - the tile the entity wants to move to
    //movementPoints - the max amount of spaces the entity can move 
    //singleMovement - whether or not the entity will stop after 1 spaces moved
    //considerEntities - whether or not the entity will see players as pathable tiles (used for guards looking for players)

    public void CalculatePathfinding(TileData startLocation, TileData targetLocation, int movementPoints, bool singleMovement, bool considerPlayers)
    {
        //open list is all the current neighbors to the analyzed path, the tiles are avalible to be scanned and haven't been checked yet
        List<AStarNode> openList = new List<AStarNode>();
        //these tiles have all been scanned, tiles in the closed list can't be added to the open list
        HashSet<TileData> closedList = new HashSet<TileData>();
        //dictionary which uses neighbors to either call or create new AStarNodes to add to the open list
        Dictionary<TileData, AStarNode> nodeLookup = new Dictionary<TileData, AStarNode>();

        AStarNode startNode = new AStarNode { ATileData = startLocation };
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            AStarNode currentNode = openList[0];
            for (int i = 1; i < openList.Count; i++)
            {
                if (openList[i].FCost < currentNode.FCost || openList[i].FCost == currentNode.FCost && openList[i].HCost < currentNode.HCost)
                {
                    currentNode = openList[i];
                }
            }
            openList.Remove(currentNode);
            closedList.Add(currentNode.ATileData);
            if (currentNode.ATileData.gridPosition == targetLocation.gridPosition)
            {
                RetracePath(startNode, currentNode, movementPoints, singleMovement);
                return;
            }
            foreach (TileData neighbor in currentNode.ATileData.adjacentTiles)
            {
                int movementCostToNeighbor = 0;
                if (closedList.Contains(neighbor))
                {
                    continue;
                }
                if (neighbor.myEntity != null)
                {
                    if (considerPlayers && neighbor.myEntity.tag == "Player")
                    {
                        movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData.gridPosition, neighbor.gridPosition);
                    }
                    else
                    {
                        if (neighbor.gridPosition != targetLocation.gridPosition && neighbor.myEntity.MoveCost > 100)
                        {
                            continue;
                        }
                        movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData.gridPosition, neighbor.gridPosition) * neighbor.myEntity.MoveCost;
                    }

                }
                else
                {
                    movementCostToNeighbor = currentNode.GCost + GetDistance(currentNode.ATileData.gridPosition, neighbor.gridPosition);
                }
                AStarNode neighborNode;
                if (!nodeLookup.ContainsKey(neighbor))
                {
                    neighborNode = new AStarNode { ATileData = neighbor };
                    nodeLookup[neighbor] = neighborNode;
                }
                else
                {
                    neighborNode = nodeLookup[neighbor];
                }
                if (movementCostToNeighbor < neighborNode.GCost || !openList.Contains(neighborNode))
                {
                    neighborNode.GCost = movementCostToNeighbor;
                    neighborNode.HCost = GetDistance(neighbor.gridPosition, targetLocation.gridPosition);
                    neighborNode.Parent = currentNode;
                    //print(neighborNode.ATileData.gridPosition + "'s parent is " + currentNode.ATileData.gridPosition);
                    // Add neighbor to the open list if it's not already there
                    if (!openList.Contains(neighborNode))
                    {
                        openList.Add(neighborNode);
                    }
                }
            }
        }
    }

    public void RetracePath(AStarNode startNode, AStarNode endNode, int actionPoint, bool singleMovement)
    {
        FullPath.Clear();
        List<AStarNode> path = new List<AStarNode>();
        AStarNode currentNode = endNode;
        while (currentNode != startNode)
        {
            //print("Current stage on path is" + currentNode.ATileData.gridPosition);
            path.Add(currentNode);
            currentNode = currentNode.Parent;

        }
        path.Reverse();

        int pathCost = 0;
        if (!singleMovement)
        {
            foreach (AStarNode CurrentTile in path)
            {
                if (CurrentTile.ATileData.myEntity != null)
                {
                    pathCost += CurrentTile.ATileData.myEntity.MoveCost;
                }
                else
                {
                    pathCost++;
                }

                // If the path cost exceeds the action points available, stop displaying the path
                if (pathCost > actionPoint)
                {
                    CurrentAvailableMoveTarget = CurrentTile.ATileData;
                    FullPath.Add(CurrentTile.ATileData);
                    continue;
                }
                // Update the current available move target and display the pathfinding visualization with the path cost
                CurrentAvailableMoveTarget = CurrentTile.ATileData;
                FullPath.Add(CurrentTile.ATileData);
            }
        }
        else
        {
            CurrentAvailableMoveTarget = path[0].ATileData;
        }
    }

    //find fastest way to get from one point to another

}
