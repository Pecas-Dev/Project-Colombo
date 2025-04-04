using UnityEngine;
using System.Collections.Generic;
using DG.DemiLib;

namespace ProjectColombo.LevelManagement
{
    public class TileWorldChamber : MonoBehaviour
    {
        int TILESIZE = 20;

        public Vector2 chamberSize = new(1,1);
        public bool isShop = false;
        public bool isLooting = false;
        public List<GameObject> entrances;
        public List<GameObject> exits;
        public List<GameObject> spawnPoints;


        List<Vector2> entrancesLocal; //for local position in tile coords
        List<Directions> entranceDir; //for directions
        List<Vector2> exitsLocal; //for local position in tile coords
        List<Directions> exitDir; //for directions


        public void Initialize()
        {
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
                Vector2 localPos = entrance.transform.position - transform.position; //get relative Position
                localPos = localPos / TILESIZE; //get to tile coord size
                
                localPos.x = localPos.x < 0 ? Mathf.Ceil(localPos.x) : Mathf.Floor(localPos.x); //round to int
                localPos.y = localPos.y < 0 ? Mathf.Ceil(localPos.y) : Mathf.Floor(localPos.y); //round to int


                entrancesLocal.Add(localPos);
                entranceDir.Add((Directions)((entrance.transform.eulerAngles.y+180) % 360));
                entrance.SetActive(false);
            }

            foreach (GameObject exit in exits)
            {
                Vector2 localPos = exit.transform.position - transform.position; //get relative Position
                localPos = localPos / TILESIZE; //get to tile coord size

                localPos.x = localPos.x < 0 ? Mathf.Ceil(localPos.x) : Mathf.Floor(localPos.x); //round to int
                localPos.y = localPos.y < 0 ? Mathf.Ceil(localPos.y) : Mathf.Floor(localPos.y); //round to int

                exitsLocal.Add(localPos);
                exitDir.Add((Directions)exit.transform.eulerAngles.y);
                exit.SetActive(false);
            }
        }

        public void ActivateChamber()
        {
            foreach (GameObject spawner in spawnPoints)
            {
                spawner.SetActive(true);
            }
        }

        Vector2 LocalToWorldCoord(Vector2 local, Vector2 reference)
        {
            return reference - local;
        }

        //get all entrance tile coords in reference to world position
        public PosDir GetEntranceCoord(Vector2 position)
        {
            Vector2 pos = LocalToWorldCoord(entrancesLocal[0], position);
            PosDir result = new(pos, entranceDir[0]);

            return result;
        }
        //public List<PosDir> GetEntranceCoord(Vector2 position)
        //{
        //    List<PosDir> result = new();

        //    for (int i = 0; i < entrancesLocal.Count; i++)
        //    {
        //        Vector2 pos = LocalToWorldCoord(entrancesLocal[i], position);
        //        result.Add(new(pos, entranceDir[i]));
        //    }

        //    return result;
        //}

        //get all exit tile coords in reference to world position
        public PosDir GetExitCoord(Vector2 position)
        {
            Vector2 pos = LocalToWorldCoord(exitsLocal[0], position);
            PosDir result = new(pos, exitDir[0]);

            return result;
        }
        //public List<PosDir> GetExitCoord(Vector2 position)
        //{
        //    List<PosDir> result = new();

        //    for (int i = 0; i < exitsLocal.Count; i++)
        //    {
        //        Vector2 pos = LocalToWorldCoord(exitsLocal[i], position);
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
                Vector2 entranceTile = new PosDir(LocalToWorldCoord(entrancesLocal[0], position), entranceDir[0]).GetRealPos();
                map.map[(int)entranceTile.x, (int)entranceTile.y].openings.Add(entranceDir[0]);
            }

            if (exitsLocal.Count != 0)
            {
                Vector2 exitTile = new PosDir(LocalToWorldCoord(exitsLocal[0], position), exitDir[0]).GetRealPos();
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