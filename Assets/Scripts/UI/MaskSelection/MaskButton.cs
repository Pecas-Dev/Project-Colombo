using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Masks;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectColombo.UI
{
    public class MaskButton : MonoBehaviour
    {
        public GameObject maskPrefab;
        public Image imageSlot;

        private void Start()
        {
            imageSlot.sprite = maskPrefab.GetComponent<BaseMask>().maskPicture;
        }

        public void SelectMask()
        {
            GameManager.Instance.GetComponent<PlayerInventory>().EquipMask(maskPrefab);
            Animator curtain = GameObject.FindGameObjectWithTag("Transition").GetComponent<Animator>();

            if (curtain != null)
            {
                StartCoroutine(Transition(curtain));
            }

        }

        IEnumerator Transition(Animator curtain)
        {
            curtain.Play("Close");

            yield return new WaitForSecondsRealtime(2.5f);

            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextScene);
        }
    }
}