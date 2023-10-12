using System;
using UnityEngine;
using XIV.DesignPatterns.Common.Extensions;
using XIV.DesignPatterns.Common.HealthSystem;
using Random = UnityEngine.Random;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class SoundPlayer : MonoBehaviour, IHealthListener
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] hurtAudioClips;
        [SerializeField] AudioClip[] deadAudioClips;

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

        void Play(AudioClip clip)
        {
            var value = Random.value;
            audioSource.pitch = value < 0.5f ? value + 0.5f : value;
            audioSource.PlayOneShot(clip);
        }

        void IHealthListener.OnHealthChange(HealthChange healthChange) => Play(hurtAudioClips.PickRandom());

        void IHealthListener.OnHealthDepleted(HealthChange healthChange) => Play(deadAudioClips.PickRandom());
    }
}