using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.Tween
{
    public class TurretDestroyTween : MonoBehaviour
    {
        public float duration;
        float currentTime;

        float normalizedTime => currentTime / duration;

        void Update()
        {
            currentTime += Time.deltaTime;
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, normalizedTime * normalizedTime);

            if (normalizedTime < Mathf.Epsilon)
                Destroy(this);
        }
    }
}