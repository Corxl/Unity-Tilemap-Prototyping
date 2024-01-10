using States;
using UnityEngine;
using UnityEngine.InputSystem;

namespace States.MovementStates
{
    public class SprintingState : Movement
    {
        private Rigidbody2D body;

        public SprintingState(Player_Move player) : base("Sprinting", player, 1.4f) { }
        public override void EnterState()
        {
            base.EnterState();
            this.player = player;
            this.body = player.body;
        }

        public override void OnAction(InputAction.CallbackContext context)
        {
            if (context.performed) return;
            movementManager.SwitchState(player.movement_state_manager.walking_state);


        }
    }


    public class CrouchingState : Movement
    {
        public CrouchingState(Player_Move player) : base("Crouching", player, 0.6f) { }
        public override void EnterState()
        {
            base.EnterState();
            this.player = player;
        }

        public override void OnAction(InputAction.CallbackContext context)
        {
            if (!context.performed)
                movementManager.SwitchState(player.movement_state_manager.walking_state);
            return;
        }

    }

    public class WalkingState : Movement
    {
        private Rigidbody2D body;

        public WalkingState(Player_Move player) : base("Walking", player)
        {

        }
        public override void EnterState()
        {
            base.EnterState();

            this.player = player;
            this.body = player.body;
            player.animator.Play("Bird_Moving");
        }

        public override void OnAction(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
            movementManager.SwitchState(player.movement_state_manager.sprinting_state);
        }
    }
    public class IdleState : Movement
    {
        public IdleState(Player_Move player) : base("Idle", player)
        {
        }

        public override void EnterState()
        {
            base.EnterState();
            player.animator.Play("Bird_Idle");
        }

        public override void OnAction(InputAction.CallbackContext context)
        {

        }
        public override void MovePlayer(Rigidbody2D body, Vector2 direction)
        {
            if (direction.x != 0 || direction.y != 0)
            {
                this.movementManager.SwitchState(this.movementManager.walking_state);
            }
        }
    }
}

