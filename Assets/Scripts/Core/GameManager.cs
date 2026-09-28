using UnityEngine;
using UnityEngine.SceneManagement;

namespace AquariumShop
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Refs")]
        public HUD hud;
        public Behaviour playerController;
        public PriceList priceList;
        public TankPanel tankPanel;
        public CounterTablet counterTablet;
        public PauseMenu pauseMenu;
        public string titleSceneName = "Title";

        [Header("Catalogue")]
        public FishSpecies[] catalogue;

        [Header("Orders")]
        public int maxOpenOrders = 5;
        public int minHoursBetweenOrders = 2;
        public int maxHoursBetweenOrders = 4;
        public int minOrderQty = 1;
        public int maxOrderQty = 4;
        public int minOrderHours = 3;
        public int maxOrderHours = 8;

        [Header("Run state")]
        public ShopState shop = new ShopState();

        public bool MenuOpen { get; private set; }
        public bool Won { get; private set; }
        public bool Lost { get; private set; }
        public bool GameOver => Won || Lost;

        int _hoursUntilNextOrder = 1;
        int _orderSerial;

        float _speedBeforeMenu = 1f;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void Start()
        {
            if (hud == null) hud = FindAnyObjectByType<HUD>();
            if (GetComponent<TimeKeeper>() == null)
                gameObject.AddComponent<TimeKeeper>();

            Won = false;
            Lost = false;

            RegisterAnchors();
            if (!TryLoad())
                _hoursUntilNextOrder = 1;
            RegisterAnchors();

            RefreshUI();
            SetMenuOpen(false);
        }

        public void RegisterAnchors()
        {
            foreach (var anchor in FindObjectsByType<TankAnchor>(FindObjectsSortMode.None))
                shop.EnsureTank(anchor);
        }

        void OnApplicationQuit()
        {
            if (!GameOver) Save();
        }

        public void OpenInteract(Interactable.Kind kind)
{
    if (GameOver) return;
    if (kind == Interactable.Kind.Counter && counterTablet != null)
        counterTablet.Open();
    else if (kind == Interactable.Kind.Tank && tankPanel != null)
        tankPanel.Open();
    else
        hud?.ShowMenu(kind == Interactable.Kind.Tank ? "Tank" : "Wholesaler");
    SetMenuOpen(true);
}

        public void ToggleManagementMenu()
{
    if (GameOver) return;
    if (MenuOpen) { CloseMenu(); return; }
    if (tankPanel != null)
        tankPanel.Open();
    else
        hud?.ShowMenu("Management");
    SetMenuOpen(true);
}

        public void CloseMenu()
{
    hud?.HideMenu();
    tankPanel?.Close();
    counterTablet?.Close();
    pauseMenu?.Close();
    SetMenuOpen(false);
    if (!GameOver) Save();
}

        public void BuyFish(FishSpecies species)
        {
            if (GameOver) return;
            var tank = FirstAccepting(species);
            if (tank == null)
            {
                Debug.Log("No tank can take that species.");
                AfterChange();
                return;
            }
            if (shop.TryBuyFish(species, tank.id, priceList, out var error))
                Debug.Log($"Bought {species.displayName} into {tank.displayName}.");
            else
                Debug.Log(error);
            AfterChange();
        }

        public void BuyFishInto(FishSpecies species, string tankId)
        {
            if (GameOver) return;
            if (shop.TryBuyFish(species, tankId, priceList, out var error))
                Debug.Log($"Bought {species.displayName}.");
            else
                Debug.Log(error);
            AfterChange();
        }

        public void BuyFood(int units)
        {
            if (GameOver) return;
            if (!shop.TryBuyFood(units, priceList, out var error)) Debug.Log(error);
            AfterChange();
        }

        public void BuyOneFood() => BuyFood(1);

        public void FeedAll()
        {
            if (GameOver) return;
            foreach (var tank in shop.tanks)
            {
                if (tank == null || tank.IsEmpty) continue;
                if (!shop.TryFeedTank(tank.id, out var error))
                {
                    Debug.Log(error);
                    break;
                }
            }
            AfterChange();
        }

        public void FeedTank(string tankId)
        {
            if (GameOver) return;
            if (!shop.TryFeedTank(tankId, out var error)) Debug.Log(error);
            AfterChange();
        }

        public void FulfillOrder(int index)
        {
            if (GameOver) return;
            if (shop.TryFulfill(index, false, out var error))
                Debug.Log("Order filled.");
            else
                Debug.Log(error);
            AfterChange();
        }

        public void PartialFillOrder(int index)
        {
            if (GameOver) return;
            if (shop.TryFulfill(index, true, out var error))
                Debug.Log("Partial fill.");
            else
                Debug.Log(error);
            AfterChange();
        }

        public void RefuseOrder(int index)
        {
            if (GameOver) return;
            if (shop.TryRefuse(index, out var error))
                Debug.Log("Order refused.");
            else
                Debug.Log(error);
            AfterChange();
        }

        public void OnGameHourPassed()
        {
            if (GameOver) return;
            shop.TickHour();
            TickOrderSpawn();
            AfterChange();
        }

        void AfterChange()
        {
            RefreshUI();
            if (!GameOver) Save();
            tankPanel?.Rebuild();
            counterTablet?.Rebuild();
        }

        TankInstance FirstAccepting(FishSpecies species)
        {
            foreach (var t in shop.tanks)
                if (t != null && t.Accepts(species)) return t;
            return null;
        }

        void TickOrderSpawn()
        {
            _hoursUntilNextOrder--;
            if (_hoursUntilNextOrder > 0) return;
            TrySpawnOrder();
            _hoursUntilNextOrder = Random.Range(minHoursBetweenOrders, maxHoursBetweenOrders + 1);
        }

        void TrySpawnOrder()
        {
            if (shop.OpenOrderCount >= maxOpenOrders) return;
            FishSpecies species = priceList != null ? priceList.PickWeighted() : null;
            if (species == null && catalogue != null && catalogue.Length > 0)
                species = catalogue[Random.Range(0, catalogue.Length)];
            if (species == null) return;

            int qty = Random.Range(minOrderQty, maxOrderQty + 1);
            float each = priceList != null ? priceList.Retail(species) : species.salePrice;
            int hours = Random.Range(minOrderHours, maxOrderHours + 1);

            _orderSerial++;
            shop.orders.Add(new Order
            {
                id = $"ord_{_orderSerial}",
                wanted = species,
                qty = qty,
                qtyFilled = 0,
                payout = each * qty,
                penalty = each * qty * 0.3f,
                hoursRemaining = hours,
                status = OrderStatus.Open
            });
        }

        public void Restart()
        {
            SaveSystem.Delete();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void TogglePauseMenu()
{
    if (GameOver) return;
    if (MenuOpen)
    {
        CloseMenu();
        return;
    }
    pauseMenu?.Open();
    SetMenuOpen(true);
}

public void QuitToTitle()
{
    Save();
    Time.timeScale = 1f;
    SceneManager.LoadScene(titleSceneName);
}

public void QuitGame()
{
    Save();
    Application.Quit();
#if UNITY_EDITOR
    UnityEditor.EditorApplication.isPlaying = false;
#endif
}

        public void Save()
        {
            var data = new SaveData
            {
                money = shop.money,
                foodUnits = shop.foodUnits,
                gameHours = TimeKeeper.Instance != null ? TimeKeeper.Instance.GameHours : 0f,
                hoursUntilNextOrder = _hoursUntilNextOrder,
                orderSerial = _orderSerial,
                level = shop.level
            };

            foreach (var tank in shop.tanks)
            {
                if (tank == null) continue;
                var ts = new TankSave
                {
                    id = tank.id,
                    displayName = tank.displayName,
                    segment = (int)tank.segment,
                    capacity = tank.capacity
                };
                foreach (var f in tank.fish)
                {
                    if (f?.species == null) continue;
                    ts.fish.Add(new FishSave { speciesId = f.species.id, hunger = f.hunger });
                }
                data.tanks.Add(ts);
            }

            foreach (var o in shop.orders)
            {
                if (o?.wanted == null) continue;
                data.orders.Add(new OrderSave
                {
                    id = o.id,
                    speciesId = o.wanted.id,
                    qty = o.qty,
                    qtyFilled = o.qtyFilled,
                    payout = o.payout,
                    penalty = o.penalty,
                    hoursRemaining = o.hoursRemaining,
                    status = (int)o.status
                });
            }
            SaveSystem.Write(data);
        }

        bool TryLoad()
        {
            var data = SaveSystem.Read();
            if (data == null) return false;

            shop.money = data.money;
            shop.foodUnits = data.foodUnits;
            shop.level = data.level <= 0 ? 1 : data.level;
            shop.orders.Clear();
            _hoursUntilNextOrder = Mathf.Max(1, data.hoursUntilNextOrder);
            _orderSerial = data.orderSerial;

            if (TimeKeeper.Instance != null)
                TimeKeeper.Instance.SetHours(data.gameHours);

            if (data.tanks != null)
            {
                foreach (var ts in data.tanks)
                {
                    if (ts == null || string.IsNullOrEmpty(ts.id)) continue;
                    var tank = shop.GetTank(ts.id);
                    if (tank == null)
                    {
                        tank = new TankInstance
                        {
                            id = ts.id,
                            displayName = ts.displayName,
                            segment = (TankSegment)ts.segment,
                            capacity = ts.capacity > 0 ? ts.capacity : 6
                        };
                        shop.tanks.Add(tank);
                    }
                    tank.fish.Clear();
                    if (ts.fish == null) continue;
                    foreach (var f in ts.fish)
                    {
                        var species = FindSpecies(f.speciesId);
                        if (species == null) continue;
                        tank.fish.Add(new FishInstance(species) { hunger = f.hunger });
                    }
                }
            }

            foreach (var o in data.orders)
            {
                var species = FindSpecies(o.speciesId);
                if (species == null) continue;
                shop.orders.Add(new Order
                {
                    id = o.id,
                    wanted = species,
                    qty = o.qty <= 0 ? 1 : o.qty,
                    qtyFilled = o.qtyFilled,
                    payout = o.payout,
                    penalty = o.penalty,
                    hoursRemaining = o.hoursRemaining,
                    status = (OrderStatus)o.status
                });
            }
            Debug.Log("Save loaded.");
            return true;
        }

        FishSpecies FindSpecies(string id)
        {
            if (catalogue == null || string.IsNullOrEmpty(id)) return null;
            foreach (var s in catalogue)
                if (s != null && s.id == id) return s;
            return null;
        }

        public void RefreshUI()
        {
            hud?.RefreshMoney(shop.money);
            hud?.RefreshStatus(BuildStatusText());
            hud?.RefreshTime(
                TimeKeeper.Instance != null ? TimeKeeper.Instance.TimeLabel : "Hour 0",
                TimeKeeper.Instance != null ? TimeKeeper.Instance.SpeedLabel : "1x");
            hud?.RefreshOrders(shop);
            hud?.RefreshHudMeta(shop.OpenOrderCount, maxOpenOrders, shop.AnyStarving);
        }

        string BuildStatusText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Money £{shop.money:0}");
            sb.AppendLine($"Food {shop.foodUnits}");
            sb.AppendLine($"Tanks {shop.tanks.Count}   Fish {shop.AliveCount}");
            sb.AppendLine($"Orders {shop.OpenOrderCount}/{maxOpenOrders}");
            if (shop.AnyStarving) sb.AppendLine("A tank is starving.");
            sb.AppendLine();
            sb.AppendLine("ORDERS");
            int shown = 0;
            foreach (var o in shop.orders)
            {
                if (o == null || !o.IsOpen || o.wanted == null) continue;
                sb.AppendLine($"- {o.qtyFilled}/{o.qty} {o.wanted.displayName}  £{o.payout:0}  {o.hoursRemaining:0}h");
                shown++;
            }
            if (shown == 0) sb.AppendLine("- none yet");
            return sb.ToString();
        }

        void SetMenuOpen(bool open)
{
    MenuOpen = open;
    if (playerController != null)
        playerController.enabled = !open && !GameOver;
    Cursor.lockState = open || GameOver ? CursorLockMode.None : CursorLockMode.Locked;
    Cursor.visible = open || GameOver;

    if (TimeKeeper.Instance != null)
    {
        if (open)
        {
            _speedBeforeMenu = TimeKeeper.Instance.Speed > 0.001f ? TimeKeeper.Instance.Speed : 1f;
            TimeKeeper.Instance.Pause();
        }
        else
        {
            if (_speedBeforeMenu >= 2.5f) TimeKeeper.Instance.Play3x();
            else if (_speedBeforeMenu >= 1.5f) TimeKeeper.Instance.Play2x();
            else TimeKeeper.Instance.Play1x();
        }
    }

    if (open) RefreshUI();
}
    }
}