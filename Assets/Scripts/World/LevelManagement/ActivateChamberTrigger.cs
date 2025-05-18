using ProjectColombo.Enemies.Pathfinding;
using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class ActivateChamberTrigger : MonoBehaviour
    {
        TileWorldChamber myChamberData;

        private void Start()
        {
            myChamberData = GetComponentInParent<TileWorldChamber>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                myChamberData.GetComponentInChildren<GridManager>().CreateGrid();
                myChamberData.ActivateChamber();
                DestroyAllDrops();
                gameObject.SetActive(false);
            }
        }

        void DestroyAllDrops()
        {
            GameObject[] drops = GameObject.FindGameObjectsWithTag("Collectable");

            foreach (var d in drops)
            {
                Destroy(d);
            }
        }
    }
}