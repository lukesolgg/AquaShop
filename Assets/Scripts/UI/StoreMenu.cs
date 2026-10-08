using System.Collections.Generic;
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

        [Header("Upgrades")]
        [SerializeField] Transform upgradesList;
        [SerializeField] Button upgradeRowTemplate;

        readonly List<Button> _upgradeRows = new List<Button>();

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            if (upgradeRowTemplate != null)
                upgradeRowTemplate.gameObject.SetActive(false);

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

            BuildUpgradeRows();
        }

        void BuildUpgradeRows()
        {
            foreach (var row in _upgradeRows)
                if (row != null) Destroy(row.gameObject);
            _upgradeRows.Clear();

            var gm = GameManager.Instance;
            if (gm == null || upgradesList == null || upgradeRowTemplate == null) return;
            if (gm.upgrades == null) return;

            foreach (var up in gm.upgrades)
            {
                if (up == null) continue;

                var row = Instantiate(upgradeRowTemplate, upgradesList);
                row.gameObject.SetActive(true);

                bool owned = gm.shop.HasUpgrade(up.id);
                bool reqOk = string.IsNullOrEmpty(up.requiresUpgradeId) || gm.shop.HasUpgrade(up.requiresUpgradeId);
                bool canBuy = !owned && reqOk && gm.shop.money >= up.cost;

                var label = row.GetComponentInChildren<TMP_Text>();
                if (label != null)
                {
                    if (owned) label.text = $"{up.displayName}  OWNED";
                    else if (!reqOk) label.text = $"{up.displayName}  £{up.cost:0}  (locked)";
                    else label.text = $"{up.displayName}  £{up.cost:0}";
                }

                string id = up.id;
                row.onClick.RemoveAllListeners();
                row.interactable = canBuy;
                if (canBuy)
                    row.onClick.AddListener(() => GameManager.Instance?.BuyUpgrade(id));

                _upgradeRows.Add(row);
            }
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