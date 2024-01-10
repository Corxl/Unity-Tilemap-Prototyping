using State.HealthState;

namespace State
{
    public class StateBase
    {
        public virtual void OnEnterState(StateManager manager)
        {

        }

        public virtual void OnExitState(StateManager manager)
        {

        }

        public virtual void OnStart()
        {

        }

        public virtual void OnUpdate()
        {

        }

        public virtual void OnAwake()
        {

        }

        public virtual void OnDestroy()
        {

        }
    }

    public interface StateManager {
        public void SwitchState(StateBase state);
    }
}

