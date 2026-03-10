using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class FPSController : MonoBehaviour
{

    [Header("Movement")]
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

    [Header("Interaction UI")]
    public GameObject interactUI;
    public TMP_Text interactUIText;
    public float interactDistance = 3f;
    bool isInteracting = false;

    [Header("Cameras")]
    public Camera mainCamera;
    private Camera currentCamera;

    [Header("Object Holding")]
    public Transform holdPoint;
    public GameObject heldObject;
    public float holdSmooth = 10f;
    public Quaternion heldRotationOffset;

    [Header("Object Rotation")]
    public float rotateSensitivity = 3f;
    bool rotationMode = false;

    [Header("Held Object Sway")]
    public float swayAmount = 0.02f;      // how much it sways
    public float swaySpeed = 5f;          // how fast it sways
    float swayTimer;
    Vector3 holdBasePosition;

    void Awake()
    {
        input = new PlayerInputActions();
    }

    // Returns true if player is running
    public bool IsRunning
    {
        get { return !walking && moveInput.magnitude > 0.1f; }
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        defaultYPos = cameraPivot.localPosition.y;
        currentCamera = mainCamera;
        mainCamera.enabled = true;
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
        if (!isInteracting)
        {
            Move();

            if (!rotationMode)
            {
                Look();
            }

            HeadBob();
            HandleFootsteps();           
        }
        else
        {
            CheckExitInteraction();
        }

        HandleHeldObject();
        HandleRotationMode();

        CheckInteractableUI();
    }

    //Movement
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

    //Look
    void Look()
    {
        float mouseX = lookInput.x * mouseSensitivity * Time.deltaTime * 100f;
        float mouseY = lookInput.y * mouseSensitivity * Time.deltaTime * 100f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);

        cameraPivot.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    //HeadBob
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

    //Interact
    void Interact()
    {
        // If holding an object, drop it
        if (heldObject != null)
        {
            DropObject();
            return;
        }

        Ray ray = new Ray(cameraPivot.position, cameraPivot.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact();

                // Optional: switch camera if object has camera
                Camera cam = interactable.GetInteractionCamera();
                if (cam != null)
                    SwitchCamera(cam);

                if (cam != null)
                {
                    Debug.Log("Switching to camera: " + cam.name);
                    SwitchCamera(cam);
                }
            }
        }
    }

    //Interactable UI
    void CheckInteractableUI()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 1.6f, cameraPivot.forward);
        RaycastHit hit;

        bool interactableDetected = false;  // tracks if we should show UI
        string interactableName = "";

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != null)
            {
                interactableDetected = true;

                // Try to get interactName from MonoBehaviour
                var mb = hit.collider.GetComponent<MonoBehaviour>();
                if (mb != null)
                {
                    var field = mb.GetType().GetField("interactName");
                    if (field != null)
                        interactableName = field.GetValue(mb).ToString();
                }
            }
        }

        // Only toggle UI if state changed
        if (interactableDetected && !interactUI.activeSelf)
        {
            interactUI.SetActive(true);
            interactUIText.text = "Press E to " + interactableName;
        }
        else if (!interactableDetected && interactUI.activeSelf)
        {
            interactUI.SetActive(false);
        }
    }

    //Start/End Interaction
    public void StartInteraction()
    {
        isInteracting = true;

        // Optional: unlock cursor for UI tasks
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void EndInteraction()
    {
        isInteracting = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ReturnToMainCamera();
    }

    //Exit Interaction
    void CheckExitInteraction()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            EndInteraction();
        }
    }

    //Switch Camera
    public void SwitchCamera(Camera newCamera)
    {
        if (currentCamera != null)
            currentCamera.enabled = false;

        currentCamera = newCamera;
        currentCamera.enabled = true;
    }

    public void ReturnToMainCamera()
    {
        SwitchCamera(mainCamera);
    }

    //Hold Object
    void HandleHeldObject()
    {
        if (heldObject == null) return;

        // Smoothly follow hold point
        Vector3 targetPos = holdPoint.position;
        Quaternion targetRot = holdPoint.rotation * heldRotationOffset;

        // Add sway only if player is moving and not in rotation mode
        if (!rotationMode && moveInput.magnitude > 0.1f)
        {
            swayTimer += Time.deltaTime * swaySpeed;
            Vector3 swayOffset = new Vector3(
                Mathf.Sin(swayTimer) * swayAmount,    // sway left/right
                Mathf.Sin(swayTimer * 2f) * swayAmount, // sway up/down
                0
            );
            targetPos += swayOffset;
        }
        else
        {
            swayTimer = 0f; // reset sway when idle
        }

        heldObject.transform.position = Vector3.Lerp(
            heldObject.transform.position,
            targetPos,
            Time.deltaTime * holdSmooth
        );

        if (heldObject != null && heldObject.GetComponent<RuneObject>() != null)
        {
            FinalRunePuzzle puzzle = FindFirstObjectByType<FinalRunePuzzle>();
            if (puzzle != null)
                puzzle.UpdateSlotHighlights(heldObject);
        }

        heldObject.transform.rotation = targetRot;
    }

    //Hold Object Rotation
    void HandleRotationMode()
    {
        if (heldObject == null) return;

        // Enter rotation mode
        if (Keyboard.current.rKey.isPressed)
        {
            rotationMode = true;
        }
        else
        {
            rotationMode = false;
        }

        if (!rotationMode) return;

        float mouseX = lookInput.x * rotateSensitivity;
        float mouseY = lookInput.y * rotateSensitivity;

        heldRotationOffset *= Quaternion.Euler(-mouseY, 0, mouseX);
    }

    //Drop Object
    void DropObject()
    {
        if (heldObject == null) return;

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        Collider col = heldObject.GetComponent<Collider>();

        // move slightly in front of player
        heldObject.transform.position = cameraPivot.position + cameraPivot.forward * 1.2f;

        if (rb != null)
        {
            rb.useGravity = true;
            rb.linearVelocity = Vector3.zero;
        }

        // re-enable collider
        if (col != null)
            col.enabled = true;

        // Check if final puzzle exists in scene
        FinalRunePuzzle puzzle = FindFirstObjectByType<FinalRunePuzzle>();
        if (puzzle != null && heldObject.GetComponent<RuneObject>() != null)
        {
            puzzle.TryPlaceRune(heldObject);
        }

        if (puzzle != null)
        {
            foreach (Transform slot in puzzle.runeSlots)
            {
                SlotHighlight sh = slot.GetComponent<SlotHighlight>();
                if (sh != null)
                    sh.ResetSlot();
            }
        }

        heldObject = null;
    }

    //Footsteps
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

    //Footsteps audio
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
                footstepSource.pitch = Random.Range(0.95f, 1.05f);
                footstepSource.PlayOneShot(clips[index]);
            }
        }
    }
}
