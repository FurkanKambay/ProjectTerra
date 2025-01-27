using SaintsField;
using Tulip.Data.Gameplay;
using UnityEngine;

namespace Tulip.Data
{
    public abstract class HealthBase : MonoBehaviour, IHealth
    {
        public virtual event IHealth.DamageEvent OnHurt;
        public virtual event IHealth.DeathEvent OnDie;
        public virtual event IHealth.HealEvent OnHeal;
        public virtual event IHealth.ReviveEvent OnRevive;

        [Header("References")]
        [SerializeField] protected SaintsInterface<Component, ITangibleEntity> entity;

        [Header("Config")]
        [SerializeField, Min(0)] protected float maxHealth = 100f;
        [SerializeField, Min(0)] protected float currentHealth = 100f;
        [SerializeField, Min(0)] protected float invulnerabilityDuration;

        public virtual float CurrentHealth
        {
            get => currentHealth;
            protected set => currentHealth = Mathf.Clamp(value, 0, MaxHealth);
        }

        public virtual float MaxHealth => maxHealth;
        public float InvulnerabilityDuration => invulnerabilityDuration;

        public virtual float Ratio => CurrentHealth / MaxHealth;
        public virtual bool IsAlive => CurrentHealth > 0;
        public virtual bool IsDead => CurrentHealth <= 0;
        public virtual bool IsFull => CurrentHealth >= MaxHealth;
        public virtual bool IsHurt => CurrentHealth < MaxHealth && !IsDead;
        public virtual bool IsInvulnerable => remainingInvulnerability > 0;

        public ITangibleEntity Entity => entity.I;
        public virtual IHealth LatestDamageSource { get; protected set; }
        public virtual IHealth LatestDeathSource { get; protected set; }

        /// Remaining seconds of invulnerability
        protected float remainingInvulnerability;

        public abstract InventoryModification Damage(float amount, IHealth source, bool checkInvulnerable = true);
        public abstract void Heal(float amount, IHealth source);
        public abstract void Revive(IHealth reviver = null);

        protected void RaiseOnHurt(HealthChangeEventArgs damage) => OnHurt?.Invoke(damage);
        protected void RaiseOnDie(HealthChangeEventArgs damage) => OnDie?.Invoke(damage);
        protected void RaiseOnHeal(HealthChangeEventArgs healing) => OnHeal?.Invoke(healing);
        protected void RaiseOnRevive(IHealth reviver) => OnRevive?.Invoke(reviver);
    }
}
