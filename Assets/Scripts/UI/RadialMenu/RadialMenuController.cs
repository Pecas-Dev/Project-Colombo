using UnityEngine;
using ProjectColombo.UI;

namespace ProjectColombo.UI
{
    public class RadialMenuController : MenuController
    {
        [Header("Radial Menu References")]
        public RadialMenuShader radialMenuShader;

        public override void Initialize()
        {
            base.Initialize();
            
            menuContainer.SetActive(false);
            
            gameObject.SetActive(false);
        }

        public override void Show()
        {
            base.Show();
            gameObject.SetActive(true);
        }

        public override void Hide()
        {
            base.Hide();
            gameObject.SetActive(false);
        }

        public override void HandleInput()
        {
            base.HandleInput();
        }
    }
}
