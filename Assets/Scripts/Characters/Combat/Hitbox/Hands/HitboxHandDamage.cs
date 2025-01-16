using UnityEngine;


namespace ProjectColombo.Combat
{
    public class HitboxHandDamage : MonoBehaviour
    {
        [SerializeField] HitboxHandManager manager;

        
        bool isLeftHand = false;


        void Start()
        {
            isLeftHand = gameObject == manager.LeftHandHitbox;
        }

        void OnTriggerEnter(Collider other)
        {
            if (isLeftHand && manager.IsLeftHandActive)
            {
                manager.AddOverlapLeft(other);
            }
            else if (!isLeftHand && manager.IsRightHandActive)
            {
                manager.AddOverlapRight(other);
            }
        }

        void OnTriggerExit(Collider other)
        {
            if (isLeftHand && manager.IsLeftHandActive)
            {
                manager.RemoveOverlapLeft(other);
            }
            else if (!isLeftHand && manager.IsRightHandActive)
            {
                manager.RemoveOverlapRight(other);
            }
        }
    }
}
