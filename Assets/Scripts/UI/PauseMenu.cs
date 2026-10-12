using UnityEngine;
using UnityEngine.UI;

namespace AquariumShop
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Button resumeButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button stuckButton;
        [SerializeField] Button feedbackButton;
        [SerializeField] Button quitToMenuButton;

        [Header("Optional settings sheet")]
        [SerializeField] GameObject settingsPanel;
        [SerializeField] Button settingsBackButton;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => GameManager.Instance?.CloseMenu());

            if (settingsButton != null)
                settingsButton.onClick.AddListener(OpenSettings);

            if (stuckButton != null)
                stuckButton.onClick.AddListener(() =>
                {
                    Debug.Log("I'm stuck — wiki placeholder");
                    GameManager.Instance?.hud?.ShowToast("Help wiki coming soon");
                });

            if (feedbackButton != null)
                feedbackButton.onClick.AddListener(() =>
                {
                    Debug.Log("Feedback placeholder");
                    GameManager.Instance?.hud?.ShowToast("Feedback form coming soon");
                });

            if (quitToMenuButton != null)
                quitToMenuButton.onClick.AddListener(() => GameManager.Instance?.QuitToTitle());

            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(CloseSettings);

            CloseSettings();
            Close();
        }

        public void Open()
        {
            if (root != null) root.SetActive(true);
            CloseSettings();
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
            CloseSettings();
        }

        void OpenSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(true);
            else
                GameManager.Instance?.hud?.ShowToast("Settings coming soon");
        }

        void CloseSettings()
        {
            if (settingsPanel != null)
                settingsPanel.SetActive(false);
        }
    }
}