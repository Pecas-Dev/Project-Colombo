using UnityEngine;
using System.Collections.Generic;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.GameManagement.Events;
using System.Collections;

namespace ProjectColombo.Enemies
{
    public class EnemyAttackPriority : MonoBehaviour
    {
        public List<GameObject> allCurrentEnemies = new();
        public List<GameObject> currentChasingEnemies = new();
        public List<GameObject> currentAttackerEnemies = new();
        public int attackersAtTheSameTime = 2;

        bool isActive = false;
        int currentAttackerIndex = 0;
        public float interval = 3f;
        float timer;

        public void Activate()
        {
            CustomEvents.OnEnemyAttack += OnEnemyAttack;
            CustomEvents.OnChamberFinished += Finished;
            CustomEvents.OnEnemyDeath += EnemyDied;

            isActive = true;
            FillEnemyList(); // One-time population
        }

        private void FillEnemyList()
        {
            allCurrentEnemies.Clear();

            GameObject[] allEnemies = GameObject.FindGameObjectsWithTag("Enemy");
            foreach (GameObject enemy in allEnemies)
            {
                if (enemy != null && !allCurrentEnemies.Contains(enemy))
                {
                    allCurrentEnemies.Add(enemy);
                }
            }
        }

        private void Update()
        {
            if (isActive)
            {
                timer += Time.deltaTime;

                if (timer >= interval)
                {
                    UpdateAllEnemies();
                    if (allCurrentEnemies.Count == 0)
                    {
                        isActive = false;
                    }


                    timer = 0f;
                    SetAttackers();
                }
            }
        }

        private void OnEnemyAttack(GameObject enemy)
        {
            currentAttackerEnemies.Remove(enemy);

            if (allCurrentEnemies.Count > 1)
            {
                SetAttackers();
                timer = 0f;
            }
        }


        private void EnemyDied(GameGlobals.MusicScale scale, GameObject enemy)
        {
            RemoveFromEnemies(enemy);
            timer = interval; //send a new enemy
        }

        private void Finished()
        {
            CustomEvents.OnEnemyAttack -= OnEnemyAttack;
            CustomEvents.OnChamberFinished -= Finished;
            CustomEvents.OnEnemyDeath -= EnemyDied;
        }


        void RemoveFromEnemies(GameObject enemy)
        {
            allCurrentEnemies.Remove(enemy);
            currentChasingEnemies.Remove(enemy);
            currentAttackerEnemies.Remove(enemy);
        }

        void UpdateAllEnemies()
        {
            allCurrentEnemies.RemoveAll(e => e == null);
        }

        void UpdateChasingEnemy()
        {
            currentChasingEnemies.Clear();

            foreach (var e in allCurrentEnemies)
            {
                var sm = e.GetComponent<MommottiStateMachine>();
                if (sm != null)
                {
                    if (sm.currentState == MommottiStateMachine.MommottiState.CHASE)
                    {
                        currentChasingEnemies.Add(e);
                    }
                }
            }
        }

        void UpdateAttackingEnemy()
        {
            for (int i = currentAttackerEnemies.Count - 1; i >= 0; i--)
            {
                GameObject enemy = currentAttackerEnemies[i];
                if (enemy == null)
                {
                    currentAttackerEnemies.RemoveAt(i);
                    continue;
                }

                var sm = enemy.GetComponent<MommottiStateMachine>();
                if (sm == null || sm.currentState != MommottiStateMachine.MommottiState.ATTACK)
                {
                    currentAttackerEnemies.RemoveAt(i);
                }
            }
        }

        void SetAttackers()
        {
            // Cleanup only nulls
            allCurrentEnemies.RemoveAll(e => e == null);

            UpdateAttackingEnemy();
            UpdateChasingEnemy();


            if (currentAttackerEnemies.Count < attackersAtTheSameTime && currentChasingEnemies.Count > 0)
            {
                currentAttackerIndex = (currentAttackerIndex + 1) % currentChasingEnemies.Count;
    
                var newAttacker = currentChasingEnemies[currentAttackerIndex];

                newAttacker.GetComponent<MommottiStateMachine>().SetAttackingState();
                currentAttackerEnemies.Add(newAttacker);
                currentChasingEnemies.Remove(newAttacker);
            }
        }
    }
}
