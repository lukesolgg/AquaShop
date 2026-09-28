using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AquariumShop
{
    public class CounterTablet : MonoBehaviour
    {
        public enum Page { Orders, Stock, Food }

        [Header("Root")]
        [SerializeField] GameObject root;

        [Header("Tabs")]
        [SerializeField] Button ordersTab;
        [SerializeField] Button stockTab;
        [SerializeField] Button foodTab;

        [Header("Pages")]
        [SerializeField] GameObject ordersPage;
        [SerializeField] GameObject stockPage;
        [SerializeField] GameObject foodPage;

        [Header("Orders")]
        [SerializeField] Transform ordersList;
        [SerializeField] Button orderRowTemplate;
        [SerializeField] TMP_Text ordersEmptyText;

        [Header("Stock")]
        [SerializeField] Transform stockList;
        [SerializeField] Button stockRowTemplate;
        [SerializeField] TMP_Text stockHintText;
        [SerializeField] Transform tankPickList;
        [SerializeField] Button tankPickTemplate;

        [Header("Food")]
        [SerializeField] TMP_Text foodStatusText;
        [SerializeField] Button buyFood10Button;
        [SerializeField] Button buyFood50Button;

        [Header("Shared")]
        [SerializeField] Button closeButton;
        [SerializeField] TMP_Text statusText;

        readonly List<Button> _spawned = new List<Button>();
        Page _page = Page.Orders;
        FishSpecies _pendingSpecies;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            HideTemplate(orderRowTemplate);
            HideTemplate(stockRowTemplate);
            HideTemplate(tankPickTemplate);

            Wire(ordersTab, () => ShowPage(Page.Orders));
            Wire(stockTab, () => ShowPage(Page.Stock));
            Wire(foodTab, () => ShowPage(Page.Food));
            Wire(buyFood10Button, () => BuyFood(10));
            Wire(buyFood50Button, () => BuyFood(50));
            if (closeButton != null)
                closeButton.onClick.AddListener(() => GameManager.Instance?.CloseMenu());

            Close();
        }

        static void HideTemplate(Button button)
        {
            if (button != null) button.gameObject.SetActive(false);
        }

        static void Wire(Button button, UnityEngine.Events.UnityAction action)
        {
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        public void Open()
        {
            if (root != null) root.SetActive(true);
            _pendingSpecies = null;
            ShowPage(Page.Orders);
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
            _pendingSpecies = null;
        }

        public void Rebuild()
        {
            if (!IsOpen) return;
            ShowPage(_page);
        }

        void ShowPage(Page page)
        {
            _page = page;
            if (ordersPage != null) ordersPage.SetActive(page == Page.Orders);
            if (stockPage != null) stockPage.SetActive(page == Page.Stock);
            if (foodPage != null) foodPage.SetActive(page == Page.Food);

            if (page == Page.Orders) BuildOrders();
            if (page == Page.Stock) BuildStock();
            if (page == Page.Food) BuildFood();
        }

        void ClearSpawned(Transform keepUnder)
        {
            for (int i = _spawned.Count - 1; i >= 0; i--)
            {
                var btn = _spawned[i];
                if (btn == null)
                {
                    _spawned.RemoveAt(i);
                    continue;
                }
                if (keepUnder != null && btn.transform.parent != keepUnder) continue;
                Destroy(btn.gameObject);
                _spawned.RemoveAt(i);
            }
        }

        void BuildOrders()
        {
            ClearSpawned(ordersList);
            var shop = GameManager.Instance?.shop;
            int shown = 0;
            if (shop != null && ordersList != null && orderRowTemplate != null)
            {
                int openIndex = 0;
                foreach (var order in shop.orders)
                {
                    if (order == null || !order.IsOpen) continue;
                    int index = openIndex;
                    openIndex++;
                    shown++;

                    var row = Instantiate(orderRowTemplate, ordersList);
                    row.gameObject.SetActive(true);
                    var label = row.GetComponentInChildren<TMP_Text>();
                    if (label != null && order.wanted != null)
                    {
                        label.text =
                            $"{order.qtyFilled}/{order.qty} {order.wanted.displayName}   " +
                            $"£{order.payout:0}   {order.hoursRemaining:0}h left";
                    }

                    WireChild(row.transform, "Fill", () =>
                    {
                        GameManager.Instance?.FulfillOrder(index);
                    });
                    WireChild(row.transform, "Partial", () =>
                    {
                        GameManager.Instance?.PartialFillOrder(index);
                    });
                    WireChild(row.transform, "Refuse", () =>
                    {
                        GameManager.Instance?.RefuseOrder(index);
                    });
                    _spawned.Add(row);
                }
            }

            if (ordersEmptyText != null)
                ordersEmptyText.gameObject.SetActive(shown == 0);
            SetStatus(shown == 0 ? "No open orders." : $"{shown} open order(s).");
        }

        void BuildStock()
        {
            ClearSpawned(stockList);
            ClearSpawned(tankPickList);

            var gm = GameManager.Instance;
            if (gm == null) return;

            if (stockList != null && stockRowTemplate != null && gm.catalogue != null)
            {
                foreach (var species in gm.catalogue)
                {
                    if (species == null) continue;
                    var row = Instantiate(stockRowTemplate, stockList);
                    row.gameObject.SetActive(true);
                    var label = row.GetComponentInChildren<TMP_Text>();
                    float wholesale = gm.priceList != null
                        ? gm.priceList.Wholesale(species)
                        : species.wholesalePrice;
                    if (label != null)
                        label.text = $"{species.displayName}   £{wholesale:0.00}   stock {gm.shop.CountSpecies(species)}";

                    var pick = species;
                    row.onClick.RemoveAllListeners();
                    row.onClick.AddListener(() =>
                    {
                        _pendingSpecies = pick;
                        BuildTankPicks();
                    });
                    _spawned.Add(row);
                }
            }

            BuildTankPicks();
        }

        void BuildTankPicks()
        {
            ClearSpawned(tankPickList);
            var gm = GameManager.Instance;
            if (gm?.shop == null || tankPickList == null || tankPickTemplate == null)
                return;

            if (_pendingSpecies == null)
            {
                if (stockHintText != null)
                    stockHintText.text = "Select a fish, then pick a tank.";
                SetStatus("Choose a species.");
                return;
            }

            int options = 0;
            foreach (var tank in gm.shop.tanks)
            {
                if (tank == null || !tank.Accepts(_pendingSpecies)) continue;
                options++;
                var row = Instantiate(tankPickTemplate, tankPickList);
                row.gameObject.SetActive(true);
                var label = row.GetComponentInChildren<TMP_Text>();
                string hold = tank.LockedSpecies != null ? tank.LockedSpecies.displayName : "Empty";
                if (label != null)
                    label.text = $"{tank.displayName}   {tank.AliveCount}/{tank.capacity}   {hold}";

                string tankId = tank.id;
                var species = _pendingSpecies;
                row.onClick.RemoveAllListeners();
                row.onClick.AddListener(() =>
                {
                    GameManager.Instance?.BuyFishInto(species, tankId);
                    BuildStock();
                });
                _spawned.Add(row);
            }

            if (stockHintText != null)
            {
                stockHintText.text = options == 0
                    ? $"No tank can take {_pendingSpecies.displayName} (full or wrong species)."
                    : $"Buying {_pendingSpecies.displayName}. Pick a tank.";
            }
            SetStatus(options == 0 ? "No valid tank." : $"Select tank for {_pendingSpecies.displayName}.");
        }

        void BuildFood()
        {
            var shop = GameManager.Instance?.shop;
            var prices = GameManager.Instance?.priceList;
            float unit = prices != null ? prices.foodUnitCost : (shop != null ? shop.foodUnitCost : 0.4f);
            if (foodStatusText != null)
            {
                foodStatusText.text = shop == null
                    ? "No shop."
                    : $"Food in stock: {shop.foodUnits}\n£{unit:0.00} per unit";
            }
            SetStatus("Buy food in bulk.");
        }

        void BuyFood(int units)
        {
            GameManager.Instance?.BuyFood(units);
            BuildFood();
        }

        static void WireChild(Transform parent, string childName, UnityEngine.Events.UnityAction action)
        {
            var child = parent.Find(childName);
            if (child == null) return;
            var button = child.GetComponent<Button>();
            if (button == null) return;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(action);
        }

        void SetStatus(string text)
        {
            if (statusText != null) statusText.text = text;
        }
    }
}