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
        [HideInInspector]public float moveSpeed;
        [HideInInspector]public float attackSpeed;
        

        
        [Header("Movement Settings")]
        public float rotationSpeedPlayer = 720f;

        [Header("Attack Settings")]
        public float attackImpulseForce = 2.5f;
        public float stunnedTime = 1f;
        [HideInInspector] public GameGlobals.MusicScale currentScale = GameGlobals.MusicScale.NONE;

        private void Start()
        {
            CustomEvents.OnLevelChange += SaveCurrentStats;
            myGlobalStats = GameManager.Instance.gameObject.GetComponent<GlobalStats>();
            myLevelStats = GameObject.Find("WorldGeneration").GetComponent<LevelStats>();
            GetCurrentStats();
        }

        private void OnDestroy()
        {
            CustomEvents.OnLevelChange -= SaveCurrentStats;
        }

        private void SaveCurrentStats()
        {
            if (gameObject.CompareTag("Player"))
            {
                myGlobalStats.currentPlayerSpeed = moveSpeed;
                myGlobalStats.currentPlayerAttackSpeed = attackSpeed;
            }
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
