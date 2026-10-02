using UnityEngine;
using UnityEngine.InputSystem;

public class FlyCamera : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 20f;
    public float sprintMultiplier = 3f;
    public float smoothTime = 0.08f;

    [Header("Look")]
    public float lookSpeed = 0.15f;
    public float maxPitch = 89f;

    [Header("Terrain Collision")]
    public float minHeightAboveTerrain = 3f;
    public LayerMask groundLayer = ~0;

    private float yaw;
    private float pitch;
    private Vector3 currentVelocity;

    void Start()
    {
        // Read the starting rotation from the Transform
        Vector3 euler = transform.eulerAngles;
        yaw = euler.y;
        pitch = euler.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (Mouse.current == null || Keyboard.current == null)
            return;

        HandleLook();
        HandleMovement();
        PreventTerrainClipping();
        HandleCursorToggle();
    }

    void HandleLook()
    {
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        yaw += mouseDelta.x * lookSpeed;
        pitch -= mouseDelta.y * lookSpeed;
        pitch = Mathf.Clamp(pitch, -maxPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMovement()
    {
        float speed = moveSpeed;

        if (Keyboard.current.leftShiftKey.isPressed)
            speed *= sprintMultiplier;

        Vector3 input = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) input += transform.forward;
        if (Keyboard.current.sKey.isPressed) input -= transform.forward;
        if (Keyboard.current.aKey.isPressed) input -= transform.right;
        if (Keyboard.current.dKey.isPressed) input += transform.right;
        if (Keyboard.current.eKey.isPressed || Keyboard.current.spaceKey.isPressed) input += Vector3.up;
        if (Keyboard.current.qKey.isPressed || Keyboard.current.leftCtrlKey.isPressed) input -= Vector3.up;

        if (input.sqrMagnitude > 1f)
            input.Normalize();

        Vector3 targetVelocity = input * speed;
        transform.position += Vector3.SmoothDamp(Vector3.zero, targetVelocity, ref currentVelocity, smoothTime) * Time.deltaTime;
    }

    void PreventTerrainClipping()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1000f, groundLayer))
        {
            float minY = hit.point.y + minHeightAboveTerrain;

            if (transform.position.y < minY)
            {
                Vector3 pos = transform.position;
                pos.y = minY;
                transform.position = pos;

                if (currentVelocity.y < 0f)
                    currentVelocity.y = 0f;
            }
        }
    }

    void HandleCursorToggle()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Mouse.current.leftButton.wasPressedThisFrame && Cursor.lockState != CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}