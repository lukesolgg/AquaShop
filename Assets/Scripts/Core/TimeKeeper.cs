using UnityEngine;

namespace AquariumShop
{
    public class TimeKeeper : MonoBehaviour
    {
        public static TimeKeeper Instance { get; private set; }

        [Tooltip("Real seconds for one 15-minute game step at 1x. TCG-style = 15.")]
        public float secondsPerQuarter = 15f;

        public float GameHours { get; private set; }
        public float Speed { get; private set; } = 1f;
        public bool ClockFrozen { get; private set; }

        public int DayNumber => Mathf.FloorToInt(GameHours / 24f) + 1;
        public int HourOfDay => Mathf.FloorToInt(GameHours) % 24;
        public int MinuteOfDay => Mathf.FloorToInt((GameHours % 1f) * 60f);

        public string SpeedLabel => Speed <= 0.001f ? "Paused" : $"{Speed:0}x";
        public string TimeLabel => $"Day {DayNumber}  {HourOfDay:00}:{MinuteOfDay:00}";

        float _acc;

        void Awake()
        {
            Instance = this;
            ApplySpeed(1f);
        }

        void OnDestroy()
        {
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.GameOver) return;
            if (ClockFrozen) return;
            if (GameManager.Instance != null && GameManager.Instance.MenuOpen) return;
            if (Speed <= 0.001f) return;

            _acc += Time.unscaledDeltaTime * Speed;
            while (_acc >= secondsPerQuarter)
            {
                _acc -= secondsPerQuarter;
                GameHours += 0.25f;
                GameManager.Instance?.OnGameHourPassed(0.25f);
            }
        }

        public void SetHours(float hours)
        {
            GameHours = hours;
            _acc = 0f;
        }

        public void SetFrozen(bool frozen) => ClockFrozen = frozen;

        public void Pause() => ApplySpeed(0f);
        public void Play1x() => ApplySpeed(1f);
        public void Play2x() => ApplySpeed(2f);
        public void Play3x() => ApplySpeed(3f);

        void ApplySpeed(float speed)
        {
            if (GameManager.Instance != null && GameManager.Instance.GameOver)
                speed = 0f;
            Speed = speed;
            Time.timeScale = speed <= 0.001f ? 0f : 1f;
            GameManager.Instance?.RefreshUI();
        }
    }
}