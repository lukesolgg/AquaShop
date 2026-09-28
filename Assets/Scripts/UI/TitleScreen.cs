using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AquariumShop
{
    public class TitleScreen : MonoBehaviour
    {
        [SerializeField] string shopSceneName = "Shop_Greybox";
        [SerializeField] Button newGameButton;
        [SerializeField] Button continueButton;
        [SerializeField] Button quitButton;

        void Awake()
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (newGameButton != null)
                newGameButton.onClick.AddListener(NewGame);
            if (continueButton != null)
            {
                continueButton.interactable = SaveSystem.Exists;
                continueButton.onClick.AddListener(Continue);
            }
            if (quitButton != null)
                quitButton.onClick.AddListener(() =>
                {
                    Application.Quit();
#if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
#endif
                });
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
    }
}