using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using NUnit.Framework.Internal;
using ProjectColombo.Enemies.Pathfinding;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UIElements;

namespace ProjectColombo.LevelManagement
{
    public enum Directions { EAST = 0, SOUTH = 90, WEST = 180, NORTH = 270};
    public struct PosDir
    {
        public Vector2 position;
        public Directions direction;

        public PosDir(Vector2 pos, Directions dir)
        {
            position = pos;
            direction = dir;
        }

        public Vector2 GetRealPos()
        {
            switch (direction)
            {
                case Directions.EAST:
                    return new(position.x + 1, position.y);
                case Directions.SOUTH:
                    return new(position.x, position.y -1);
                case Directions.WEST:
                    return new(position.x - 1, position.y);
                case Directions.NORTH:
                    return new(position.x, position.y +1);
            }

            return position;
        }
    }

    public struct Tile
    {
        public Vector2 position;
        public bool walkable;
        public int xPos;
        public int yPos;
        public int index;

        public List<Directions> openings;

	    public void CreateTile(Vector2 myPosition)
        {
            openings = new();
            position = myPosition;
            walkable = true;
        }

        public void CreateTile(Vector2 myPosition, bool n, bool w, bool s, bool e)
        {
            openings = new();
            position = myPosition;

            if (n) openings.Add(Directions.NORTH);
            if (w) openings.Add(Directions.WEST);
            if (s) openings.Add(Directions.SOUTH);
            if (e) openings.Add(Directions.EAST);
            walkable = true;
        }

        public void SetEntrace(Directions dir)
        {
            if (!openings.Contains(dir))
            {
                openings.Add(dir);
            }
        }
    }

    public struct Tilemap
    {
        int TILESIZE;

        public Tile[,] map;
        public int height;
        public int width;
        public bool walkable;

