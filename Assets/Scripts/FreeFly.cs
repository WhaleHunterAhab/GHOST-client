using UnityEngine;
using UnityEngine.InputSystem;
/// </summary>
public class FreeFly : MonoBehaviour
{
    [SerializeField] Transform rigToMove;
    [SerializeField] Transform head;
    [SerializeField] float speed = 3f;
    [SerializeField] float turnSpeed = 60f;

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

        Vector2 move = moveAction.ReadValue<Vector2>();
        Vector3 dir = head.forward * move.y + head.right * move.x;
        if (dir.sqrMagnitude > 1f)
            dir.Normalize();
        if (dir.sqrMagnitude > 0.0001f)
            rigToMove.position += dir * (speed * Time.deltaTime);

        // Right stick: turn OR up/down, never both
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
