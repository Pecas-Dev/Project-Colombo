using UnityEngine;
using System.Collections.Generic;
using DG.DemiLib;
using ProjectColombo.GameManagement;
using Unity.VisualScripting;

namespace ProjectColombo.LevelManagement
{
    public class TileWorldChamber : MonoBehaviour
    {
        int tilesize = GameGlobals.TILESIZE;

        public Vector2 chamberSize = new(1,1);
        public bool isShop = false;
        public bool isLooting = false;
        public List<GameObject> entrances;
        public List<GameObject> exits;
        public List<GameObject> spawnPoints;

        [HideInInspector] public Vector2 chamberTilePosition = new();
        [HideInInspector] public bool entrancesConnected = false;
        [HideInInspector] public bool exitsConnected = false;
        bool isActive = false;
        float timer = 0;
        float checkIntervall = 1f;

        List<Vector2> entrancesLocal; //for local position in tile coords
        List<Directions> entranceDir; //for directions
        List<Vector2> exitsLocal; //for local position in tile coords
        List<Directions> exitDir; //for directions

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                Initialize(transform.position);
                ActivateChamber();
            }


            if (isActive)
            {
                timer += Time.deltaTime;

                if (timer >= checkIntervall)
                {
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                    if (enemies.Length <= 0)
                    {
                        foreach (GameObject exit in exits)
                        {
                            exit.GetComponent<BoxCollider>().isTrigger = true;
                        }

                        isActive = false;
                    }

                    timer = 0;
                }
            }
        }

        public void Initialize(Vector2 pos)
        {
            chamberTilePosition = pos;

            entrancesLocal = new();
            entranceDir = new();
            exitsLocal = new();
            exitDir = new();

            foreach (GameObject spawner in spawnPoints)
            {
                spawner.SetActive(false);
            }


            foreach (GameObject entrance in entrances)
            {
                Vector3 localPosVec3 = entrance.transform.position; //get relative Position

                Vector2 localPos = new( localPosVec3.x, localPosVec3.z ); //translate to x,y
                localPos = localPos / tilesize; //get to tile coord size

                localPos.x = Mathf.RoundToInt(localPos.x); //round to int
                localPos.y = Mathf.RoundToInt(localPos.y); //round to int

                //Debug.Log("Entrance: " + entrance.transform.position + "Gloal: " + chamberTilePosition + localPos);

                entrancesLocal.Add(localPos);
                entranceDir.Add((Directions)((entrance.transform.eulerAngles.y+180) % 360));
                //Debug.Log(entranceDir[0] + ": " + GetEntranceCoord().GetRealPos());
                entrance.GetComponent<MeshRenderer>().enabled = false ;
                entrance.GetComponent<BoxCollider>().isTrigger = true;
            }

            foreach (GameObject exit in exits)
            {
                Vector3 localPosVec3 = exit.transform.position; //get relative Position

                Vector2 localPos = new(localPosVec3.x, localPosVec3.z); //translate to x,y
                localPos = localPos / tilesize; //get to tile coord size

                localPos.x = Mathf.RoundToInt(localPos.x); //round to int
                localPos.y = Mathf.RoundToInt(localPos.y); //round to int

                //Debug.Log("Exit: " + exit.transform.position + "Global: " + chamberTilePosition + localPos);

                exitsLocal.Add(localPos);
                exitDir.Add((Directions)exit.transform.eulerAngles.y);
                //Debug.Log(exitDir[0] + ": " + GetExitCoord().GetRealPos());
                exit.GetComponent<MeshRenderer>().enabled = false;
                exit.GetComponent<BoxCollider>().isTrigger = false; //all exits are locked from the get go
            }
        }

        public void ActivateChamber()
        {
            foreach (GameObject spawner in spawnPoints)
            {
                spawner.SetActive(true);
            }

            foreach (GameObject entrance in entrances)
            {
                entrance.GetComponent<BoxCollider>().isTrigger = false;
            }

            foreach (GameObject exit in exits)
            {
                exit.GetComponent<BoxCollider>().isTrigger = false;
            }

            isActive = true;
        }

        Vector2 LocalToWorldCoord(Vector2 local)
        {
            return chamberTilePosition + local;
        }

        //get all entrance tile coords in reference to world position
        public PosDir GetEntranceCoord()
        {
            if (entrancesLocal.Count == 0)
            {
                return new(new(0, 0), Directions.NORTH);
            }

            Vector2 pos = LocalToWorldCoord(entrancesLocal[0]);

            PosDir result = new(pos, entranceDir[0]);

            return result;
        }
        //public List<PosDir> GetEntranceCoord()
        //{
        //    List<PosDir> result = new();

        //    for (int i = 0; i < entrancesLocal.Count; i++)
        //    {
        //        Vector2 pos = LocalToWorldCoord(entrancesLocal[i], chamberTilePosition);
        //        result.Add(new(pos, entranceDir[i]));
        //    }

        //    return result;
        //}

        //get all exit tile coords in reference to world position
        public PosDir GetExitCoord()
        {
            if (exitsLocal.Count == 0)
            {
                return new(new(0, 0), Directions.NORTH);
            }

            Vector2 pos = LocalToWorldCoord(exitsLocal[0]);
            PosDir result = new(pos, exitDir[0]);

            return result;
        }
        //public List<PosDir> GetExitCoord()
        //{
        //    List<PosDir> result = new();

        //    for (int i = 0; i < exitsLocal.Count; i++)
        //    {
        //        Vector2 pos = LocalToWorldCoord(exitsLocal[i], chamberTilePosition);
        //        result.Add(new(pos, exitDir[i]));
        //    }

        //    return result;
        //}

        public bool CheckAndBlockOnTilemap(Vector2 position, Tilemap map)
        {

            //check entrance
            if (entrancesLocal.Count != 0)
            {
                Vector2 entranceTile = new PosDir(position + entrancesLocal[0], entranceDir[0]).GetRealPos();

                if (entranceTile.x < 0 || entranceTile.y < 0 || entranceTile.x > map.width || entranceTile.y > map.height)
                {
                    return false;
                }

                if (!map.map[(int)entranceTile.x, (int)entranceTile.y].walkable) return false;
            }

            //check exit
            if (exitsLocal.Count != 0)
            {
                Vector2 exitTile = new PosDir(position + exitsLocal[0], exitDir[0]).GetRealPos();

                if (exitTile.x < 0 || exitTile.y < 0 || exitTile.x > map.width || exitTile.y > map.height)
                {
                    return false;
                }

                if (!map.map[(int)exitTile.x, (int)exitTile.y].walkable) return false;
            }


            //position will be the middle of chamber? might be entrance but middle for now
            int topLeftX = Mathf.CeilToInt(position.x - chamberSize.x / 2);
            int topLeftY = Mathf.CeilToInt(position.y - chamberSize.y / 2);

            // Ensure we're within the bounds of the map
            if (topLeftX < 0 || topLeftY < 0 || topLeftX + chamberSize.x > map.width || topLeftY + chamberSize.y > map.height)
            {
                return false;
            }

            //check if the space is available
            for (int x = 0; x < chamberSize.x; x++)
            {
                for (int y = 0; y < chamberSize.y; y++)
                {
                    if(!map.map[topLeftX + x, topLeftY + y].walkable)
                    {
                        return false;
                    }
                }
            }


            //mark entrance and exit
            if (entrancesLocal.Count != 0)
            {
                Vector2 entranceTile = new PosDir(LocalToWorldCoord(entrancesLocal[0]), entranceDir[0]).GetRealPos();
                map.map[(int)entranceTile.x, (int)entranceTile.y].openings.Add((Directions)(((int)entranceDir[0] + 180) % 360));
            }

            if (exitsLocal.Count != 0)
            {
                Vector2 exitTile = new PosDir(LocalToWorldCoord(exitsLocal[0]), exitDir[0]).GetRealPos();
                map.map[(int)exitTile.x, (int)exitTile.y].openings.Add((Directions)(((int)exitDir[0] + 180) % 360));
            }

            for (int x = 0; x < chamberSize.x; x++)
            {
                for (int y = 0; y < chamberSize.y; y++)
                {
                    map.map[topLeftX + x, topLeftY + y].walkable = false;
                }
            }

            return true;
        }
    }
}