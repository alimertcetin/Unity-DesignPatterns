using Cinemachine;
using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.PlayerDamageEffects.CameraEffects
{
    public class CameraFOVTween : CameraEffectTween
    {
        [SerializeField] CinemachineVirtualCamera cam;
        [SerializeField] float targetFOV;
        float initialFOV;

        void Awake()
        {
            this.enabled = false;
            initialFOV = cam.m_Lens.FieldOfView;
        }

        void Update()
        {
            if (timer.Update(Time.deltaTime))
            {
                this.enabled = false;
            }
            cam.m_Lens.FieldOfView = Mathf.Lerp(initialFOV, targetFOV, timer.normalizedTimePingPong);
        }

        public override void StartTween()
        {
            if (timer.IsDone() == false)
            {
                cam.m_Lens.FieldOfView = initialFOV;
            }
            base.timer.Reset();
            this.enabled = true;
        }

#if UNITY_EDITOR

        void OnValidate()
        {
            if (cam) return;
            cam = GetComponent<CinemachineVirtualCamera>();
        }

#endif
    }
}