using UnityEngine;
using System.Collections.Generic;
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
            //Debug.Log(pos + ", " + dir);
        }

        public Vector2 GetRealPos()
        {
            Vector2 pos = new();
            switch (direction)
            {
                case Directions.EAST:
                    pos = new(position.x + 1, position.y);
                    break;
                case Directions.SOUTH:
                    pos = new(position.x, position.y - 1);
                    break;
                case Directions.WEST:
                    pos = new(position.x - 1, position.y);
                    break;
                case Directions.NORTH:
                    pos = new(position.x, position.y + 1);
                    break;
            }

            //Debug.Log("Real Position: " + position + pos);
            return pos;
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
        int tilesize;

        public Tile[,] map;
        public int height;
        public int width;
        public bool walkable;

        public void CreateTilemap(int w, int h)
        {
            tilesize = GameGlobals.TILESIZE;
            width = w;
            height = h;

            map = new Tile[width, height];

            float halfTileSize = tilesize / 2;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector2 position = new(halfTileSize + x * tilesize, halfTileSize + y * tilesize);
                    map[x, y].CreateTile(new(halfTileSize + x * tilesize, halfTileSize + y * tilesize));
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
        int tilesize = GameGlobals.TILESIZE;

        //data
        int worldHeight = 25;
        int worldWidth = 0;
        int chamberOffset = 4;

        public List<GameObject> chamberVariants;
        public List<GameObject> ICorridors;
        public List<GameObject> LCorridors;
        public List<GameObject> TCorridors;
        public List<GameObject> XCorridors;
        public GameObject startChamber;
        public GameObject endChamber;
        public List<int> layersOfChambers;

        List<List<GameObject>> chamberLayers = new();

        Tilemap world;
        TileWorldPathAlgorythm algorythm;
        List<List<Vector2>> paths = new();
        List<GameObject> createdChambers = new();

        private void Start()
        {
            algorythm = GetComponent<TileWorldPathAlgorythm>();
            worldWidth = 2 * chamberOffset + layersOfChambers.Count * chamberOffset;

            world.CreateTilemap(worldWidth, worldHeight);

            Vector2 startChamberTilePos = new(1, Mathf.RoundToInt(worldHeight / 2));
            TryToMakeChamber(startChamber, startChamberTilePos, createdChambers);
            createdChambers[0].GetComponent<TileWorldChamber>().ActivateChamber(); //activate start chamber

            Vector2 endChamberTilePos = new(chamberOffset + layersOfChambers.Count * chamberOffset, Mathf.RoundToInt(worldHeight / 2));
            TryToMakeChamber(endChamber, endChamberTilePos, createdChambers);

            for (int layer = 0; layer < layersOfChambers.Count; layer++)
            {
                List<GameObject> currentLayer = new();
                int chamberCount = layersOfChambers[layer];

                int posX = chamberOffset * layer + chamberOffset; //start + currentlayer
                int posY = (int)(startChamberTilePos.y - (chamberCount / 2f) * chamberOffset + 3); //move down to centralize
                
                //create  layer chambers
                for (int i = 0; i < chamberCount; i++)
                {
                    Vector2 position = new(posX, posY);
                    int index = Random.Range(0, chamberVariants.Count);
                    TryToMakeChamber(chamberVariants[index], position, currentLayer);
                    posY += chamberOffset;
                }

                chamberLayers.Add(currentLayer);
            }

            //connect first layer to entrance
            foreach (GameObject c in chamberLayers[0])
            {
                paths.Add(CreatePath(createdChambers[0], c));
            }

            //connect layers to one another
            for (int i = 0; i < chamberLayers.Count - 1; i++)
            {
                ConnectLayers(chamberLayers[i], chamberLayers[i + 1]);
            }

            //connect last layer to exit
            foreach (GameObject c in chamberLayers[^1]) //^1 means last
            {
                paths.Add(CreatePath(c, createdChambers[1]));
            }

            MakeCorridors();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.O))
            {
                ReRollWorld();
            }
        }

        void ConnectLayers(List<GameObject> firstLayer, List<GameObject> secondLayer)
        {
            int currentIndexFirst = 0;
            int currentIndexSecond = 0;

            //Debug.Log("firstlayer: " + firstLayer.Count + ", " + currentIndexFirst);
            //Debug.Log("firstlayer: " + secondLayer.Count + ", " + currentIndexSecond);

            while (currentIndexFirst < firstLayer.Count || currentIndexSecond < secondLayer.Count)
            {
                //in case one is smaller as the other they need to be set back to their last chamber
                if (currentIndexFirst == firstLayer.Count) currentIndexFirst--;
                if (currentIndexSecond == secondLayer.Count) currentIndexSecond--;

                //Debug.Log("firstlayer: " + firstLayer.Count + ", " + currentIndexFirst);
                //Debug.Log("firstlayer: " + secondLayer.Count + ", " + currentIndexSecond);

                paths.Add(CreatePath(firstLayer[currentIndexFirst], secondLayer[currentIndexSecond]));

                currentIndexFirst++;
                currentIndexSecond++;
            }
        }

        List<Vector2> CreatePath(GameObject start, GameObject end)
        {
            PosDir startExit = start.GetComponent<TileWorldChamber>().GetExitCoord();
            PosDir endEntrance = end.GetComponent<TileWorldChamber>().GetEntranceCoord();

            start.GetComponent<TileWorldChamber>().exitsConnected = true;
            end.GetComponent<TileWorldChamber>().entrancesConnected = true;

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
                    world.GetTileAt(parentX, parentY).SetEntrace(Directions.SOUTH);
                    world.GetTileAt(currentX, currentY).SetEntrace(Directions.NORTH);
                }
                else if (currentY > parentY)
                {
                    world.GetTileAt(parentX, parentY).SetEntrace(Directions.NORTH);
                    world.GetTileAt(currentX, currentY).SetEntrace(Directions.SOUTH);
                }
            }
        }

        void TryToMakeChamber(GameObject chamber, Vector2 position, List<GameObject> list)
        {
            GameObject result = Instantiate(chamber, transform.position, transform.rotation);
            TileWorldChamber myChamber = result.GetComponent<TileWorldChamber>();

            //align even chamberes
            if (myChamber.chamberSize.x % 2 == 0) result.transform.position = new((float)(result.transform.position.x - tilesize / 2f), result.transform.position.y, result.transform.position.z);
            if (myChamber.chamberSize.y % 2 == 0) result.transform.position = new(result.transform.position.x, result.transform.position.y, (float)(result.transform.position.z - tilesize / 2f));

            myChamber.Initialize(position);

            if (myChamber.CheckAndBlockOnTilemap(position, world))
            {
                Vector3 startPos = new Vector3(position.x * (float)tilesize, 0, position.y * (float)tilesize);
                result.transform.position = result.transform.position + startPos;
                list.Add(result);
            }
            else
            {
                Destroy(result);
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

                    Vector3 pos = new Vector3(x * tilesize, 0, y * tilesize);

                    if (openings == 2)
                    {
                        if (world.GetTileAt(x,y).openings.Contains(Directions.NORTH) && world.GetTileAt(x, y).openings.Contains(Directions.SOUTH) 
                            || world.GetTileAt(x, y).openings.Contains(Directions.EAST) && world.GetTileAt(x, y).openings.Contains(Directions.WEST))
                        {
                            int randI = Random.Range(0, ICorridors.Count);
                            ICorridors[randI].GetComponent<TileWorldCorridor>().PlaceICorridor(world.GetTileAt(x,y));
                            continue;
                        }

                        int rand = Random.Range(0, LCorridors.Count);
                        LCorridors[rand].GetComponent<TileWorldCorridor>().PlaceLCorridor(world.GetTileAt(x, y));
                    }
                    else if (openings == 3)
                    {
                        int rand = Random.Range(0, TCorridors.Count);
                        TCorridors[rand].GetComponent<TileWorldCorridor>().PlaceTCorridor(world.GetTileAt(x, y));
                    }
                    else if (openings == 4)
                    {
                        int rand = Random.Range(0, XCorridors.Count);
                        XCorridors[rand].GetComponent<TileWorldCorridor>().PlaceXCorridor(world.GetTileAt(x, y));
                    }
                    else if (openings == 1)
                    {
                        //Debug.Log("only one opening: " + x + ", " + y);
                    }
                    else
                    {
                        //Debug.Log("too many openings: " + x + ", " + y);
                    }
                }
            }
        }

        private void OnDrawGizmos()
        {
            if (world.map == null) return;

            float gizmoHeight = 0.2f;
            float halfTile = tilesize / 2f;
            float openingLength = tilesize * 0.3f;

            for (int y = 0; y < worldHeight; y++)
            {
                for (int x = 0; x < worldWidth; x++)
                {
                    Tile tile = world.map[x, y];
                    Vector3 center = new Vector3(tile.position.x - tilesize / 2, gizmoHeight, tile.position.y - tilesize / 2);

                    // Draw tile base
                    Gizmos.color = tile.walkable ? Color.green : Color.red;
                    Gizmos.DrawWireCube(center, new Vector3(tilesize * 0.9f, 0.1f, tilesize * 0.9f));

                    if (!tile.walkable) continue;

                    // Draw openings
                    Gizmos.color = Color.blue;

                    foreach (Directions dir in tile.openings)
                    {
                        Vector3 direction = dir switch
                        {
                            Directions.NORTH => Vector3.forward,
                            Directions.SOUTH => Vector3.back,
                            Directions.EAST => Vector3.right,
                            Directions.WEST => Vector3.left,
                            _ => Vector3.zero
                        };

                        Gizmos.DrawLine(center, center + direction * openingLength);
                    }
                }
            }

            // Draw chamber entrances and exits (yellow cubes)
            foreach (var chamber in createdChambers)
            {
                TileWorldChamber c = chamber.GetComponent<TileWorldChamber>();
                if (c == null) continue;

                Vector2 basePos = new Vector2(
                    Mathf.RoundToInt(chamber.transform.position.x / tilesize),
                    Mathf.RoundToInt(chamber.transform.position.z / tilesize)
                );

                // Entrance
                PosDir entrance = c.GetEntranceCoord();
                Vector3 entranceRealPos = new Vector3(entrance.GetRealPos().x * tilesize, gizmoHeight, entrance.GetRealPos().y * tilesize);
                Gizmos.color = Color.yellow;
                Gizmos.DrawCube(entranceRealPos, Vector3.one * 2);

                // Exit
                PosDir exit = c.GetExitCoord();
                Vector3 exitRealPos = new Vector3(exit.GetRealPos().x * tilesize, gizmoHeight, exit.GetRealPos().y * tilesize);
                Gizmos.DrawCube(exitRealPos, Vector3.one * 2);
            }
        }

        void ReRollWorld()
        {
            // Destroy all previously generated chambers
            foreach (GameObject chamber in createdChambers)
            {
                Destroy(chamber);
            }
            createdChambers.Clear(); // Clear the chamber list

            foreach (var layer in chamberLayers)
            {
                foreach (GameObject chamber in layer)
                {
                    Destroy(chamber);
                }
            }

            chamberLayers = new();

            // Destroy all GameObjects with TileWorldCorridor attached to them
            TileWorldCorridor[] allCorridors = FindObjectsByType<TileWorldCorridor>(FindObjectsSortMode.None);
            foreach (TileWorldCorridor corridor in allCorridors)
            {
                Destroy(corridor.gameObject); // Destroy the GameObject that holds the TileWorldCorridor script
            }

            // Restart the world generation process
            Start();
        }
    }
}