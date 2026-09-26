using UnityEngine;

namespace AquariumShop
{
    public class TimeKeeper : MonoBehaviour
    {
        public static TimeKeeper Instance { get; private set; }

        [Tooltip("At 1x, 1 real second = this many game hours.")]
        public float gameHoursPerRealSecond = 0.1f;

        public float GameHours { get; private set; }
        public float Speed { get; private set; } = 1f;

        float _hourAcc;

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
            if (GameManager.Instance != null && GameManager.Instance.GameOver)
                return;

            _hourAcc += Time.deltaTime * gameHoursPerRealSecond;
            while (_hourAcc >= 1f)
            {
                _hourAcc -= 1f;
                GameHours += 1f;
                GameManager.Instance?.OnGameHourPassed();
            }
        }

        public void SetHours(float hours)
        {
            GameHours = hours;
            _hourAcc = 0f;
        }

        public void Pause() => ApplySpeed(0f);
        public void Play1x() => ApplySpeed(1f);
        public void Play2x() => ApplySpeed(2f);
        public void Play3x() => ApplySpeed(3f);

        void ApplySpeed(float speed)
        {
            if (GameManager.Instance != null && GameManager.Instance.GameOver)
                speed = 0f;
            Speed = speed;
            Time.timeScale = speed;
            GameManager.Instance?.RefreshUI();
        }

        public string SpeedLabel => Speed <= 0.001f ? "Paused" : $"{Speed:0}x";
        public string TimeLabel => $"Hour {GameHours:0}";
    }
}