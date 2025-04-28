using TMPro;
using UnityEngine;


namespace ProjectColombo.UI
{
    public class OptionsMenuController : MenuController
    {
        [Header("Tab Navigation")]
        [SerializeField] GameObject[] tabScreens;
        [SerializeField] GameObject[] tabSelectionIndicators;
        [SerializeField] TextMeshProUGUI[] tabTitles;


        int currentTabIndex = 0;

        Coroutine[] textAnimationCoroutines;

        public override void Initialize()
        {
            textAnimationCoroutines = new Coroutine[tabTitles.Length];

            for (int i = 0; i < tabScreens.Length; i++)
            {
                tabScreens[i].SetActive(false);
                if (tabSelectionIndicators != null && i < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[i].SetActive(false);
                }

                if (tabTitles != null && i < tabTitles.Length)
                {
                    SetTextToDefaultSize(tabTitles[i]);
                }
            }

            SelectTab(0);
        }

        public override void Show()
        {
            base.Show();

            if (gameInputSO != null)
            {
                gameInputSO.EnableUIMode();
            }
        }

        public override void Hide()
        {
            base.Hide();
        }

        public override void HandleInput()
        {
            if (gameInputSO != null)
            {
                if (gameInputSO.playerInputActions.UI.MoveLeft.WasPressedThisFrame())
                {
                    NavigateLeft();
                }
                else if (gameInputSO.playerInputActions.UI.MoveRight.WasPressedThisFrame())
                {
                    NavigateRight();
                }
            }
        }

        void NavigateLeft()
        {
            int newIndex = currentTabIndex - 1;

            if (newIndex < 0)
            {
                newIndex = tabScreens.Length - 1;
            }

            SelectTab(newIndex);
        }

        void NavigateRight()
        {
            int newIndex = (currentTabIndex + 1) % tabScreens.Length;

            SelectTab(newIndex);
        }

        void SelectTab(int index)
        {
            if (index < 0 || index >= tabScreens.Length)
            {
                return;
            }

            if (currentTabIndex >= 0 && currentTabIndex < tabScreens.Length)
            {
                tabScreens[currentTabIndex].SetActive(false);

                if (tabSelectionIndicators != null && currentTabIndex < tabSelectionIndicators.Length)
                {
                    tabSelectionIndicators[currentTabIndex].SetActive(false);
                }

                if (tabTitles != null && currentTabIndex < tabTitles.Length)
                {
                    if (textAnimationCoroutines[currentTabIndex] != null)
                    {
                        StopCoroutine(textAnimationCoroutines[currentTabIndex]);
                    }

                    textAnimationCoroutines[currentTabIndex] = StartCoroutine(
                        AnimateTextSize(tabTitles[currentTabIndex], selectedMinFontSize, selectedMaxFontSize, defaultMinFontSize, defaultMaxFontSize, animationDuration));
                }
            }

            currentTabIndex = index;
            tabScreens[currentTabIndex].SetActive(true);

            if (tabSelectionIndicators != null && currentTabIndex < tabSelectionIndicators.Length)
            {
                tabSelectionIndicators[currentTabIndex].SetActive(true);
            }

            if (tabTitles != null && currentTabIndex < tabTitles.Length)
            {
                if (textAnimationCoroutines[currentTabIndex] != null)
                {
                    StopCoroutine(textAnimationCoroutines[currentTabIndex]);
                }

                textAnimationCoroutines[currentTabIndex] = StartCoroutine(AnimateTextSize(tabTitles[currentTabIndex], defaultMinFontSize, defaultMaxFontSize, selectedMinFontSize, selectedMaxFontSize, animationDuration));
            }
        }
    }
}