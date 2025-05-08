using ProjectColombo.Enemies.DropSystem;
using ProjectColombo.GameManagement;
using UnityEngine;

namespace ProjectColombo.Objects.Decorations
{
    public class ChestAnimationEvent : MonoBehaviour
    {
        public GameObject pickUpPos;

        public void DropItem()
        {
            pickUpPos.GetComponent<DropSystem>().DropItem();
            Destroy(pickUpPos);
        }
    }
}