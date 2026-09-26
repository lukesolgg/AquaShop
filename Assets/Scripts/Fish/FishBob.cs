using UnityEngine;

namespace AquariumShop
{
    public class FishBob : MonoBehaviour
    {
        public float amplitude = 0.04f;
        public float speed = 1.4f;

        Vector3 _origin;
        float _phase;

        void Start()
        {
            _origin = transform.localPosition;
            _phase = Random.Range(0f, Mathf.PI * 2f);
            speed += Random.Range(-0.3f, 0.3f);
        }

        void Update()
        {
            float y = Mathf.Sin(Time.time * speed + _phase) * amplitude;
            float x = Mathf.Sin(Time.time * speed * 0.6f + _phase) * amplitude * 0.5f;
            transform.localPosition = _origin + new Vector3(x, y, 0f);
        }
    }
}
