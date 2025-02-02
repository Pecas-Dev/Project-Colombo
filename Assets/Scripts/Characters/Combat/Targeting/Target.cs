using UnityEngine;
using UnityEngine.UI;


namespace ProjectColombo.Combat
{
    public class Target : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Image targetIcon;

        void Start()
        {
            targetIcon.enabled = false;
        }

        public void SetTargetIconActive(bool isActive)
        {
            if (targetIcon != null)
            {
                targetIcon.enabled = isActive;
            }
        }
    }
}
