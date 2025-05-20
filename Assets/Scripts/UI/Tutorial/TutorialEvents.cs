using System;
using UnityEngine;

namespace ProjectColombo.Tutorial
{
    public class TutorialEvents : MonoBehaviour
    {
        public static event Action OnDummyHit;
        public static event Action OnDummyDeath;

        public static void DummyHit()
        {
            OnDummyHit?.Invoke();
        }

        public static void DummyDied()
        {
            OnDummyDeath?.Invoke();
        }
    }
}