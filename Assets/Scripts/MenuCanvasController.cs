using UnityEngine;
using UnityEngine.InputSystem;

public class MenuCanvasController : MonoBehaviour
{
    [SerializeField] GameObject canvasObject;

    [SerializeField] Transform head;

    [SerializeField] float spawnDistance = 1f;
    [SerializeField] float hideDistance = 3f;

    InputAction toggleAction;

    void Awake()
    {
        if (head == null && Camera.main != null)
            head = Camera.main.transform;

        toggleAction = new InputAction("MenuToggle", InputActionType.Button);
        toggleAction.AddBinding("<XRController>{LeftHand}/menuButton");
        toggleAction.AddBinding("<XRController>{LeftHand}/menu");

        if (canvasObject != null)
            canvasObject.SetActive(false);
    }

    void OnEnable() => toggleAction.Enable();
    void OnDisable() => toggleAction.Disable();

    void Update()
    {
        if (canvasObject == null || head == null)
            return;

        if (toggleAction.WasPressedThisFrame())
        {
            if (canvasObject.activeSelf)
                Hide();
            else
                Show();
        }

        if (canvasObject.activeSelf &&
            Vector3.Distance(head.position, canvasObject.transform.position) > hideDistance)
        {
            Hide();
        }

        canvasObject.transform.rotation = Quaternion.LookRotation(canvasObject.transform.position - head.position);
    }

    void Show()
    {
        Vector3 pos = head.position + head.forward * spawnDistance;
        canvasObject.transform.position = pos;
        
        canvasObject.transform.rotation = Quaternion.LookRotation(pos - head.position);
        canvasObject.SetActive(true);
    }

    void Hide() => canvasObject.SetActive(false);
}
