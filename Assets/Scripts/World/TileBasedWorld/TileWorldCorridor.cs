using System;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class TileWorldCorridor : MonoBehaviour
    {
        int TILESIZE = 20;
        public void PlaceICorridor(Tile tile)
        {
            Quaternion rotation = transform.rotation;
            Vector3 position = new Vector3(tile.position.x - TILESIZE/2, 0, tile.position.y - TILESIZE / 2);

            if (tile.openings.Contains(Directions.NORTH))
            {
                rotation *= Quaternion.Euler(0, 90, 0);
            }

            Instantiate(this.gameObject, position, rotation);
        }


        public void PlaceLCorridor(Tile tile)
        {
            Quaternion rotation = transform.rotation;
            Vector3 position = new Vector3(tile.position.x - TILESIZE / 2, 0, tile.position.y - TILESIZE / 2);

            if (tile.openings.Contains(Directions.WEST) && tile.openings.Contains(Directions.NORTH))
                rotation *= Quaternion.Euler(0, 0, 0);
            else if (tile.openings.Contains(Directions.NORTH) && tile.openings.Contains(Directions.EAST))
                rotation *= Quaternion.Euler(0, 90, 0);
            else if (tile.openings.Contains(Directions.EAST) && tile.openings.Contains(Directions.SOUTH))
                rotation *= Quaternion.Euler(0, 180, 0);
            else if (tile.openings.Contains(Directions.SOUTH) && tile.openings.Contains(Directions.WEST))
                rotation *= Quaternion.Euler(0, 270, 0);

            Instantiate(this.gameObject, position, rotation);
        }

        public void PlaceTCorridor(Tile tile)
        {
            Quaternion rotation = transform.rotation * Quaternion.Euler(0,180,0);
            Vector3 position = new Vector3(tile.position.x - TILESIZE / 2, 0, tile.position.y - TILESIZE / 2);

            foreach (Directions d in Enum.GetValues(typeof(Directions)))
            {
                if (!tile.openings.Contains(d))
                {
                    Instantiate(this.gameObject, position, rotation);
                    break;
                }

                rotation *= Quaternion.Euler(0, 90, 0);
            }
        }

        public void PlaceXCorridor(Tile tile)
        {
            Vector3 position = new Vector3(tile.position.x - TILESIZE / 2, 0, tile.position.y - TILESIZE / 2);
            Instantiate(this.gameObject, position, transform.rotation);
        }
    }
}