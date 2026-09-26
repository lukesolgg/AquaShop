using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AquariumShop
{
    public class HUD : MonoBehaviour
    {
        [Header("Main HUD")]
        [SerializeField] TMP_Text moneyText;
        [SerializeField] TMP_Text promptText;
        [SerializeField] TMP_Text timeText;
        [SerializeField] TMP_Text speedText;

        [Header("Management Panel & Tabs")]
        [SerializeField] GameObject managementPanel;
        [SerializeField] GameObject tankTab;
        [SerializeField] GameObject businessTab;
        [SerializeField] Button tankTabButton;
        [SerializeField] Button businessTabButton;

        [Header("Controls & Details")]
        [SerializeField] TMP_Text bodyText;
        [SerializeField] Button closeButton;
        [SerializeField] Button buyFoodButton;
        [SerializeField] Button feedButton;
        [SerializeField] Button pauseButton;
        [SerializeField] Button speed1Button;
        [SerializeField] Button speed2Button;
        [SerializeField] Button speed3Button;

        [Header("Shop & Order Buttons")]
        [SerializeField] Button[] buyFishButtons;
        [SerializeField] Button[] fulfillButtons;

        [Header("Game Over Panel")]
        [SerializeField] GameObject endPanel;
        [SerializeField] TMP_Text endText;
        [SerializeField] Button restartButton;

        void Awake()
        {
            // Tab button wiring
            if (tankTabButton != null)
                tankTabButton.onClick.AddListener(() => SwitchTab(true));
            if (businessTabButton != null)
                businessTabButton.onClick.AddListener(() => SwitchTab(false));

            // Core UI wiring
            if (closeButton != null)
                closeButton.onClick.AddListener(() => GameManager.Instance?.CloseMenu());
            if (buyFoodButton != null)
                buyFoodButton.onClick.AddListener(() => GameManager.Instance?.BuyOneFood());
            if (feedButton != null)
                feedButton.onClick.AddListener(() => GameManager.Instance?.FeedAll());
            if (pauseButton != null)
                pauseButton.onClick.AddListener(() => TimeKeeper.Instance?.Pause());
            if (speed1Button != null)
                speed1Button.onClick.AddListener(() => TimeKeeper.Instance?.Play1x());
            if (speed2Button != null)
                speed2Button.onClick.AddListener(() => TimeKeeper.Instance?.Play2x());
            if (speed3Button != null)
                speed3Button.onClick.AddListener(() => TimeKeeper.Instance?.Play3x());
            if (restartButton != null)
                restartButton.onClick.AddListener(() => GameManager.Instance?.Restart());

            HideMenu();
            if (endPanel != null) endPanel.SetActive(false);
            SetPrompt(string.Empty);
        }

        void Start()
        {
            WireBuyButtons();
            WireFulfillButtons();
        }

        public void SwitchTab(bool showTank)
{
    if (tankTab != null) tankTab.SetActive(showTank);
    if (businessTab != null) businessTab.SetActive(!showTank);

    var gm = GameManager.Instance;
    if (gm != null)
    {
        gm.RefreshUI();
        WireBuyButtons(); 
        if (gm.shop != null) RefreshOrders(gm.shop);
    }
}

        void WireBuyButtons()
        {
            var gm = GameManager.Instance;
            if (gm == null || buyFishButtons == null) return;
            for (int i = 0; i < buyFishButtons.Length; i++)
            {
                var btn = buyFishButtons[i];
                if (btn == null) continue;
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    if (gm.catalogue != null && index < gm.catalogue.Length)
                        gm.BuyFish(gm.catalogue[index]);
                });
                if (gm.catalogue != null && index < gm.catalogue.Length && gm.catalogue[index] != null)
                {
                    var label = btn.GetComponentInChildren<TMP_Text>();
                    var s = gm.catalogue[index];
                    if (label != null) label.text = $"Buy {s.displayName}  £{s.wholesalePrice:0}";
                }
            }
        }

        void WireFulfillButtons()
        {
            if (fulfillButtons == null) return;
            for (int i = 0; i < fulfillButtons.Length; i++)
            {
                var btn = fulfillButtons[i];
                if (btn == null) continue;
                int index = i;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => GameManager.Instance?.FulfillOrder(index));
            }
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm != null && gm.MenuOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                gm.CloseMenu();
        }

        public void RefreshMoney(float amount)
        {
            if (moneyText != null) moneyText.text = $"£{amount:0}";
        }

        public void RefreshStatus(string text)
        {
            if (bodyText != null) bodyText.text = text;
        }

        public void RefreshTime(string timeLabel, string speedLabel)
        {
            if (timeText != null) timeText.text = timeLabel;
            if (speedText != null) speedText.text = speedLabel;
        }

        public void RefreshOrders(ShopState shop)
        {
            if (fulfillButtons == null) return;
            for (int i = 0; i < fulfillButtons.Length; i++)
            {
                var btn = fulfillButtons[i];
                if (btn == null) continue;
                var order = shop != null ? shop.GetOpenOrder(i) : null;
                btn.gameObject.SetActive(order != null);
                if (order == null) continue;
                var label = btn.GetComponentInChildren<TMP_Text>();
                if (label != null && order.wanted != null)
                    label.text = $"Sell {order.wanted.displayName}  £{order.payout:0}  {order.hoursRemaining:0}h";
            }
        }

        public void SetPrompt(string text)
        {
            if (promptText != null) promptText.text = text;
        }

        public void ShowMenu(string defaultView = "Tank")
{
    if (managementPanel != null) managementPanel.SetActive(true);
        bool isTank = defaultView.Equals("Tank", System.StringComparison.OrdinalIgnoreCase);
    SwitchTab(isTank);
}

        public void HideMenu()
        {
            if (managementPanel != null) managementPanel.SetActive(false);
        }

        public void ShowEnd(string message)
        {
            HideMenu();
            if (endPanel != null) endPanel.SetActive(true);
            if (endText != null) endText.text = message;
        }
    }
}