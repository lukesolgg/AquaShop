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

        [Header("Catalogue")]
        public FishSpecies[] catalogue;

        [Header("Orders")]
        public int maxOpenOrders = 3;
        public int minHoursBetweenOrders = 2;
        public int maxHoursBetweenOrders = 4;
        public int orderTimeoutHours = 8;

        [Header("Goals")]
        public float winMoney = 1000f;

        [Header("Run state")]
        public ShopState shop = new ShopState();

        public bool MenuOpen { get; private set; }
        public bool Won { get; private set; }
        public bool Lost { get; private set; }
        public bool GameOver => Won || Lost;

        int _hoursUntilNextOrder = 1;
        int _orderSerial;

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
    // Updated FindAnyObjectByType to clear the CS0618 deprecation warning
    if (hud == null) hud = FindAnyObjectByType<HUD>(); 
    if (GetComponent<TimeKeeper>() == null)
        gameObject.AddComponent<TimeKeeper>();

    // Always clear run-ending flags when a fresh scene boots up
    Won = false;
    Lost = false;

    if (!TryLoad())
        _hoursUntilNextOrder = 1;

    RefreshUI();
    SetMenuOpen(false);
    CheckEnd();
}

        void OnApplicationQuit()
        {
            // Only save on quit if the game isn't already over
            if (!GameOver) Save();
        }

        public void OpenInteract(Interactable.Kind kind)
        {
            if (GameOver) return;
            hud?.ShowMenu(kind == Interactable.Kind.Tank ? "Tank" : "Wholesaler");
            SetMenuOpen(true);
        }

        public void ToggleManagementMenu()
        {
            if (GameOver) return;
            if (MenuOpen) { CloseMenu(); return; }
            hud?.ShowMenu("Management");
            SetMenuOpen(true);
        }

        public void CloseMenu()
        {
            hud?.HideMenu();
            SetMenuOpen(false);
            if (!GameOver) Save();
        }

        public void BuyFish(FishSpecies species)
        {
            if (GameOver) return;
            if (shop.TryBuyFish(species, out var error))
                Debug.Log($"Bought {species.displayName}.");
            else
                Debug.Log(error);
            AfterChange();
        }

        public void BuyOneFood()
        {
            if (GameOver) return;
            if (!shop.TryBuyFood(1, out var error)) Debug.Log(error);
            AfterChange();
        }

        public void FeedAll()
        {
            if (GameOver) return;
            if (!shop.TryFeedAll(out var error)) Debug.Log(error);
            AfterChange();
        }

        public void FulfillOrder(int index)
        {
            if (GameOver) return;
            var order = shop.GetOpenOrder(index);
            if (shop.TryFulfill(index, out var error))
                Debug.Log($"Sold {order.wanted.displayName} for £{order.payout:0}.");
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
            CheckEnd();
            RefreshUI();
            if (!GameOver) Save();
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
            if (catalogue == null || catalogue.Length == 0) return;
            if (shop.OpenOrderCount >= maxOpenOrders) return;
            var species = catalogue[Random.Range(0, catalogue.Length)];
            if (species == null) return;
            _orderSerial++;
            shop.orders.Add(new Order
            {
                id = $"ord_{_orderSerial}",
                wanted = species,
                qty = 1,
                payout = species.salePrice,
                hoursRemaining = orderTimeoutHours,
                status = OrderStatus.Open
            });
        }

        public void CheckEnd()
        {
            if (Won || Lost) return;

            if (shop.money >= winMoney)
            {
                Won = true;
                EndGame(true);
                return;
            }
            if (shop.money <= 0f && shop.tank.AliveCount == 0)
            {
                Lost = true;
                EndGame(false);
            }
        }

        void EndGame(bool win)
        {
            TimeKeeper.Instance?.Pause();
            hud?.ShowEnd(win
                ? $"You win!\nFinal Balance: £{shop.money:0}"
                : "You went bust\nNo money, empty tank.");

            // Clear the save on game over so restarting starts completely fresh
            SaveSystem.Delete();
        }

        public void Restart()
        {
            // Wipe save and unpause timescale before loading
            SaveSystem.Delete();
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
                won = Won,
                lost = Lost
            };
            foreach (var f in shop.tank.fish)
            {
                if (f?.species == null) continue;
                data.fish.Add(new FishSave { speciesId = f.species.id, hunger = f.hunger });
            }
            foreach (var o in shop.orders)
            {
                if (o?.wanted == null) continue;
                data.orders.Add(new OrderSave
                {
                    id = o.id,
                    speciesId = o.wanted.id,
                    payout = o.payout,
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
            shop.tank.fish.Clear();
            shop.orders.Clear();
            _hoursUntilNextOrder = Mathf.Max(1, data.hoursUntilNextOrder);
            _orderSerial = data.orderSerial;

            if (TimeKeeper.Instance != null)
                TimeKeeper.Instance.SetHours(data.gameHours);

            foreach (var f in data.fish)
            {
                var species = FindSpecies(f.speciesId);
                if (species == null) continue;
                shop.tank.fish.Add(new FishInstance(species) { hunger = f.hunger });
            }
            foreach (var o in data.orders)
            {
                var species = FindSpecies(o.speciesId);
                if (species == null) continue;
                shop.orders.Add(new Order
                {
                    id = o.id,
                    wanted = species,
                    qty = 1,
                    payout = o.payout,
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
        }

        string BuildStatusText()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Money £{shop.money:0}  /  £{winMoney:0} to win");
            sb.AppendLine($"Food {shop.foodUnits}");
            sb.AppendLine($"Tank {shop.tank.UsedSpace}/{shop.tank.capacity} space");
            sb.AppendLine();
            if (shop.tank.AliveCount == 0) sb.AppendLine("Tank is empty.");
            else
            {
                foreach (var f in shop.tank.fish)
                {
                    if (f?.species == null) continue;
                    sb.AppendLine($"- {f.species.displayName}  hunger {(f.hunger * 100f):0}%");
                }
            }
            sb.AppendLine();
            sb.AppendLine("ORDERS");
            int shown = 0;
            foreach (var o in shop.orders)
            {
                if (o == null || !o.IsOpen || o.wanted == null) continue;
                sb.AppendLine($"- 1 {o.wanted.displayName}  £{o.payout:0}  {o.hoursRemaining:0}h");
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
            if (open) RefreshUI();
        }
    }
}