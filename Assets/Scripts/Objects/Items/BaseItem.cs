
using ProjectColombo.GameManagement;
using UnityEngine;

namespace ProjectColombo.Objects.Items
{
    public enum RARITY { COMMON = 0, RARE, LEGENDARY }
    public abstract class BaseItem : MonoBehaviour
    {
        public string itemName;
        public RARITY itemRarity;
        public Sprite itemPicture;
        [TextArea] public string itemDescription;
        [TextArea] public string itemLore;

        private void Start()
        {
            GameManager.Instance.gameInput.EnableInput(GameInputSystem.InputActionType.Interact);
        }

        private void Update()
        {

            if (GameManager.Instance.gameInput.InteractPressed)
            {
                GameManager.Instance.gameInput.ResetUseItemPressed();

                GameObject thisIsAHack = GameObject.Find("PickUpCanvas");

                if (thisIsAHack == null)
                {
                    Activate();
                }
            }
        }


        public abstract void Activate();
    }
}