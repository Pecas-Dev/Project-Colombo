using ProjectColombo.GameManagement;
using ProjectColombo.Inventory;
using ProjectColombo.Objects.Masks;
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
            int nextScene = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadScene(nextScene);
        }
    }
}