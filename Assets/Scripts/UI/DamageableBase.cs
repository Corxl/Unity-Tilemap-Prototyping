using State.HealthState;
using State.HealthState.States;
using UnityEngine;

public abstract class DamageableBase : MonoBehaviour
{
    public float max_health, health;
    public HealthStateManager stateManager;
    public void Awake()
    {
        this.health = max_health;
        this.stateManager = new HealthStateManager(this);
    }
    public void Start()
    {
        this.stateManager.currentState.OnStart();
        this.OnStart();
        HealthEvents.healthEvents.OnHealthChanged(this);
    }

    public virtual void OnStart() {}

    public virtual void OnUpdate() {}

    public void Update()
    {
        this.stateManager.currentState.OnUpdate();
        this.OnUpdate();
    }
    public void Damage(float amount)
    {
        this.health -= amount;
        if (this.health < max_health)
        {
            if (!this.stateManager.currentState.Equals(this.stateManager.POISONED))
                this.stateManager.SwitchState(this.stateManager.INJURED);
        }
        if (this.health <= 0f)
        {
            this.health = 0f;
            this.stateManager.SwitchState(this.stateManager.DEAD);
        }

        HealthEvents.healthEvents.OnHealthChanged(this);
    }

    public float getHealthPercentage()
    {
        return this.health / this.max_health;
    }
    public bool IsFullyHealed()
    {
        return this.health == max_health;
    }

    public void Heal(float amount)
    {
        if (this.health >= max_health) return;

        this.health += amount;
        if (this.health >= max_health)
        {
            this.health = max_health;
            this.stateManager.SwitchState(this.stateManager.HEALTHY);
        }

        HealthEvents.healthEvents.OnHealthChanged(this);
    }

    public virtual void OnDeath()
    {

    }
}
