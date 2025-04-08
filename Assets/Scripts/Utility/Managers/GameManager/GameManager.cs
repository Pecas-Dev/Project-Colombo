using System.Collections.Generic;
using UnityEngine;

namespace ProjectColombo.GameManagement
{
    public enum AllWeapons { SWORD };

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public AllWeapons playerWeapon;
        public List<GameObject> allWeapons;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // Destroy duplicates
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }

        public GameObject GetMyWeapon()
        {
            switch (playerWeapon)
            {
            case AllWeapons.SWORD:
                return allWeapons[0];
            }

            return null;
        }
    }
}