using UnityEngine;
using System.Collections.Generic;

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
        List<Vector2> exitsLocal; //for local position in tile coords

        public void Initialize()
        {
            entrancesLocal = new();
            exitsLocal = new();

            foreach (GameObject entrance in entrances)
            {
                Vector2 localPos = entrance.transform.position - transform.position; //get relative Position
                localPos = localPos / TILESIZE; //get to tile coord size

                localPos.x = Mathf.Floor(localPos.x); //round to int
                localPos.y = Mathf.Floor(localPos.y);

                entrancesLocal.Add(localPos);
            }

            foreach (GameObject exit in exits)
            {
                Vector2 localPos = exit.transform.position - transform.position; //get relative Position
                localPos = localPos / TILESIZE; //get to tile coord size

                localPos.x = Mathf.Floor(localPos.x); //round to int
                localPos.y = Mathf.Floor(localPos.y);

                exitsLocal.Add(localPos);
            }
        }

        public void ActivateChamber()
        {
            if (spawnPoints == null) return;

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
        public Vector2 GetEntranceCoord(Vector2 position)
        {
            Vector2 result = new();

            result = LocalToWorldCoord(entrancesLocal[0], position);

            return result;
        }
        //public List<Vector2> GetEntranceCoord(Vector2 position)
        //{
        //    List<Vector2> result = new();

        //    foreach (Vector2 local in entrancesLocal)
        //    {
        //        result.Add(LocalToWorldCoord(local, position));
        //    }

        //    return result;
        //}

        //get all exit tile coords in reference to world position
        public Vector2 GetExitCoord(Vector2 position)
        {
            Vector2 result = new();

            result = LocalToWorldCoord(exitsLocal[0], position);

            return result;
        }
        //public List<Vector2> GetExitCoord(Vector2 position)
        //{
        //    List<Vector2> result = new();

        //    foreach (Vector2 local in exitsLocal)
        //    {
        //        result.Add(LocalToWorldCoord(local, position));
        //    }

        //    return result;
        //}

        public bool CheckAndBlockOnTilemap(Vector2 position, Tilemap map)
        {
            bool spaceAvailable = true;

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
                        spaceAvailable = false;
                        return spaceAvailable;
                    }
                }
            }

            //block the space for the chamber
            if (spaceAvailable)
            {
                for (int x = 0; x < chamberSize.x; x++)
                {
                    for (int y = 0; y < chamberSize.y; y++)
                    {
                        map.map[topLeftX + x, topLeftY + y].walkable = true;
                    }
                }
            }

            //return if successfull or not
            return spaceAvailable;
        }
    }
}