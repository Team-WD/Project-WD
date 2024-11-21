using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;



public class WeaponManager : NetworkBehaviour
{
    [Networked, Capacity(20)] 
    private NetworkArray<ProjectileData> _projectiles => default;

    public float projectileSpeed = 10f;
    public float projectileLifetime = 5f;
    public GameObject bulletPrefab;

    private List<GameObject> bulletVisuals = new List<GameObject>();

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData input))
        {
            if (input.IsFirePressed)
            {
                Fire(input.AimDirection);
            }
        }

        UpdateProjectiles();
    }

    private void Fire(Vector2 direction)
    {
        if (!Object.HasStateAuthority) return;

        for (int i = 0; i < _projectiles.Length; i++)
        {
            ProjectileData projectile = _projectiles[i];
            if (projectile.LifeTime.ExpiredOrNotRunning(Runner))
            {
                projectile = new ProjectileData
                {
                    Position = transform.position,
                    Velocity = direction * projectileSpeed,
                    LifeTime = TickTimer.CreateFromSeconds(Runner, projectileLifetime),
                    OwnerId = Object.Id
                };
                _projectiles.Set(i, projectile);  // 배열 실제로 수정
                break;
            }
        }
    }

    private void UpdateProjectiles()
    {
        for (int i = 0; i < _projectiles.Length; i++)
        {
            ProjectileData projectile = _projectiles[i];
            if (!projectile.LifeTime.ExpiredOrNotRunning(Runner))
            {
                projectile.Position += projectile.Velocity * Runner.DeltaTime;
                CheckCollision(ref projectile);
                _projectiles.Set(i, projectile);
            }
        }
    }

    private void CheckCollision(ref ProjectileData projectile)
    {
        RaycastHit2D hit = Physics2D.Raycast(projectile.Position, projectile.Velocity.normalized, projectile.Velocity.magnitude * Runner.DeltaTime);
        if (hit.collider != null)
        {
            MultiEnemyDamage hitEnemy = hit.collider.GetComponent<MultiEnemyDamage>();
            if (hitEnemy != null)
            {
                hitEnemy.TakeDamage(10, hit.collider.transform.position, projectile.IsCritical);
            }
            projectile.LifeTime = TickTimer.None;
        }
    }

    public override void Render()
    {
        // 기존 시각적 표현 제거
        foreach (var bullet in bulletVisuals)
        {
            Destroy(bullet);
        }
        bulletVisuals.Clear();

        // 새로운 시각적 표현 생성
        for (int i = 0; i < _projectiles.Length; i++)
        {
            ProjectileData projectile = _projectiles[i];
            if (!projectile.LifeTime.ExpiredOrNotRunning(Runner))
            {
                RenderProjectile(projectile);
            }
        }
    }

    private void RenderProjectile(ProjectileData projectile)
    {
        GameObject bulletVisual = Instantiate(bulletPrefab, projectile.Position, Quaternion.identity);
        bulletVisual.transform.rotation = Quaternion.LookRotation(Vector3.forward, projectile.Velocity);
        bulletVisuals.Add(bulletVisual);
    }
}