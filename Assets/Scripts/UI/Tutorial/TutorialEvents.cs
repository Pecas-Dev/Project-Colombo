using System;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialEvents : MonoBehaviour
    {
        public static event Action OnEnemyHit;

        public static void EnemyHit()
        {
            OnEnemyHit?.Invoke();
        }
    }
}