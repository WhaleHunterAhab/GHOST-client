using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Free-fly locomotion for a Meta (OVR) camera rig. No gravity, no collision, no CharacterController,
/// and NO scaling — it only ever changes the rig's position/rotation, so you can scale the rig in the
/// scene however you like and this won't touch it.
///
///   - Left thumbstick: fly along where you're looking (look up to climb, down to dive; left/right strafes).
///   - Right thumbstick X: smooth turn left / right.
///   - Right thumbstick Y: fly straight up / down.
///
/// Turn and up/down are mutually exclusive: whichever axis you push further wins, and a big deadzone
/// keeps a casual diagonal push from doing either by accident — so you can't turn and rise at once.
///
/// Setup:
///   1. Attach to your Camera Rig (the "[BuildingBlock] Camera Rig" object), or set "Rig To Move".
///   2. Set "Head" to the rig's CenterEyeAnchor (falls back to Camera.main if left empty).
///   3. Remove/disable the Unity XRI ContinuousMoveProvider so they don't conflict.
/// </summary>
public class FreeFly : MonoBehaviour
{
    [Tooltip("The rig root to move. Defaults to this GameObject's transform.")]
    [SerializeField] Transform rigToMove;

    [Tooltip("The head/center-eye transform that defines 'forward'. Defaults to Camera.main.")]
    [SerializeField] Transform head;

    [Tooltip("Fly speed in meters/second. Constant — not affected by rig scale.")]
    [SerializeField] float speed = 3f;

    [Tooltip("Smooth turn speed in degrees/second at full stick.")]
    [SerializeField] float turnSpeed = 60f;

    [Tooltip("Right-stick deadzone. Big on purpose so turn and up/down can't trigger together.")]
    [Range(0f, 0.9f)]
    [SerializeField] float rightStickDeadzone = 0.5f;

    InputAction moveAction;
    InputAction lookAction;

    void Awake()
    {
        if (rigToMove == null)
            rigToMove = transform;
        if (head == null && Camera.main != null)
            head = Camera.main.transform;

        moveAction = new InputAction("FreeFlyMove", InputActionType.Value, expectedControlType: "Vector2");
        moveAction.AddBinding("<XRController>{LeftHand}/thumbstick");
        moveAction.AddBinding("<XRController>{LeftHand}/Primary2DAxis");

        lookAction = new InputAction("FreeFlyLook", InputActionType.Value, expectedControlType: "Vector2");
        lookAction.AddBinding("<XRController>{RightHand}/thumbstick");
        lookAction.AddBinding("<XRController>{RightHand}/Primary2DAxis");
    }

    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
    }

    void Update()
    {
        if (rigToMove == null || head == null)
            return;

        // Left stick: fly along gaze + strafe.
        Vector2 move = moveAction.ReadValue<Vector2>();
        Vector3 dir = head.forward * move.y + head.right * move.x;
        if (dir.sqrMagnitude > 1f)
            dir.Normalize();
        if (dir.sqrMagnitude > 0.0001f)
            rigToMove.position += dir * (speed * Time.deltaTime);

        // Right stick: turn OR up/down, never both. Dominant axis wins, past a big deadzone.
        Vector2 look = lookAction.ReadValue<Vector2>();
        if (Mathf.Abs(look.x) >= Mathf.Abs(look.y))
        {
            if (Mathf.Abs(look.x) > rightStickDeadzone)
                rigToMove.RotateAround(head.position, Vector3.up, look.x * turnSpeed * Time.deltaTime);
        }
        else
        {
            if (Mathf.Abs(look.y) > rightStickDeadzone)
                rigToMove.position += Vector3.up * (look.y * speed * Time.deltaTime);
        }
    }
}
