using UnityEngine;
using ProjectColombo.UI;

public static class UIManagerExtension
{
    public static void RegisterMenu(this UIManager uiManager, MenuController menuController)
    {
        var menuCacheField = typeof(UIManager).GetField("menuCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (menuCacheField != null)
        {
            var menuCache = menuCacheField.GetValue(uiManager) as System.Collections.Generic.Dictionary<string, MenuController>;

            if (menuCache != null && menuController != null)
            {
                string menuType = menuController.GetType().Name;
                menuCache[menuType] = menuController;
                menuController.Initialize();
                menuController.Hide();

                Debug.Log($"<color=#00AAFF>[UIManagerExtension] Directly registered menu: {menuType}</color>");
            }
        }
        else
        {
            Debug.LogError("[UIManagerExtension] Failed to access menuCache field via reflection!");
        }
    }
}