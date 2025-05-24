
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
            GameManager.Instance.gameInput.EnableInput(GameInputSystem.PlayerInputAction.Interact);
        }

        private void Update()
        {

            if (GameManager.Instance.gameInput.GetInputPressed(GameInputSystem.PlayerInputAction.Interact))
            {
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