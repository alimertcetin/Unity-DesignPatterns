using UnityEngine;
using XIV.DesignPatterns.Common.HealthSystem;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class ObserverExample01Player : MonoBehaviour, IDamageable
    {
        [SerializeField] Health health;
        [SerializeField] Gun gun;

        void Start()
        {
            health.Initialize();
            // Inform other systems
            GameEvents.InvokeOnDamageableLoaded(this);
        }

        void Update()
        {
            if (Input.GetMouseButton(0) == false) return;

            gun.Fire();
        }

        bool IDamageable.CanReceiveDamage()
        {
            return health.isDepleted == false;
        }

        void IDamageable.ReceiveDamage(float amount)
        {
            health.DecreaseCurrentHealth(amount);
        }

        Health IDamageable.GetHealth()
        {
            return health;
        }
    }
}