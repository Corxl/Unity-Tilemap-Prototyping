using System.Collections;
using UnityEngine;

namespace State.HealthState.States
{
    public enum HealthStatus : int
    {
        HEALTHY, INJURED, REGENERATING, POISONED, DEAD, NONE
    }
    public class Regenerating : HealthStateBase
    {
        private IEnumerator regenRoutine;
        private float tickDelaySeconds = 1.5f;
        private float healAmount = 15f;
        public Regenerating(DamageableBase entity) : base(entity, HealthStatus.REGENERATING)
        {

        }

        public override void EnterState(StateManager manager)
        {
            this.regenRoutine = TickRegen();
            this.entity.StartCoroutine(regenRoutine);
        }

        public override void ExitState(StateManager manager)
        {
            this.entity.StopCoroutine(this.regenRoutine);
        }

        private IEnumerator TickRegen()
        {
            while (true) {
                this.entity.Heal(healAmount);
                yield return new WaitForSeconds(tickDelaySeconds);
            }
        }

    }

    public class Healthy : HealthStateBase
    {

        public Healthy(DamageableBase damageableEntity) : base(damageableEntity, HealthStatus.HEALTHY)
        {

        }

        public override void EnterState(StateManager manager)
        {

        }

        public override void ExitState(StateManager manager)
        {
            
        }
        
    }

    public class Injured : HealthStateBase
    {
        private int delayToRegeneration = 3;
        private Coroutine countDown;
        public Injured(DamageableBase damageableEntity) : base(damageableEntity, HealthStatus.INJURED)
        {

        }

        public override void EnterState(StateManager manager)
        {
            if (this.entity.IsFullyHealed())
            {
                this.entity.stateManager.SwitchState(this.entity.stateManager.HEALTHY);
                return;
            }
            countDown = this.entity.StartCoroutine(CountDownToRegenerate());
        }

        public override void ExitState(StateManager manager)
        {
            if (this.countDown != null)
            {
                this.entity.StopCoroutine(countDown);
            }
        }
        private IEnumerator CountDownToRegenerate()
        {
            yield return new WaitForSeconds(delayToRegeneration);
            this.entity.stateManager.SwitchState(this.entity.stateManager.REGENERATING);
        }
    }

    public class None : HealthStateBase
    {
        public None(DamageableBase entity) : base(entity, HealthStatus.NONE)
        {

        }

        public override void EnterState(StateManager manager)
        {
            
        }

        public override void ExitState(StateManager manager)
        {
            
        }
    }

    public class Poisoned : HealthStateBase
    {
        private IEnumerator poisonRoutine;
        public float tickDelaySeconds = 0.8f;
        public float damagePerTick = 5f;
        public int duration = 10;
        public Poisoned(DamageableBase entity) : base(entity, HealthStatus.POISONED)
        {
        }


        public override void EnterState(StateManager manager)
        {
            this.poisonRoutine = TickPoison();
            this.entity.StartCoroutine(this.poisonRoutine);
        }

        public override void ExitState(StateManager manager)
        {
            if (poisonRoutine != null)
            {
                this.entity.StopCoroutine(this.poisonRoutine);
            }
        }
        private IEnumerator TickPoison()
        {
            for (int x = 0; x < duration; x++)
            {
                this.entity.Damage(this.damagePerTick);
                yield return new WaitForSeconds(tickDelaySeconds);
            }
            this.entity.stateManager.SwitchState(this.entity.stateManager.INJURED);
        }
    }
    public class Dead : HealthStateBase
    {
        public Dead(DamageableBase entity) : base(entity, HealthStatus.POISONED)
        {

        }

        public override void EnterState(StateManager manager)
        {
            
        }

        public override void ExitState(StateManager manager)
        {

        }
    }
}
