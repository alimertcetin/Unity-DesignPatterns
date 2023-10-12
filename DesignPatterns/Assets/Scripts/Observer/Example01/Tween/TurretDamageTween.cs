using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.Tween
{
    public class TurretDamageTween : MonoBehaviour
    {
        public float duration;

        float currentTime;

        float normalizedTime => currentTime / duration;
        float normalizedTimePingPong
        {
            get
            {
                var t = normalizedTime;
                return t > 0.5f ? (1f - t) / 1f : (t / 0.5f);
            }
        }

        void Update()
        {
            currentTime += Time.deltaTime;

            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, normalizedTimePingPong);

            if (currentTime > duration)
                enabled = false;
        }

        public void InitializeTween(float duration)
        {
            this.currentTime = 0f;
            this.duration = duration;
            enabled = true;
        }
    }
}