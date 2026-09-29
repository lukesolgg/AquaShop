using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AquariumShop
{
    public class StoreMenu : MonoBehaviour
    {
        [SerializeField] GameObject root;
        [SerializeField] TMP_Text statusText;
        [SerializeField] Button openButton;
        [SerializeField] Button closeButton;
        [SerializeField] Button skipButton;
        [SerializeField] Button tanksButton;
        [SerializeField] Button doneButton;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            if (openButton != null) openButton.onClick.AddListener(() => GameManager.Instance?.OpenShop());
            if (closeButton != null) closeButton.onClick.AddListener(() => GameManager.Instance?.CloseShop());
            if (skipButton != null) skipButton.onClick.AddListener(() => GameManager.Instance?.SkipToMorning());
            if (tanksButton != null) tanksButton.onClick.AddListener(OpenTanks);
            if (doneButton != null) doneButton.onClick.AddListener(() => GameManager.Instance?.CloseMenu());
            Close();
        }

        public void Open()
        {
            if (root != null) root.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }

        public void Refresh()
        {
            if (!IsOpen) return;
            var gm = GameManager.Instance;
            if (gm == null) return;

            var phase = gm.shop.phase;
            if (statusText != null)
            {
                string clock = TimeKeeper.Instance != null ? TimeKeeper.Instance.TimeLabel : "";
                string label = phase == ShopPhase.Open ? "OPEN" :
                    phase == ShopPhase.ClosedNight ? "CLOSED (night)" : "CLOSED (morning)";
                statusText.text = $"{clock}\nShop is {label}";
            }

            if (openButton != null) openButton.interactable = phase == ShopPhase.ClosedMorning;
            if (closeButton != null) closeButton.interactable = phase == ShopPhase.Open;
            if (skipButton != null) skipButton.interactable = phase != ShopPhase.Open;
        }

        void OpenTanks()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;
            Close();
            gm.tankPanel?.Open();
        }
    }
}