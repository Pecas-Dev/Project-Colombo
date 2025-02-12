using UnityEngine;

namespace ProjectColombo.LevelManagement
{
    public class ActivateChamberTrigger : MonoBehaviour
    {
        ChamberData myChamberData;

        private void Start()
        {
            myChamberData = GetComponentInParent<ChamberData>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                myChamberData.ActivateSpawners();
                myChamberData.LockEntrance();
            }
        }
    }
}