using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;


namespace ProjectColombo.LevelManagement
{
    public class BossRoomTurnOff : MonoBehaviour
    {
        public List<GameObject> obstructingElements;

        private void Start()
        {
            GetAllChildren(transform);

            foreach (GameObject g in obstructingElements)
            {
                g.SetActive(false);
            }
        }

        void GetAllChildren(Transform parent)
        {
            foreach (Transform child in parent)
            {
                obstructingElements.Add(child.gameObject);
                GetAllChildren(child); // Recursively add this child's children
            }
        }
    }
}