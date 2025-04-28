using UnityEngine;
using ProjectColombo.UI;
using ProjectColombo.GameInputSystem;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameInputSO gameInputSO;

    [Header("Menu Controllers")]
    [SerializeField] MainMenuController mainMenuController;

    [SerializeField] OptionsMenuController optionsMenuController;

    MenuController currentActiveMenu;

    void Start()
    {
        if (mainMenuController != null)
        {
            mainMenuController.Initialize();
        }

        if (optionsMenuController != null)
        {
            optionsMenuController.Initialize();
        }

        if (mainMenuController != null)
        {
            mainMenuController.Hide();
        }

        if (optionsMenuController != null)
        {
            optionsMenuController.Hide();
        }

        ShowMainMenu();
    }

    void Update()
    {
        if (currentActiveMenu != null)
        {
            currentActiveMenu.HandleInput();
        }

        if (currentActiveMenu != mainMenuController && gameInputSO != null && gameInputSO.playerInputActions != null)        
        {
            if (gameInputSO.playerInputActions.UI.Cancel.WasPressedThisFrame())
            {
                ShowMainMenu();
            }
        }
    }

    public void ShowMenu(MenuController menuToShow)
    {
        if (menuToShow == null)
        {
            return;
        }

        if (currentActiveMenu != null)
        {
            currentActiveMenu.Hide();
        }

        menuToShow.Show();

        currentActiveMenu = menuToShow;
    }

    public void ShowMainMenu()
    {
        ShowMenu(mainMenuController);
    }

    public void ShowOptionsMenu()
    {
        ShowMenu(optionsMenuController);
    }
}