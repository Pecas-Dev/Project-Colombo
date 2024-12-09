using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.Port;

public class Node
{
    public Vector3 worldPosition; // The position of the node in the world
    public bool walkable;         // Whether the node is traversable
    public int gCost, hCost;      // gCost (from start), hCost (heuristic)
    public Node parent;           // To reconstruct the path
    public int gridX, gridZ;      // The node's position in the grid

    public int fCost => gCost + hCost; // Combined cost

    public Node(Vector3 _worldPosition, bool _walkable, int _gridX, int _gridZ)
    {
        worldPosition = _worldPosition;
        walkable = _walkable;
        gridX = _gridX;
        gridZ = _gridZ;
    }
}

[ExecuteInEditMode]
public class GridManager : MonoBehaviour
{
    public int gridSizeX, gridSizeZ;   // Grid dimensions
    public float nodeSize;            // Size of each node
    [Range(0f, 1f)] // Creates a slider in the Inspector
    public float gizmosOpacity = 1f;
    public LayerMask obstacleLayer;   // Layer for obstacles
    private Node[,] grid;             // 2D array of nodes

    public void CreateGrid()
    {
        grid = new Node[gridSizeX, gridSizeZ];
        Vector3 bottomLeft = transform.position - Vector3.right * gridSizeX / 2 * nodeSize - Vector3.forward * gridSizeZ / 2 * nodeSize;

        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Vector3 worldPoint = bottomLeft + Vector3.right * (x * nodeSize + nodeSize / 2) + Vector3.forward * (z * nodeSize + nodeSize / 2);
                bool walkable = !Physics.CheckSphere(worldPoint, nodeSize / 2, obstacleLayer);
                grid[x, z] = new Node(worldPoint, walkable, x, z);
            }
        }
    }

    public Node GetNodeFromWorldPosition(Vector3 worldPosition)
    {
        float percentX = Mathf.Clamp01((worldPosition.x - transform.position.x + gridSizeX / 2 * nodeSize) / (gridSizeX * nodeSize));
        float percentZ = Mathf.Clamp01((worldPosition.z - transform.position.z + gridSizeZ / 2 * nodeSize) / (gridSizeZ * nodeSize));

        int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
        int z = Mathf.RoundToInt((gridSizeZ - 1) * percentZ);

        return grid[x, z];
    }

    public List<Node> GetNeighbors(Node node)
    {
        List<Node> neighbors = new List<Node>();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dz = -1; dz <= 1; dz++)
            {
                if (dx == 0 && dz == 0)
                    continue; // Skip the current node

                int checkX = node.gridX + dx;
                int checkZ = node.gridZ + dz;

                if (checkX >= 0 && checkX < gridSizeX && checkZ >= 0 && checkZ < gridSizeZ)
                {
                    neighbors.Add(grid[checkX, checkZ]);
                }
            }
        }

        return neighbors;
    }
    private void Update()
    {
        CreateGrid();
    }


    private void OnDrawGizmos()
    {
        // Ensure grid exists
        if (grid == null)
        {
            Debug.Log("no grid");
            return;
        }

        // Loop through all nodes
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int z = 0; z < gridSizeZ; z++)
            {
                Node node = grid[x, z];

                Color good = new Color(Color.white.r, Color.white.g, Color.white.b, gizmosOpacity);
                Color bad = new Color(Color.red.r, Color.red.g, Color.red.b, gizmosOpacity);

                // Draw the node's boundaries
                Gizmos.color = node.walkable ? good : bad;
                Gizmos.DrawCube(node.worldPosition, Vector3.one * (nodeSize * 0.9f));
            }
        }
    }
}
