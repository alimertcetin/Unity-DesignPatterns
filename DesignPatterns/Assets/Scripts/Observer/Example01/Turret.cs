using System.Buffers;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using XIV.DesignPatterns.Common.FOV;
using XIV.DesignPatterns.Common.HealthSystem;
using XIV.DesignPatterns.Observer.Example01.ScriptableObjects;
using XIV.DesignPatterns.Observer.Example01.Tween;

namespace XIV.DesignPatterns.Observer.Example01
{
    public class Turret : MonoBehaviour, IDamageable
    {
        [SerializeField] Transform topPivot;
        [SerializeField] Transform firePos;
        [SerializeField] TurretConfigSO config;

        FOVTargetSelector fovTargetSelector;
        Health health;

        List<ProjectileFireData> acitveProjectiles = new List<ProjectileFireData>();
        ObjectPool<GameObject> projectilePool;
        float currentCooldown;

        void Awake()
        {
            projectilePool = new ObjectPool<GameObject>(CreateProjectile, OnGetProjectile, OnReleaseProjectile);
            health = new Health(config.maxHealth, config.currentHealth);
            fovTargetSelector = new FOVTargetSelector(config.obstacleLayerMask, config.targetLayerMask);
        }

        void Update()
        {
            fovTargetSelector.UpdateTarget(GetFieldOfViewData());
            RotateTowardsTarget();
            CheckActiveProjectiles();
            MoveActiveProjectiles();
            Fire();
        }
        
        void RotateTowardsTarget()
        {
            if (fovTargetSelector.currentTarget == null)
            {
                topPivot.rotation = Quaternion.RotateTowards(topPivot.rotation, Quaternion.LookRotation(transform.forward), config.rotationSpeed * Time.deltaTime);
                return;
            }

            Vector3 currentTargetPosition = fovTargetSelector.currentTarget.bounds.center;
            Vector3 dir = (currentTargetPosition - topPivot.position).normalized;
            dir.y = 0f;
            var targetRot = Quaternion.LookRotation(dir);
            topPivot.rotation = Quaternion.RotateTowards(topPivot.rotation, targetRot, config.rotationSpeed * Time.deltaTime);
        }

        void CheckActiveProjectiles()
        {
            int count = acitveProjectiles.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var projectileData = acitveProjectiles[i];

                if (Vector3.Distance(projectileData.projectile.transform.position, firePos.position) > projectileData.maxDistance)
                {
                    projectilePool.Release(projectileData.projectile);
                    acitveProjectiles.RemoveAt(i);
                }
            }
        }

        void MoveActiveProjectiles()
        {
            RaycastHit[] raycastHitBuffer = ArrayPool<RaycastHit>.Shared.Rent(2);
            int count = acitveProjectiles.Count;
            int mask = config.obstacleLayerMask | config.targetLayerMask;
            for (int i = 0; i < count; i++)
            {
                ProjectileFireData projectileData = acitveProjectiles[i];
                Vector3 pos = projectileData.projectile.transform.position;
                Vector3 nextPos = pos + (projectileData.direction * (config.projectileSpeed * Time.deltaTime));
                Vector3 diff = nextPos - pos;

                var ray = new Ray(pos, diff);
                Debug.DrawLine(pos, nextPos, Color.red, config.fireCooldown);
                int hitCount = Physics.RaycastNonAlloc(ray, raycastHitBuffer, diff.magnitude, mask);
                if (hitCount == 0)
                {
                    projectileData.projectile.transform.position = nextPos;
                    continue;
                }
                HandleHit(raycastHitBuffer, hitCount, i);
            }
            ArrayPool<RaycastHit>.Shared.Return(raycastHitBuffer, false);
        }

        void Fire()
        {
            currentCooldown += Time.deltaTime;
            if (currentCooldown < config.fireCooldown || fovTargetSelector.currentTarget == null) return;

            var dirToTarget = (fovTargetSelector.currentTarget.bounds.center - topPivot.position).normalized;
            var angle = Vector3.Dot(topPivot.forward, dirToTarget);
            if (angle < 0.65f) return;

            currentCooldown = 0f;
            projectilePool.Get();
        }

        void HandleHit(RaycastHit[] raycastHitBuffer, int hitCount, int projectileDataIndex)
        {
            var projectileData = acitveProjectiles[projectileDataIndex];
            projectileData.maxDistance = 0f;
            projectileData.projectile.transform.position = raycastHitBuffer[0].point;
            acitveProjectiles[projectileDataIndex] = projectileData;

            for (int i = 0; i < hitCount; i++)
            {
                if (raycastHitBuffer[i].transform.TryGetComponent<IDamageable>(out var damageable) && damageable.CanReceiveDamage())
                {
                    damageable.ReceiveDamage(config.damage);
                }
            }
        }

        GameObject CreateProjectile()
        {
            GameObject projectileGo = ProjectileFactory.CreateProjectile(ProjectileUser.Turret);
            projectileGo.transform.position = firePos.position;
            return projectileGo;
        }

        void OnGetProjectile(GameObject projectileGo)
        {
            acitveProjectiles.Add(new ProjectileFireData(projectileGo, firePos.forward, 80f));
            projectileGo.transform.position = firePos.position;
            projectileGo.SetActive(true);
        }

        void OnReleaseProjectile(GameObject projectileGo)
        {
            projectileGo.SetActive(false);
            projectileGo.GetComponent<TrailRenderer>().Clear();
        }

        FieldOfViewData GetFieldOfViewData()
        {
            return new FieldOfViewData(topPivot.position, topPivot.forward, config.FOVAngle, config.FOVDistance);
        }

        void OnHealthChange()
        {
            const float DURATION = 0.25f;
            if (TryGetComponent<TurretDamageTween>(out var tween))
            {
                tween.InitializeTween(DURATION);
                return;
            }
            tween = gameObject.AddComponent<TurretDamageTween>();
            tween.InitializeTween(DURATION);
        }

        void OnHealthDepleted()
        {
            // BUG : Active particles and projectiles won't work anymore
            this.enabled = false;
            Destroy(Instantiate(config.destroyedParticlePrefab), 3f);
            Destroy(this.gameObject, 3f);
            var turretDestroyTween = this.gameObject.AddComponent<TurretDestroyTween>();
            turretDestroyTween.duration = 2.5f;
        }

        bool IDamageable.CanReceiveDamage()
        {
            return health.isDepleted == false;
        }

        void IDamageable.ReceiveDamage(float amount)
        {
            var prevAmount = health.current;
            health.DecreaseCurrentHealth(amount);
            if (health.isDepleted)
            {
                OnHealthDepleted();
                return;
            }

            var newAmount = health.current;
            if (Mathf.Abs(newAmount - prevAmount) > Mathf.Epsilon)
            {
                OnHealthChange();
            }
        }

        Health IDamageable.GetHealth()
        {
            return health;
        }
    }
}