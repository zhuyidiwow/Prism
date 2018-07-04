using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class Fox : MonoBehaviour {
    public static Fox Instance;

    public GameObject ParticleSplash;
    public ParticleSystem Heart;
    [Header("Movement")] public float MoveForce;
    public float MaxSpeed;
    public float JumpSpeed;
    public float LookLerpFactor;

    [Header("Audio")] public AudioClip ClipWalk;
    public AudioClip ClipJump;
    public AudioClip ClipLand;
    public AudioClip ClipHawl;
    public AudioClip ClipCollectItem;
    public AudioClip ClipIntoWater;
    public AudioClip ClipSwim;
    public AudioClip ClipOutWater;

    [HideInInspector] public bool ReceiveInput = true;
    [HideInInspector] public bool IsHowling;
    [HideInInspector] public bool IsSwimmingOut;

    private AudioSource sourceWater;
    private AudioSource sourceSwim;
    private AudioSource sourceSFX;
    private AudioSource sourceJump;
    private AudioSource sourceWalk;
    private AudioSource sourceHowl;

    private Animator animator;
    private Rigidbody rb;

    private bool isJumping;
    private bool isRunning;
    private bool isInWater;
    
    private Transform cameraTransform;
    private Vector3 lookAtTarget;
    private Vector2 input;
    private Coroutine smoothLookCoroutine;

    // inventory
    [HideInInspector] public GameObject WheatGrass;
    [HideInInspector] public GameObject Berry;
    [HideInInspector] public GameObject Honey;
    
    [SerializeField] private Transform[] itemSlots;
    public Transform MouthHoney;
    public Transform MouthBerry;
    public Transform MouthWheatGrass;

    public Transform GetAvailableItemSlot() {
        for (int i = 0; i < itemSlots.Length; i++) {
            if (itemSlots[i].childCount == 0) return itemSlots[i];
        }

        return itemSlots[0];
    }

    public void SmoothLookAt(Vector3 target) {
        if (smoothLookCoroutine != null) StopCoroutine(smoothLookCoroutine);
        smoothLookCoroutine = StartCoroutine(SmoothLookAtCoroutine(target));
    }

    public void StandUp() {
        StartCoroutine(StandUpCoroutine());
    }

    public void StartHowling() {
        animator.SetTrigger("Hawl");
        Audio.PlayAudio(sourceHowl, ClipHawl, 1f, true);
        GameManager.Instance.LogEvent("Key Pressed", "F, Fox Start Howling", "Frequency", 1);
    }

    public void StopHowling() {
        animator.SetTrigger("Idle");
        Audio.StopIfPlaying(sourceHowl);
    }

    public void StopMoving() {
        StartCoroutine(StopInputCoroutine());
#if (UNITY_IOS || UNITY_ANDROID)
        Stop();
#endif
    }

    public void RegainControl() {
        if (!IsHowling && !GameManager.IsOwlIn && !GameManager.IsInteracting) {
            ReceiveInput = true;

#if (UNITY_IOS || UNITY_ANDROID)
            rb.isKinematic = true;
#endif
        }
    }

    public void PlayCollectItemSound() {
        Audio.PlayAudio(sourceSFX, ClipCollectItem);
    }

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    private void Start() {
        sourceSFX = GetComponents<AudioSource>()[0];
        sourceJump = GetComponents<AudioSource>()[1];
        sourceWalk = GetComponents<AudioSource>()[2];
        sourceHowl = GetComponents<AudioSource>()[3];
        sourceWater = gameObject.AddComponent<AudioSource>();
        sourceSwim = gameObject.AddComponent<AudioSource>();

        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
        CameraManager.Instance.SetFollowingObject(gameObject);
        cameraTransform = CameraManager.Instance.transform;
        Heart.Stop();
#if (UNITY_IOS || UNITY_ANDROID)

//        CameraManager.Instance.LerpFactor = 8f;
        rb.isKinematic = true;
        agent = gameObject.AddComponent<NavMeshAgent>();
        agent.baseOffset = -0.04f;
        agent.speed = 3f;
        agent.angularSpeed = 360f;
        agent.radius = 0.26f;
        agent.height = 0.59f;
        agent.enabled = false;
#endif
    }

#if (UNITY_IOS || UNITY_ANDROID)
    private NavMeshAgent agent;

    public void RunTo(Vector3 destination) {
        if (agent.enabled == false) {
            agent.enabled = true;
        }

        agent.isStopped = false;
        agent.SetDestination(destination);

        if (!isRunning) {
            animator.SetTrigger("Run");
            isRunning = true;
        }

        Audio.PlayIfNotAlready(sourceWalk, ClipWalk, 0.3f, true);
        Audio.StopIfPlaying(sourceSwim);
        GameManager.Instance.PlaceWayPoint(destination);
    }

    void Update() {
        if (isRunning && HasReachedDestination()) {
            Stop();
        }
    }

    public void Stop() {
        if (isRunning) {
            agent.isStopped = true;
            agent.enabled = false;
            isRunning = false;
            animator.SetTrigger("Idle");
            Audio.StopIfPlaying(sourceWalk);
        }

        GameManager.Instance.HideWayPoint();
    }

    public void Jump() {
        if (!isJumping) {
            rb.isKinematic = true;
            rb.velocity = new Vector3(rb.velocity.x, JumpSpeed, rb.velocity.z);
            isRunning = false;

            isJumping = true;
            animator.ResetTrigger("Run");
            animator.SetTrigger("Jump");

            Audio.StopIfPlaying(sourceWalk);
            Audio.PlayAudio(sourceJump, ClipJump, 0.5f);
        }
    }

    private void Land() {
        if (isJumping) {
            rb.isKinematic = false;
            isJumping = false;
            Audio.PlayAudio(sourceJump, ClipLand, 0.5f);

            if (agent.enabled && !HasReachedDestination()) {
                isRunning = true;
                animator.SetTrigger("Run");
            } else {
                animator.ResetTrigger("Run");
                isRunning = false;
                animator.SetTrigger("Idle");
            }
        }
    }
    
    private bool HasReachedDestination() {
        return Vector3.Distance(transform.position, agent.destination) < (agent.stoppingDistance + 0.25f);
    }
#else

    private void Update() {
        if (GameManager.IsNotGoing || GameManager.IsPaused || !ReceiveInput) return;

        if (Input.GetKeyDown(Settings.Instance.JumpKey) && !isJumping) {
            Jump();
        }

        Move();
        CapHorizontalSpeed();
        
#if UNITY_WEBGL
        if (!GameManager.IsInteracting && !isRunning && !isJumping) {
            Vector3 offset = CameraManager.Instance.transform.forward;
            offset.y = 0f;
            offset = offset.normalized;
            lookAtTarget = transform.position + offset;
            transform.LookAt(Vector3.Lerp(transform.position + transform.forward, lookAtTarget, LookLerpFactor * Time.deltaTime));
        }
#endif
    }

    private void Jump() {
        rb.velocity = new Vector3(rb.velocity.x, JumpSpeed, rb.velocity.z);
        isRunning = false;

        isJumping = true;
        animator.ResetTrigger("Run");
        animator.SetTrigger("Jump");

        Audio.StopIfPlaying(sourceWalk);
        Audio.PlayAudio(sourceJump, ClipJump, 0.5f);
    }

    private void Land() {
        if (isJumping) {
            isJumping = false;
            Audio.PlayAudio(sourceJump, ClipLand, 0.5f);

            if (ReceiveInput && input.magnitude > 0.1f) {
                isRunning = true;
                animator.SetTrigger("Run");
                Audio.PlayIfNotAlready(sourceWalk, ClipWalk, 0.3f, true);
            } else {
                animator.ResetTrigger("Run");
                isRunning = false;
                Audio.StopIfPlaying(sourceWalk);
                animator.SetTrigger("Idle");
            }
        }
    }
#endif

    private void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.CompareTag("Terrain") && isJumping) {
            Land();
        }

        if (collision.gameObject.CompareTag("Terrain") && isInWater) {
            Audio.PlayAudio(sourceWater, ClipOutWater, 0.5f);
            Audio.StopIfPlaying(sourceSwim);
            Audio.PlayIfNotAlready(sourceWalk, ClipWalk, 0.3f, true);
            isInWater = false;
        }

        if (collision.gameObject.CompareTag("Water")) {
            if (!isInWater) {
#if (UNITY_IOS || UNITY_ANDROID)
                Stop();
                rb.isKinematic = true;
#endif
                SwimBackToLand(collision.contacts[0]);
                float splashSoundVolume = rb.velocity.magnitude / 3f;
                Audio.PlayAudio(sourceWater, ClipIntoWater, splashSoundVolume);
                GameObject splash = Instantiate(ParticleSplash, transform.position, Quaternion.identity);
                Destroy(splash, 3f);
            }
        }
    }

    private void SwimBackToLand(ContactPoint point) {
        isJumping = false;
        isInWater = true;
        IsSwimmingOut = true;

        const int raycastAmount = 8;
        const float runAwayFromWaterDistance = 3f;
        Vector3 rayOrigin = point.point;
        Vector3 runTarget = new Vector3();

        float shortestDistance = 100f;
        float rayAngle = 0f;

        for (int i = 0; i < raycastAmount; i++) {
            float angleInRad = rayAngle * Mathf.Deg2Rad;
            Vector3 rayDir = new Vector3(Mathf.Cos(angleInRad), 0, Mathf.Sin(angleInRad));

            RaycastHit hit;
            Physics.Raycast(rayOrigin, rayDir, out hit, 10f, LayerMask.GetMask("Terrain"));

            float distance = Vector3.Distance(hit.point, rayOrigin);

            if (distance < shortestDistance) {
                shortestDistance = distance;
                runTarget = hit.point;
            }

            rayAngle += 360f / raycastAmount;
        }

        Vector3 dir = (runTarget - rayOrigin).normalized;
        RaycastHit nextHit;

        if (Physics.Raycast(runTarget + dir * runAwayFromWaterDistance + Vector3.up * 5f, Vector3.down, out nextHit, 10f,
            LayerMask.GetMask("Terrain"))) {
            runTarget = nextHit.point;
        }

        DropItems();
        StartCoroutine(RunAwayFromWaterCoroutine(runTarget));
    }

    private void DropItems() {
        if (GameManager.IsBoarCompleted) {
            if (Berry != null) {
                Berry.transform.SetParent(null);
                Destroy(Berry.GetComponent<Berry>());
                foreach (Collider itemCollider in Berry.GetComponents<Collider>()) {
                    itemCollider.enabled = true;
                }

                Rigidbody itemRb = Berry.AddComponent<Rigidbody>();
                itemRb.mass = 0.5f;
                itemRb.drag = 5f;
                Berry = null;
            }
        }

        if (GameManager.IsBearCompleted && GameManager.IsBoarCompleted) {
            if (Honey != null) {
                Honey.transform.SetParent(null);
                Destroy(Honey.GetComponent<Beehive>());
                foreach (Collider itemCollider in Honey.GetComponents<Collider>()) {
                    itemCollider.enabled = true;
                }

                Rigidbody itemRb = Honey.AddComponent<Rigidbody>();
                itemRb.mass = 0.5f;
                itemRb.drag = 5f;
                Honey = null;
            }
        }
    }

    private void Move() {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

        if (Mathf.Abs(input.y) > 0.1f) {
            rb.drag = 0;

            Vector3 forwardDirection = transform.position - cameraTransform.position;
            forwardDirection.y = 0;
            forwardDirection = forwardDirection.normalized;

            Vector3 moveDirection = (forwardDirection * input.y).normalized;

            lookAtTarget = transform.position + moveDirection;
            transform.LookAt(Vector3.Lerp(transform.position + transform.forward, lookAtTarget, LookLerpFactor * Time.deltaTime));

            rb.AddForce(moveDirection * MoveForce);

            if (isJumping) return;

            if (!isRunning) {
                animator.SetTrigger("Run");
                Audio.PlayIfNotAlready(sourceWalk, ClipWalk, 0.3f, true);
                Audio.StopIfPlaying(sourceSwim);
                isRunning = true;
            }
        } else {
            if (!isJumping) rb.drag = 5;

            if (isRunning && rb.velocity.magnitude < 0.06f) {
                animator.ResetTrigger("Run");
                animator.SetTrigger("Idle");
                isRunning = false;
                Audio.StopIfPlaying(sourceWalk);
            }
        }
    }

    private void CapSpeed() {
        if (rb.velocity.magnitude > MaxSpeed) {
            Vector3 maxVelocity = rb.velocity.normalized * MaxSpeed;
            rb.velocity = maxVelocity;
        }
    }

    private void CapHorizontalSpeed() {
        if (rb.velocity.magnitude > MaxSpeed) {
            Vector3 maxVelocity = rb.velocity.normalized * MaxSpeed;
            rb.velocity = new Vector3 {
                x = maxVelocity.x,
                y = rb.velocity.y,
                z = maxVelocity.z
            };
        }
    }

    private IEnumerator StopInputCoroutine() {
        rb.velocity = Vector3.zero;
        ReceiveInput = false;

        yield return new WaitForSeconds(Time.deltaTime * 2f);

        if (!isJumping) {
            rb.drag = 5;
        }

        if (isRunning) {
            animator.ResetTrigger("Run");
            animator.SetTrigger("Idle");
        }

        isRunning = false;
        Audio.StopIfPlaying(sourceWalk);
    }

    private IEnumerator SmoothLookAtCoroutine(Vector3 target, float duration = 0.5f) {
        Vector3 startLookAt = transform.position + transform.forward;

        if (!isJumping) {
            target.y = transform.position.y;
        }

        Vector3 endLookAt = transform.position + (target - transform.position).normalized;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            endLookAt.y = transform.position.y;
            transform.LookAt(Vector3.Lerp(startLookAt, endLookAt, elapsedTime / duration));
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        smoothLookCoroutine = null;
    }

    private IEnumerator RunAwayFromWaterCoroutine(Vector3 target) {
        const float distanceTolerance = 0.3f;
        ReceiveInput = false;
        GetComponent<CapsuleCollider>().material = MaterialContainer.Instance.NoFriction;

        yield return new WaitForSeconds(0.3f);
        animator.SetTrigger("Run");
        Audio.PlayAudio(sourceSwim, ClipSwim, 1f, true);

        float elapsedTime = 0f;
        yield return new WaitUntil(() => {
            elapsedTime += Time.deltaTime;
            Vector3 dir = (target - transform.position).normalized;

            // dir.y = 0f;
            lookAtTarget = transform.position + dir;
            transform.LookAt(Vector3.Lerp(transform.position + transform.forward, lookAtTarget, 30f * Time.deltaTime));

            rb.AddForce(dir * MoveForce);

            if (rb.velocity.magnitude > MaxSpeed / 7f) {
                Vector3 maxVelocity = rb.velocity.normalized * MaxSpeed / 7f;
                rb.velocity = maxVelocity;
            }

            float distance = Vector3.Distance(target, transform.position);
            return distance < distanceTolerance || elapsedTime > 5f;
        });

        rb.velocity = Vector3.zero;
        isRunning = false;
        IsSwimmingOut = false;
        animator.SetTrigger("Idle");
        Audio.StopIfPlaying(sourceSwim);
        Audio.StopIfPlaying(sourceWalk);
        GetComponent<CapsuleCollider>().material = null;
        RegainControl();
    }

    private IEnumerator StandUpCoroutine() {
        GetComponent<Animator>().SetTrigger("Stand Up");
        yield return new WaitForSeconds(1.5f);
        ReceiveInput = true;
    }
}