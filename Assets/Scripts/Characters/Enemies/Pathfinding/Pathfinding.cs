using System.Collections.Generic;
using UnityEngine;


public class Pathfinding : MonoBehaviour
{
    public GridManager gridManager;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = GameObject.Find("GridManager").GetComponent<GridManager>();
        }
    }

    public List<Node> FindPath(Vector3 startPos, Vector3 targetPos)
    {
        Debug.Log("NEW PATH CALCULATED");
        Node startNode = gridManager.GetNodeFromWorldPosition(startPos);
        Node targetNode = gridManager.GetNodeFromWorldPosition(targetPos);

        if (!startNode.walkable || !targetNode.walkable)
        {
            Debug.Log("Start or target node is unwalkable");
            return null; // Return null if the target is unreachable
        }

        List<Node> openSet = new List<Node>();        // Nodes to be evaluated
        HashSet<Node> closedSet = new HashSet<Node>(); // Already evaluated nodes
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentNode.fCost || (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                {
                    currentNode = openSet[i];
                }
            }

            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            if (currentNode == targetNode)
                return RetracePath(startNode, targetNode);

            foreach (Node neighbor in gridManager.GetNeighbors(currentNode))
            {
                if (!neighbor.walkable || closedSet.Contains(neighbor))
                    continue;

                int newMovementCost = currentNode.gCost + GetDistance(currentNode, neighbor);
                if (newMovementCost < neighbor.gCost || !openSet.Contains(neighbor))
                {
                    neighbor.gCost = newMovementCost;
                    neighbor.hCost = GetDistance(neighbor, targetNode);
                    neighbor.parent = currentNode;

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        Debug.Log("No path found");
        return null; // No path found
    }

    int GetDistance(Node a, Node b)
    {
        int distX = Mathf.Abs(a.gridX - b.gridX);
        int distZ = Mathf.Abs(a.gridZ - b.gridZ);

        // Diagonal movement
        if (distX > distZ)
            return 14 * distZ + 10 * (distX - distZ);
        return 14 * distX + 10 * (distZ - distX);
    }

    List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        return path;
    }

    public Node GetNode(Vector3 position)
    {
        return gridManager.GetNodeFromWorldPosition(position);
    }
}

