using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.AI;
using UnityEngine;
using ProjectColombo.Core;
using ProjectColombo.Control;

namespace ProjectColombo.SceneManagement
{
    public class Portal : MonoBehaviour
    {
        enum DestinationIdentifier
        {
            DI_Portal1,
            DI_Portal2,
            DI_Portal3,
            DI_Portal4
        }

        [Header("Scene Value")]
        [SerializeField] int sceneToLoad = -1;

        [Header("Spawn Point")]
        [SerializeField] Transform spawnPoint;

        [Header("Portal ID")]
        [SerializeField] DestinationIdentifier destinationIdentifier;

        [Header("Fade Times")]
        [SerializeField] float fadeOutTime = 1f;
        [SerializeField] float fadeInTime = 2f;
        [SerializeField] float fadeWaitTime = 0.5f;


        void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                StartCoroutine(Transition());
            }
        }

        IEnumerator Transition()
        {
            if (sceneToLoad < 0) yield break;

            DontDestroyOnLoad(gameObject);

            Fade fade = FindFirstObjectByType<Fade>();

            yield return fade.FadeOut(fadeOutTime);
            yield return SceneManager.LoadSceneAsync(sceneToLoad);

            GameObject player = GameObject.FindWithTag("Player");
            player.GetComponent<ActionSchedueler>().CancelCurrentAction();
            player.GetComponent<PlayerController>().enabled = false;

            Portal otherPortal = GetOtherPortal();
            UpdatePortal(otherPortal);

            yield return new WaitForSeconds(fadeWaitTime);
            yield return fade.FadeIn(fadeInTime);
            player.GetComponent<PlayerController>().enabled = true;


            Destroy(gameObject);
        }

        Portal GetOtherPortal()
        {
            foreach (Portal portal in FindObjectsOfType<Portal>())
            {
                if (portal == this) continue;
                if (portal.destinationIdentifier != destinationIdentifier) continue;

                return portal;
            }

            return null;
        }

        void UpdatePortal(Portal otherPortal)
        {
            GameObject player = GameObject.FindWithTag("Player");

            player.GetComponent<NavMeshAgent>().Warp(otherPortal.spawnPoint.position);
            player.transform.rotation = otherPortal.spawnPoint.rotation;
        }
    }
}


