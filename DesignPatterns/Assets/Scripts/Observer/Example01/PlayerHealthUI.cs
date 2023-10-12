using System;
using UnityEngine;
using UnityEngine.UI;
using XIV.DesignPatterns.Common.HealthSystem;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class PlayerHealthUI : MonoBehaviour, IHealthListener
    {
        [SerializeField] Image img_HealthIndicator;
        
        IDamageable damageable;
        
        void OnEnable()
        {
            GameEvents.onDamageableLoaded += OnDamageableLoaded;
            damageable?.GetHealth().Register(this);
        }

        void OnDisable()
        {
            GameEvents.onDamageableLoaded -= OnDamageableLoaded;
            damageable?.GetHealth().Unregister(this);
        }

        void OnDamageableLoaded(Component obj)
        {
            damageable?.GetHealth().Unregister(this);
            damageable = obj as IDamageable;
            damageable?.GetHealth().Register(this);
        }

        void UpdateUI(HealthChange healthChange)
        {
            img_HealthIndicator.fillAmount = healthChange.normalized;
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange) => UpdateUI(healthChange);

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => UpdateUI(healthChange);
    }
}