        public void CreateTilemap(int w, int h)
        {
            TILESIZE = 20;
            width = w;
            height = h;

            map = new Tile[width, height];

            float halfTileSize = TILESIZE / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 position = new(halfTileSize + x * TILESIZE, halfTileSize + y * TILESIZE);
                    map[x, y].CreateTile(new(halfTileSize + x * TILESIZE, halfTileSize + y * TILESIZE));
                    map[x, y].index = x + y * width;
                    map[x, y].xPos = x;
                    map[x, y].yPos = y;
                }
            }
        }


        public Tile GetTileAt(int x, int y)
        {
            return map[x, y];
        }
        public List<Tile> GetSurroundingTiles(int x, int y)
        {
            List<Tile> result = new();

            if (x > 0)
                result.Add(map[x - 1, y]);

            if (x < width - 1)
                result.Add(map[x + 1, y]);

            if (y > 0)
                result.Add(map[x, y - 1]);

            if (y < height - 1)
                result.Add(map[x, y + 1]);

            return result;
        }
    }

    public class TileWorldManager : MonoBehaviour
    {
        int TILESIZE = 20;

        //data
        public int worldWidth = 15;
        public int worldHeight = 15;   

        public List<GameObject> chamberVariants;
        public List<GameObject> ICorridors;
        public List<GameObject> LCorridors;
        public List<GameObject> TCorridors;
        public List<GameObject> XCorridors;
        public GameObject startChamber;
        public GameObject endChamber;

        Tilemap world;
        TileWorldPathAlgorythm algorythm;
        List<List<Vector2>> paths = new();

        private void Start()
        {
            algorythm = GetComponent<TileWorldPathAlgorythm>();
            world.CreateTilemap(worldWidth, worldHeight);

            Vector2 startChamberTilePos = new(2, 5);
            MakeChamber(startChamber, startChamberTilePos);

            Vector2 endChamberTilePos = new(10, 12);
            MakeChamber(endChamber, endChamberTilePos);

            Vector2 otherChamberTilePos = new(8, 3);
            MakeChamber(chamberVariants[3], otherChamberTilePos);

            
            paths.Add(CreatePath(startChamber, startChamberTilePos, endChamber, endChamberTilePos));
            paths.Add(CreatePath(startChamber, startChamberTilePos, chamberVariants[0], otherChamberTilePos));
            paths.Add(CreatePath(chamberVariants[0], otherChamberTilePos, endChamber, endChamberTilePos));

            MakeCorridors();
        }


        List<Vector2> CreatePath(GameObject start, Vector2 startPos, GameObject end, Vector2 endPos)
        {
            PosDir startExit = start.GetComponent<TileWorldChamber>().GetExitCoord(startPos);
            PosDir endEntrance = end.GetComponent<TileWorldChamber>().GetEntranceCoord(endPos);

            List<Vector2> result = algorythm.GetPath(startExit.GetRealPos(), endEntrance.GetRealPos(), world);

            MarkConnections(result);

            return result;
        }

        void MarkConnections(List<Vector2> path)
        {
            for (int i = 1; i < path.Count; i++) //mark the connections in tiles
            {
                int currentX = (int)path[i].x;
                int currentY = (int)path[i].y;
                int parentX = (int)path[i - 1].x;
                int parentY = (int)path[i - 1].y;

                if (currentX < parentX)
                {
                    world.GetTileAt(parentX, parentY).SetEntrace(Directions.WEST);
                    world.GetTileAt(currentX, currentY).SetEntrace(Directions.EAST);
                }
                else if (currentX > parentX)
                {
                    world.GetTileAt(parentX, parentY).SetEntrace(Directions.EAST);
                    world.GetTileAt(currentX, currentY).SetEntrace(Directions.WEST);
                }
                else if (currentY < parentY)
                {
                    world.GetTileAt(parentX, parentY).SetEntrace(Directions.NORTH);
                    world.GetTileAt(currentX, currentY).SetEntrace(Directions.SOUTH);
                }
                else if (currentY > parentY)
                {
                    world.GetTileAt(parentX, parentY).SetEntrace(Directions.SOUTH);
                    world.GetTileAt(currentX, currentY).SetEntrace(Directions.NORTH);
                }
            }
        }

        void MakeChamber(GameObject chamber, Vector2 position)
        {
            TileWorldChamber startChamberScript = chamber.GetComponent<TileWorldChamber>();
            startChamberScript.Initialize();

            if (startChamberScript.CheckAndBlockOnTilemap(position, world))
            {
                Vector3 startPos = new Vector3(position.x * TILESIZE, 0, position.y * TILESIZE);
                Instantiate(chamber, startPos, transform.rotation);
            }
        }

        void MakeCorridors()
        {
            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    if (!world.GetTileAt(x, y).walkable) continue; //if not walkable no path on tile

                    int openings = world.GetTileAt(x, y).openings.Count;
                    if (openings == 0) 
                    {
                        continue; //if no openings no path on tile
                    }

                    Vector3 pos = new Vector3(x * TILESIZE, 0, y * TILESIZE);

                    if (openings == 2)
                    {
                        if (world.GetTileAt(x,y).openings.Contains(Directions.NORTH) && world.GetTileAt(x, y).openings.Contains(Directions.SOUTH) 
                            || world.GetTileAt(x, y).openings.Contains(Directions.EAST) && world.GetTileAt(x, y).openings.Contains(Directions.WEST))
                        {
                            ICorridors[0].GetComponent<TileWorldCorridor>().PlaceICorridor(world.GetTileAt(x,y));
                            continue;
                        }

                        LCorridors[0].GetComponent<TileWorldCorridor>().PlaceLCorridor(world.GetTileAt(x, y));
                    }
                    else if (openings == 3)
                    {
                        TCorridors[0].GetComponent<TileWorldCorridor>().PlaceTCorridor(world.GetTileAt(x, y));
                    }
                    else if (openings == 4)
                    {
                        XCorridors[0].GetComponent<TileWorldCorridor>().PlaceXCorridor(world.GetTileAt(x, y));
                    }
                    else
                    {
                        Debug.Log("too many openings");
                    }
                }
            }
        }
    }
}