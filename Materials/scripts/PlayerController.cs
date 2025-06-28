using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Looking")]
    public Transform playerCamera; // Drag your Main Camera here in the Inspector
    public float mouseSensitivity = 100f;
    private float xRotation = 0f;

    [Header("Movement")]
    public float moveSpeed = 12f;
    public float gravity = -19.62f; // A more realistic gravity
    private CharacterController controller;
    private Vector3 velocity;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotation to the camera (up/down) and the player body (left/right)
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        // Check if the player is on the ground, and if so, reset downward velocity
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; // A small downward force to keep it grounded
        }

        float x = Input.GetAxis("Horizontal"); // A/D keys
        float z = Input.GetAxis("Vertical");   // W/S keys

        // Create the move vector relative to where the player is looking
        Vector3 move = transform.right * x + transform.forward * z;

        // Move the player
        controller.Move(move * moveSpeed * Time.deltaTime);

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}