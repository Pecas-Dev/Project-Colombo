using UnityEngine;
using System.Collections.Generic;
using ProjectColombo.StateMachine.Mommotti;
using ProjectColombo.GameManagement.Events;
using System.Collections;

namespace ProjectColombo.Enemies
{
    public class EnemyAttackPriority : MonoBehaviour
    {
        public List<GameObject> currentEnemies = new();
        public int attackersAtTheSameTime = 2;
        public List<GameObject> currentAttackerEnemies = new();

        public float intervall = 3f;
        float timer;

        public void Activate() //this will be called externally
        {
            CustomEvents.OnEnemyAttack += OnEnemyAttack;
            CustomEvents.OnChamberFinished += Finished;
            CustomEvents.OnEnemyDeath += EnemyDied;
        }

        private void Update()
        {
            timer += Time.deltaTime;

            if (timer > intervall)
            {
                timer = 0;
                SetAttackers();
            }
        }

        private void EnemyDied(GameGlobals.MusicScale obj, GameObject enemy)
        {
            RemoveFromEnemies(enemy);
        }

        private void OnEnemyAttack(GameObject enemy)
        {
            currentAttackerEnemies.Remove(enemy);

            StartCoroutine(AddOldAttacker(enemy));
        }

        IEnumerator AddOldAttacker(GameObject enemy)
        {
            yield return new WaitForSeconds(0.5f);

            if (enemy != null && !currentEnemies.Contains(enemy) && !currentAttackerEnemies.Contains(enemy))
            {
                AddToEnemies(enemy);
            }
        }


        private void Finished()
        {
            CustomEvents.OnEnemyAttack -= OnEnemyAttack;
            CustomEvents.OnChamberFinished -= Finished;
            CustomEvents.OnEnemyDeath -= EnemyDied;
        }

        public void AddToEnemies(GameObject newEnemy)
        {
            if (currentAttackerEnemies.Contains(newEnemy))
            {
                currentAttackerEnemies.Remove(newEnemy);
            }

            if (!currentEnemies.Contains(newEnemy))
            {
                currentEnemies.Add(newEnemy);
            }
        }

        public void RemoveFromEnemies(GameObject enemy)
        {
            if (currentEnemies.Contains(enemy)) 
            { 
                currentEnemies.Remove(enemy); 
            }

            if (currentAttackerEnemies.Contains(enemy))
            {
                currentAttackerEnemies.Remove(enemy);
            }
        }

        public void SetAttackers()
        {
            // Defensive cleanup
            currentAttackerEnemies.RemoveAll(enemy => enemy == null);
            currentEnemies.RemoveAll(enemy => enemy == null);


            if (currentAttackerEnemies.Count < attackersAtTheSameTime && currentEnemies.Count > 0)
            {
                GameObject newAttacker = currentEnemies[0];
                newAttacker.GetComponent<MommottiStateMachine>().SetAttackingState();
                currentEnemies.RemoveAt(0);
                currentAttackerEnemies.Add(newAttacker);
            }
        }
    }
}