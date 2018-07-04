using UnityEngine;

public class InputManager : MonoBehaviour {
    public static InputManager Instance;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    public static bool GetSkipDialogue() {
        switch (Application.platform) {
            case RuntimePlatform.Android:
            case RuntimePlatform.IPhonePlayer:
                return (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) && !Fox.Instance.IsHowling;
            default:
                return (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !Fox.Instance.IsHowling;
        }
    }

    public static bool GetInteractKeyDown() {
        return Input.GetKeyDown(KeyCode.T);
    }

    public static float GetMouseScroll() {
        return Input.mouseScrollDelta.y;
    }

    public static Vector2 GetMouseAxis() {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

#if (UNITY_IOS || UNITY_ANDROID)
    [SerializeField] private float lookSensitivity = 0.06f;
    [SerializeField] private float tapDetectionThreshold = 0.2f;
    [SerializeField] private float interactDistance = 3f;
    [SerializeField] private float sootheAccelerationThreshold = 1.5f;
    private float tapBeganTime;
    private int tapFingerID;

    private void Start() {
        Input.gyro.enabled = true;
        Physics.queriesHitTriggers = false;
    }

    public Vector2 GetAxisInput2D() {
        if (Input.touchCount > 0) {
            Vector2 touchDeltaPosition = Vector2.zero;
            int divident = Input.touchCount;
            for (int i = 0; i < Input.touchCount; i++) {
                touchDeltaPosition += Input.GetTouch(i).deltaPosition;
            }

            if (divident < 1) divident = 1;
            return touchDeltaPosition / divident * lookSensitivity;
        }

        return Vector2.zero;
    }

    private void Update() {
        if (!GameManager.IsInteracting && !GameManager.IsPaused && !Fox.Instance.IsHowling && Fox.Instance.ReceiveInput) {
            DetectJump();
            DetectMoveAndInteract();
        }

        DetectSoothe();
    }

    private void DetectSoothe() {
        if (Input.gyro.userAcceleration.magnitude > sootheAccelerationThreshold) {
            if (OverloadManager.Instance.IsOverloaded && !OverloadManager.Instance.IsSoothed && !Fox.Instance.IsHowling) {
                Fox.Instance.Stop();
                OverloadManager.Instance.Soothe();
            }
        }
    }

    private void DetectJump() {
        if (Input.touchCount == 2) {
            bool allJustBegan = true;
            foreach (Touch touch in Input.touches) {
                if (touch.phase != TouchPhase.Began) allJustBegan = false;
            }

            if (allJustBegan) {
                Fox.Instance.Jump();
            }
        }
    }

    private void DetectMoveAndInteract() {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Began) {
            tapBeganTime = Time.time;
            tapFingerID = Input.GetTouch(0).fingerId;
        }

        if (Input.touchCount == 1 && Input.GetTouch(0).fingerId == tapFingerID) {
            if (Time.time - tapBeganTime >= tapDetectionThreshold) {
                tapFingerID = -1;
            }

            // ended
            if (Input.GetTouch(0).phase == TouchPhase.Ended && Time.time - tapBeganTime < tapDetectionThreshold) {
                tapFingerID = -1;

                Ray ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);
                RaycastHit hit;
                Physics.Raycast(ray, out hit, 30f);

                if (hit.collider != null) {
                    if (hit.collider.CompareTag("Terrain")) {
                        Fox.Instance.RunTo(hit.point);
                    } else if (hit.collider.CompareTag("Animal")) {
                        if (Vector3.Distance(Fox.Instance.transform.position, hit.point) <= interactDistance) {
                            if (hit.collider.name == "Stag" || hit.collider.name == "Doe") {
                                GameManager.Instance.StagNDeer.GetComponent<StagAndDeer>().RespondToRay();
                            } else {
                                hit.collider.GetComponent<AAnimal>().RespondToRay();
                            }

                            Fox.Instance.Stop();
                        }
                    } else if (hit.collider.CompareTag("Collectable")) {
                        if (Vector3.Distance(Fox.Instance.transform.position, hit.point) <= interactDistance) {
                            hit.collider.GetComponent<Collectable>().Collect();
                        }
                    }
                }
            }
        }
    }

#else
    public Vector2 GetAxisInput2D() {
        return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }
#endif
}