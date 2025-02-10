using UnityEngine;


namespace ProjectColombo.Combat
{
    public class WeaponHitboxManager : MonoBehaviour
    {
        [Header("Hitbox References")]
        [SerializeField] CapsuleCollider swordCollider;

        private void Start()
        {
            if (swordCollider == null) return;

            swordCollider.enabled = false;
        }

        public void EnableSwordHitbox()
        {
            if (swordCollider == null) return;

            swordCollider.enabled = true;
        }

        public void DisableSwordHitbox()
        {
            if (swordCollider == null) return;

            swordCollider.enabled = false;
        }
    }
}


