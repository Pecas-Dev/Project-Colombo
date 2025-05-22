using ProjectColombo.Combat;
using ProjectColombo.GameManagement;
using ProjectColombo.GameManagement.Events;
using ProjectColombo.GameManagement.Stats;
using ProjectColombo.LevelManagement;
using UnityEngine;

namespace ProjectColombo
{
    public class EntityAttributes : MonoBehaviour
    {
        GlobalStats myGlobalStats;
        LevelStats myLevelStats;
        [ReadOnlyInspector]public float moveSpeed;
        [ReadOnlyInspector]public float attackSpeed;
        

        
        [Header("Movement Settings")]
        public float rotationSpeedPlayer = 720f;
        public float rollImpulseForce = 2.5f;

        [Header("Attack Settings")]
        public float attackImpulseForce = 2.5f;
        public float stunnedTime = 1f;
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;

        private void Start()
        {
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();

            myLevelStats = GameObject.Find("WorldGeneration").GetComponent<LevelStats>();
            GetCurrentStats();
        }


        void GetCurrentStats()
        {
            if (gameObject.CompareTag("Player"))
            {
                moveSpeed = myGlobalStats.currentPlayerSpeed;
                attackSpeed = myGlobalStats.currentPlayerAttackSpeed;
                
            }
            else if (gameObject.CompareTag("Enemy"))
            {
                moveSpeed = myLevelStats.currentMommottiSpeed;
            }
            else if (gameObject.CompareTag("Boss"))
            {
                moveSpeed = myLevelStats.defaultBossSpeed;
            }
        }


        public void Destroy()
        {
            Destroy(this.gameObject);
        }

        public void SetScale(GameGlobals.MusicScale scale)
        {
            currentScale = scale;

            GetComponentInChildren<WeaponAttributes>().currentScale = scale;
        }
    }
}
