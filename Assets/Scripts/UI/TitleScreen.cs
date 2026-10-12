using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AquariumShop
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] string shopSceneName = "Shop_Greybox";
        [SerializeField] string playerName = "Player";

        [Header("Main")]
        [SerializeField] Button continueButton;
        [SerializeField] Button newGameButton;
        [SerializeField] Button settingsButton;
        [SerializeField] Button creditsButton;
        [SerializeField] Button quitButton;
        [SerializeField] TMP_Text playerNameText;

        [Header("Social (placeholders)")]
        [SerializeField] Button discordButton;
        [SerializeField] Button xButton;
        [SerializeField] Button redditButton;

        [Header("Panels")]
        [SerializeField] GameObject settingsPanel;
        [SerializeField] Button settingsBackButton;
        [SerializeField] GameObject creditsPanel;
        [SerializeField] Button creditsBackButton;

        void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerNameText != null)
                playerNameText.text = playerName;

            if (continueButton != null)
            {
                continueButton.interactable = SaveSystem.Exists;
                continueButton.onClick.AddListener(Continue);
            }
            if (newGameButton != null)
                newGameButton.onClick.AddListener(NewGame);
            if (settingsButton != null)
                settingsButton.onClick.AddListener(() => ShowPanel(settingsPanel));
            if (creditsButton != null)
                creditsButton.onClick.AddListener(() => ShowPanel(creditsPanel));
            if (quitButton != null)
                quitButton.onClick.AddListener(Quit);

            if (settingsBackButton != null)
                settingsBackButton.onClick.AddListener(() => HidePanel(settingsPanel));
            if (creditsBackButton != null)
                creditsBackButton.onClick.AddListener(() => HidePanel(creditsPanel));

            // Placeholders — no URLs yet
            WireStub(discordButton);
            WireStub(xButton);
            WireStub(redditButton);

            HidePanel(settingsPanel);
            HidePanel(creditsPanel);
        }

        static void WireStub(Button button)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => Debug.Log("Social link placeholder"));
        }

        void ShowPanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(true);
        }

        void HidePanel(GameObject panel)
        {
            if (panel != null) panel.SetActive(false);
        }

        void NewGame()
        {
            SaveSystem.Delete();
            SceneManager.LoadScene(shopSceneName);
        }

        void Continue()
        {
            SceneManager.LoadScene(shopSceneName);
        }

        void Quit()
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }
}