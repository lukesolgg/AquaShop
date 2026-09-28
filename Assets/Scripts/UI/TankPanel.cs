using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AquariumShop
{
    public class TankPanel : MonoBehaviour
    {
        [Header("Root")]
        [SerializeField] GameObject root;

        [Header("Segments")]
        [SerializeField] Button allButton;
        [SerializeField] Button islandButton;
        [SerializeField] Button backButton;
        [SerializeField] Button leftButton;
        [SerializeField] Button rightButton;

        [Header("List")]
        [SerializeField] Transform listParent;
        [SerializeField] Button rowTemplate;

        [Header("Detail")]
        [SerializeField] TMP_Text titleText;
        [SerializeField] TMP_Text detailText;
        [SerializeField] Button feedButton;
        [SerializeField] Button closeButton;

        readonly List<Button> _rows = new List<Button>();
        TankSegment? _filter;
        string _selectedId;

        public bool IsOpen => root != null && root.activeSelf;

        void Awake()
        {
            if (rowTemplate != null)
                rowTemplate.gameObject.SetActive(false);

            Wire(allButton, () => SetFilter(null));
            Wire(islandButton, () => SetFilter(TankSegment.Island));
            Wire(backButton, () => SetFilter(TankSegment.Back));
            Wire(leftButton, () => SetFilter(TankSegment.Left));
            Wire(rightButton, () => SetFilter(TankSegment.Right));

            if (feedButton != null)
                feedButton.onClick.AddListener(FeedSelected);
            if (closeButton != null)
                closeButton.onClick.AddListener(() => GameManager.Instance?.CloseMenu());

            Close();
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
            if (string.IsNullOrEmpty(_selectedId))
            {
                var primary = GameManager.Instance?.shop?.PrimaryTank();
                _selectedId = primary != null ? primary.id : null;
            }
            Rebuild();
        }

        public void Close()
        {
            if (root != null) root.SetActive(false);
        }

        public void Rebuild()
        {
            if (!IsOpen) return;
            BuildRows();
            RefreshDetail();
        }

        void SetFilter(TankSegment? segment)
        {
            _filter = segment;
            _selectedId = null;
            Rebuild();
        }

        void BuildRows()
        {
            foreach (var row in _rows)
                if (row != null) Destroy(row.gameObject);
            _rows.Clear();

            var shop = GameManager.Instance?.shop;
            if (shop == null || listParent == null || rowTemplate == null) return;

            foreach (var tank in shop.tanks)
            {
                if (tank == null) continue;
                if (_filter.HasValue && tank.segment != _filter.Value) continue;

                var row = Instantiate(rowTemplate, listParent);
                row.gameObject.SetActive(true);
                row.name = tank.id;

                var label = row.GetComponentInChildren<TMP_Text>();
                if (label != null) label.text = RowLabel(tank);

                string id = tank.id;
                row.onClick.RemoveAllListeners();
                row.onClick.AddListener(() =>
                {
                    _selectedId = id;
                    RefreshDetail();
                });
                _rows.Add(row);

                if (string.IsNullOrEmpty(_selectedId))
                    _selectedId = tank.id;
            }
        }

        static string RowLabel(TankInstance tank)
        {
            string species = tank.LockedSpecies != null ? tank.LockedSpecies.displayName : "Empty";
            string hunger = tank.IsEmpty ? "" : $"  {tank.AverageHunger * 100f:0}%";
            return $"{tank.displayName}  {tank.AliveCount}/{tank.capacity}  {species}{hunger}";
        }

        void RefreshDetail()
        {
            var shop = GameManager.Instance?.shop;
            var tank = shop != null ? shop.GetTank(_selectedId) : null;

            if (titleText != null)
                titleText.text = tank != null ? tank.displayName : "No tank selected";

            if (detailText != null)
                detailText.text = tank != null ? Detail(tank) : "Tag tanks with TankAnchor to see them here.";

            if (feedButton != null)
                feedButton.interactable = tank != null && !tank.IsEmpty;
        }

        static string Detail(TankInstance tank)
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"Segment: {tank.segment}");
            sb.AppendLine($"Space: {tank.UsedSpace}/{tank.capacity}");
            if (tank.IsEmpty)
            {
                sb.AppendLine("Empty. Buy stock from the counter.");
                return sb.ToString();
            }
            sb.AppendLine($"Species: {tank.LockedSpecies.displayName}");
            sb.AppendLine($"Hunger: {tank.AverageHunger * 100f:0}%");
            sb.AppendLine();
            foreach (var f in tank.fish)
            {
                if (f?.species == null || !f.IsAlive) continue;
                sb.AppendLine($"- {f.species.displayName}  {f.hunger * 100f:0}%");
            }
            return sb.ToString();
        }

        void FeedSelected()
        {
            if (string.IsNullOrEmpty(_selectedId)) return;
            GameManager.Instance?.FeedTank(_selectedId);
            Rebuild();
        }
    }
}