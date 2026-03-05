using UnityEngine;
using UnityEngine.InputSystem;

public class FPSController : MonoBehaviour
{
    public CharacterController controller;
    public Transform cameraPivot;

    public float walkSpeed = 3f;
    public float runSpeed = 7f;
    public float mouseSensitivity = 2f;
    public float gravity = -9.8f;

    float yVelocity;
    float xRotation;

    bool walking = false;

    PlayerInputActions input;

    Vector2 moveInput;
    Vector2 lookInput;

    [Header("Head Bob")]
    public float bobSpeed = 10f;
    public float bobAmount = 0.05f;

    float defaultYPos;
    float timer;

    [Header("Footsteps (Surface Based)")]
    public AudioSource footstepSource;

    // Footstep clips for different surfaces
    public AudioClip[] footstepGrass;
    public AudioClip[] footstepWood;
    public AudioClip[] footstepSand;
    public AudioClip[] footstepStone;

    public float runStepInterval = 0.35f;

    float stepTimer;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultYPos = cameraPivot.localPosition.y;
    }

    void OnEnable()
    {
        input.Enable();

        input.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        input.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        input.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        input.Player.Look.canceled += ctx => lookInput = Vector2.zero;

        input.Player.WalkToggle.performed += ctx => walking = !walking;

        input.Player.Interact.performed += ctx => Interact();
    }

    void Update()
    {
        Move();
        Look();
        HeadBob();
        HandleFootsteps();
    }

    void Move()
    {
        float speed = walking ? walkSpeed : runSpeed;

        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;

        controller.Move(move * speed * Time.deltaTime);

        if (controller.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;

        controller.Move(Vector3.up * yVelocity * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HeadBob()
    {
        if (moveInput.x == 0 && moveInput.y == 0)
        {
            timer = 0;
            Vector3 pos = cameraPivot.localPosition;
            pos.y = Mathf.Lerp(pos.y, defaultYPos, Time.deltaTime * 5f);
            cameraPivot.localPosition = pos;
            return;
        }

        float speed = walking ? bobSpeed : bobSpeed * 1.6f;
        timer += Time.deltaTime * speed;

        Vector3 newPos = cameraPivot.localPosition;
        newPos.y = defaultYPos + Mathf.Sin(timer) * bobAmount;

        cameraPivot.localPosition = newPos;
    }

    void Interact()
    {
        Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 3f))
        {
            if (hit.collider.CompareTag("Interactable"))
            {
                hit.collider.SendMessage("Interact", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    void HandleFootsteps()
    {
        if (!controller.isGrounded) return;  // only grounded
        if ((moveInput.x == 0 && moveInput.y == 0) || walking)
        {
            stepTimer = 0;  // reset timer if idle or walking
            return;
        }

        stepTimer += Time.deltaTime;

        if (stepTimer >= runStepInterval)
        {
            PlayFootstepSurface();
            stepTimer = 0;
        }
    }

    void PlayFootstepSurface()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 2f))
        {
            AudioClip[] clips = null;

            switch (hit.collider.tag)  // check the tag of the surface
            {
                case "Grass":
                    clips = footstepGrass;
                    break;
                case "Wood":
                    clips = footstepWood;
                    break;
                case "Sand":
                    clips = footstepSand;
                    break;
                case "Stone":
                    clips = footstepStone;
                    break;
            }

            if (clips != null && clips.Length > 0)
            {
                int index = Random.Range(0, clips.Length);
                footstepSource.PlayOneShot(clips[index]);
            }
        }
    }
}
