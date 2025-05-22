using UnityEngine;
using System.Collections.Generic;

namespace ProjectColombo.UI
{
    public class DialogFunctions : MonoBehaviour
    {
        public List<GameObject> setActiveSlot;
        public List<GameObject> setInactiveSlot;

        public void SetActive()
        {
            foreach (var g in setActiveSlot)
            {
                g.SetActive(true);
            }
        }

        public void SetInactive()
        {
            foreach (var g in setInactiveSlot)
            {
                g.SetActive(false);
            }
        }
    }
}
