using System.Collections.Generic;
using System.Linq;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    struct Node
    {
        public int myIndex;
        public int parent;
        public int xPosition;
        public int yPosition;
        public bool walkable;
         
        public float gCost;
        public float hCost;

        public void CreateNode()
        {
            parent = -1;
            xPosition = 0;
            yPosition = 0;
            walkable = false;
        }

        public void CreateNode(int isParent, int xPos, int yPos, bool isWalkable)
        {
            parent = isParent;
            xPosition = xPos;
            yPosition = yPos;
            walkable = isWalkable;
        }

        public float GetFCost()  
        { 
            return gCost + hCost; 
        }
    };

    public class TileWorldPathAlgorythm : MonoBehaviour
    {
        Dictionary<int, Node> visitedNodes = new();
        List<int> openList = new();
        List<int> closedList = new();

        float GetHeuristic(int startX, int startY, int endX, int endY)
        {
            return Mathf.Abs(endX - startX) + Mathf.Abs(endY - startY);
        }


        //List<Vector2> GetPath(int startX, int startY, int endX, int endY, Tilemap world)
        public List<Vector2> GetPath(Vector2 start, Vector2 end, Tilemap world)
        {
            List<Vector2> pathPositions = new();

            // Clear open and closed lists
            openList.Clear();
            closedList.Clear();
            visitedNodes.Clear(); // Ensure visitedNodes are cleared properly

            // Create the start and end nodes
            Node startNode = new();
            startNode.CreateNode(-1, (int)start.x, (int)start.y, true); // world->GetTileAt(startX, startY)->walkable); //parent index is -1 as indicator
            startNode.gCost = 0;
            startNode.hCost = GetHeuristic((int)start.x, (int)start.y, (int)end.x, (int)end.y);
            startNode.myIndex = world.GetTileAt((int)start.x, (int)start.y).index;

            // Push the start node to the visited and open lists
            visitedNodes[startNode.myIndex] = startNode;
            openList.Add(startNode.myIndex);

            // Pathfinding loop
            while (openList.Any())
            {
                // Sort the open list based on FCost (g + h)
                openList = openList.OrderBy(a => visitedNodes[a].GetFCost()).ToList();


                // Get the node with the lowest FCost
                Node currentNode = visitedNodes[openList[0]];
                openList.RemoveAt(0);
                closedList.Add(currentNode.myIndex);

                // If we've reached the end node, reconstruct the path
                if (currentNode.xPosition == (int)end.x && currentNode.yPosition == (int)end.y)
                {
                    while (currentNode.parent != -1)
                    {
                        pathPositions.Add(new(currentNode.xPosition, currentNode.yPosition));
                        currentNode = visitedNodes[currentNode.parent];
                    }

                    pathPositions.Add(new((int)start.x, (int)start.y));
                    //pathPositions.Add(new((int)end.x, (int)end.y));
                    pathPositions.Reverse(); // Reverse to get the correct order

                    return pathPositions;
                }

                // Get the neighboring tiles of the current node
                List<Tile> neighborTiles = world.GetSurroundingTiles(currentNode.xPosition, currentNode.yPosition);

                foreach (var neighbor in neighborTiles)
                {
                    if (!neighbor.walkable) continue; // Skip unwalkable tiles
                    if (closedList.Contains(neighbor.index)) continue;


                    // Calculate the GCost for the neighbor
                    float neighborGCost = currentNode.gCost + 1;

                    if (openList.Contains(neighbor.index))
                    {
                        if (neighborGCost < visitedNodes[neighbor.index].gCost) // If this GCost is better, update it
                        {
                            Node old = visitedNodes[neighbor.index];
                            old.gCost = neighborGCost;
                            old.parent = currentNode.myIndex;
                            visitedNodes[neighbor.index] = old;
                        }
                    }
                    else // If neighbor is not in open list, add it
                    {
                        Node newNeighbor = new();
                        newNeighbor.CreateNode(currentNode.myIndex, neighbor.xPos, neighbor.yPos, true);
                        newNeighbor.gCost = neighborGCost;
                        newNeighbor.hCost = GetHeuristic(neighbor.xPos, neighbor.yPos, (int)end.x, (int)end.y);
                        newNeighbor.myIndex = world.GetTileAt(neighbor.xPos, neighbor.yPos).index;

                        visitedNodes[newNeighbor.myIndex] = newNeighbor;

                        openList.Add(newNeighbor.myIndex); // Add to open list
                    }
                }
            }

            Debug.Log("no path found");
            return new List<Vector2>(); // Return empty list if no path is found

        }

    }
}