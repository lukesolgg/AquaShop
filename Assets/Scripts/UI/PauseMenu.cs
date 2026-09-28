using UnityEngine;
using UnityEngine.UI;

namespace AquariumShop
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] Button resumeButton;
        [SerializeField] Button saveButton;
        [SerializeField] Button titleButton;
        [SerializeField] Button quitButton;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(() => GameManager.Instance?.CloseMenu());
            if (saveButton != null)
                saveButton.onClick.AddListener(Save);
            if (titleButton != null)
                titleButton.onClick.AddListener(() => GameManager.Instance?.QuitToTitle());
            if (quitButton != null)
                quitButton.onClick.AddListener(() => GameManager.Instance?.QuitGame());
            Close();
        }

        public void Open()
        {
            if (root != null) root.SetActive(true);
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }

        void Save()
        {
            GameManager.Instance?.Save();
            Debug.Log("Saved.");
        }
    }
}