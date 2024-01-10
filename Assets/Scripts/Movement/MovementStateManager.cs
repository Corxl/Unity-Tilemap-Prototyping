using States;
using States.MovementStates;
using System;
using UnityEngine;

[RequireComponent(typeof(Player_Move))]
public class MovementStateManager : MonoBehaviour
{
    public States.Movement sprinting_state { get; private set; }
    public MovementScriptable sprint_stats;
    public States.Movement walking_state { get; private set; }
    public MovementScriptable walk_stats;

    public States.Movement crouching_state { get; private set; }
    public MovementScriptable crouch_stats;

    public States.Movement idle_state { get; private set; }
    public MovementScriptable idle_stats;

    public Movement current_movement_state { get; private set; }
    public string state { get; private set; } = "";
    private Player_Move player;

    private void OnEnable ()
    {
        this.player = transform.GetComponent<Player_Move>();
        sprinting_state = new SprintingState(player);
        walking_state = new WalkingState(player);
        crouching_state = new CrouchingState(player);
        idle_state = new IdleState(player);

        sprinting_state.movement_stats = sprint_stats;
        walking_state.movement_stats = walk_stats;
        crouching_state.movement_stats = crouch_stats;
        idle_state.movement_stats = idle_stats;
    }

    public void SwitchState(Movement state)
    {
        this.current_movement_state?.OnExitState();

        this.current_movement_state = state;
        player.state = this.current_movement_state.name;

        this.current_movement_state.EnterState();
    }
}
