using UnityEngine;

namespace XIV.DesignPatterns.Observer.Example01.ScriptableObjects
{
    [CreateAssetMenu(menuName = "SO/" + nameof(TurretConfigSO))]
    public class TurretConfigSO : ScriptableObject
    {
        public float rotationSpeed = 90f;
        public LayerMask targetLayerMask;
        public LayerMask obstacleLayerMask;
        public float FOVAngle = 90f;
        public float FOVDistance = 5f;
        public float fireCooldown = 0.5f;
        [Tooltip("How fast projectile will move")]
        public float projectileSpeed = 40f;
        public float damage = 2.5f;
        public GameObject destroyedParticlePrefab;

        [Header("Health Data")]
        public float maxHealth = 100f;
        public float currentHealth = 100f;
    }
}