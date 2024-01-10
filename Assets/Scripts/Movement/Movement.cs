
using UnityEngine;
using UnityEngine.InputSystem;

namespace States
{
    public abstract class Movement
    {
        public string name { get; private set; }
        public MovementScriptable movement_stats { get; set; }
        private float previous_x, previous_y;
        protected Player_Move player;
        protected MovementStateManager movementManager;
        protected float animtionSpeed = 1f;
        public Movement(string name, Player_Move player, float animtionSpeed = 1f)
        {
            this.player = player;
            this.movementManager = player.movement_state_manager;
            this.name = name;
            this.animtionSpeed = animtionSpeed;
        }
        public virtual void EnterState()
        {
            this.player.animator.speed = this.animtionSpeed;
            this.movementManager = player.movement_state_manager;
        }

        public virtual void UpdateMoveScale(Vector2 direction)
        {
            Transform transform = player.sprite;
            if (direction.x <= 0)
            {
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x), transform.localScale.y);
            }
            else
            {
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * -1f, transform.localScale.y);
            }  
        }
        public virtual void MovePlayer(Rigidbody2D body, Vector2 direction)
        {
            if (direction.x == 0 && direction.y == 0)
            {
                body.velocity = new Vector2(0, 0);
                movementManager.SwitchState(player.movement_state_manager.idle_state);
                return;
            }
            this.UpdateMoveScale(direction);
            body.velocity = (player.transform.right * direction.x + player.transform.up * direction.y).normalized * (this.movement_stats.movement_speed * 50f) * Time.fixedDeltaTime;
            //Debug.Log("WH");
        }

        public virtual void OnAction(InputAction.CallbackContext context)
        {

        }
        public virtual void OnUpdate()
        {

        }

        public virtual void OnExitState()
        {

        }
    }
}
