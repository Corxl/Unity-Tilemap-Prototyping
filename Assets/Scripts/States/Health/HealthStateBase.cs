using State.HealthState.States;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace State.HealthState
{
    public abstract class HealthStateBase : StateBase
    {
        public DamageableBase entity { get; protected set; }
        public HealthStatus status { get; protected set; }
        public HealthStateBase(DamageableBase damageableEntity, HealthStatus status)
        {
            this.entity = damageableEntity;
            this.status = status;
        }

        public override void OnEnterState(StateManager manager)
        {
            this.EnterState(manager);
        }

        public abstract void EnterState(StateManager manager);

        public override void OnExitState(StateManager manager)
        {
            this.ExitState(manager);
        }

        public abstract void ExitState(StateManager manager);


    }
    public class HealthStateManager : StateManager
    {
        public StateBase currentState { get; private set; }
        #region State References
        public StateBase HEALTHY { get; private set; }
        public StateBase NONE { get; private set; }
        public StateBase POISONED { get; private set; }
        public StateBase REGENERATING { get; private set; }
        public StateBase DEAD { get; private set; }
        public StateBase INJURED { get; private set; }
        #endregion
        public HealthStateManager(DamageableBase entity)
        {
            #region Initialized States
            this.HEALTHY = new Healthy(entity);
            this.NONE = new None(entity);
            this.POISONED = new Poisoned(entity);
            this.DEAD = new Dead(entity);
            this.REGENERATING = new Regenerating(entity);
            this.INJURED = new Injured(entity);
            #endregion

            this.currentState = HEALTHY;
            this.currentState.OnEnterState(this);
        }

        public void SwitchState(StateBase state)
        {
            this.currentState.OnExitState(this);
            this.currentState = state;
            this.currentState.OnEnterState(this);
        }
    }
}
