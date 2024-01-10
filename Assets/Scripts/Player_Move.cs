using States.MovementStates;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D), typeof(MovementStateManager))]
public class Player_Move : MonoBehaviour
{
    private Vector2 move;
    [SerializeField] private GameObject parent;
    public Rigidbody2D body { get; private set; }
    public string state = ""; // debug purposes
    private bool hasStarted = false;
    public Animator animator;
    public Transform sprite;
    public MovementStateManager movement_state_manager { get; private set; }

    // Start is called before the first frame update
    void OnEnable()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        movement_state_manager?.current_movement_state.MovePlayer(body, move);
    }

    private void Update()
    {
        if (!hasStarted)
        {
            this.movement_state_manager = transform.GetComponent<MovementStateManager>();
            body = transform.GetComponent<Rigidbody2D>();
            movement_state_manager.SwitchState(movement_state_manager.idle_state);
            hasStarted = true;
        }
        updateFollowCamera();
    }

    public void updateFollowCamera()
    {
        Camera.main.transform.position = transform.position + new Vector3(0, 0, -10);
    }

    public void onMove(InputAction.CallbackContext context)
    {
        move = context.ReadValue<Vector2>();
    }

    public void onShoot(InputAction.CallbackContext context)
    {


        // StartCoroutine(changeColorThread.changeColor());
        // Debug.Log("RAN!!");
    }

    public void onSprint(InputAction.CallbackContext context)
    {
        Debug.Log(context.performed + " SPRINT");
        if (!(movement_state_manager.current_movement_state is WalkingState) && !(movement_state_manager.current_movement_state is SprintingState)) return;
        movement_state_manager.current_movement_state.OnAction(context);
    }

    public void onCrouch(InputAction.CallbackContext context)
    {
        Debug.Log(context.performed + " <----");
        if (!(movement_state_manager.current_movement_state is WalkingState) && !(movement_state_manager.current_movement_state is CrouchingState)) return;
        if (context.performed)
        {
            movement_state_manager.SwitchState(movement_state_manager.crouching_state);
        }
        movement_state_manager.current_movement_state.OnAction(context);
    }

    
}
