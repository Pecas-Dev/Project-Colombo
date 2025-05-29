using ProjectColombo.StateMachine.Player;
using System.Drawing;
using UnityEngine;
using UnityEngine.VFX;

namespace ProjectColombo.Combat
{
    public class CursedNoteWave : MonoBehaviour
    {
        public int damage;
        public float timeTillDelete;
        float timer;
        public Collider myCollider;
        bool hit = false;

        bool isMajor;
        public VisualEffect majorVFX;
        public VisualEffect minorVFX;

        private void Start()
        {
            myCollider.enabled = true;
        }
        void Update()
        {
            if (!hit) myCollider.enabled = true;

            timer += Time.deltaTime;
            if (timer >= timeTillDelete)
            {
                Destroy(this.gameObject);
            }

            float vfxRadius = isMajor ? majorVFX.GetFloat("Size") : minorVFX.GetFloat("Size");

            if (myCollider is SphereCollider sphere)
            {
                sphere.radius = vfxRadius;
            }
        }

        public void SetVFX(GameGlobals.MusicScale scale)
        {
            if (scale == GameGlobals.MusicScale.MAJOR)
            {
                isMajor = true;
                majorVFX.Play();
            }
            else if (scale == GameGlobals.MusicScale.MINOR)
            {
                isMajor = false;
                minorVFX.Play();
            }
        }


        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                PlayerStateMachine sm = other.GetComponent<PlayerStateMachine>();

                hit = true;
                myCollider.enabled = false;

                if (sm.currentStateEnum == PlayerStateMachine.PlayerState.Roll ||
                    sm.isParrying || sm.isBlocking)
                {
                    return;
                }

                sm.myHealthManager.TakeDamage(damage);
            }
        }
    }
}