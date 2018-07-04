using System.Collections;
using UnityEngine;


public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;

    public Transform EndingTransform;
    
    [Tooltip("The max angle to rotate per second")]
    public float RotationSpeedX;
    public float RotationSpeedY;

    public float Distance;
    public Vector2 DistanceRange;
    public float ScrollSensitivity;
    public float LerpFactor;


    public bool CutSceneMode;
    public Vector3 LookTarget; // used in Cutscene mode only

    private float angle_x;
    private float angle_y;

    private GameObject followedObject;
    private Vector3 targetPosition;
    private bool shouldFollowObject;
    private Vector3 lookAtPosition;
    private SkinnedMeshRenderer foxRenderer;
    private Coroutine lookatCoroutine;
    private Coroutine moveCoroutine;
    private bool canRotate;

    public void SetFollowingObject(GameObject objectToFollow) {
        followedObject = objectToFollow;
        shouldFollowObject = true;
    }

    public void ContinueFollowingObject() {
        shouldFollowObject = true;
        canRotate = false;
    }

    public void StopFollowingObject() {
        shouldFollowObject = false;
    }

    public void SmoothLookAt(Vector3 targetA, Vector3 targetB, float lerpFactor = 0.5f, float duration = 1f) {
        SmoothLookAt( targetA * lerpFactor + targetB * (1f - lerpFactor), duration);
    }
    
    public void SmoothLookAt(Vector3 target, float duration = 1f) {
        shouldFollowObject = false;
        if (lookatCoroutine != null) StopCoroutine(lookatCoroutine);
        lookatCoroutine = StartCoroutine(LookAtCoroutine(target, duration));
        lookAtPosition = target;
    }

    public void SmoothMoveTo(Vector3 target, float duration = 1f) {
        shouldFollowObject = false;
        if (moveCoroutine != null) StopCoroutine(moveCoroutine);
        moveCoroutine = StartCoroutine(MoveCoroutine(target, duration));
    }

    private IEnumerator MoveCoroutine(Vector3 target, float duration) {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            transform.position = Vector3.Lerp(startPos, target, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator LookAtCoroutine(Vector3 target, float duration) {
        canRotate = false;
        Vector3 oriTarget = transform.position + transform.forward;
        Vector3 endTarget = transform.position + (target - transform.position).normalized;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            target = Vector3.Lerp(oriTarget, endTarget, elapsedTime / duration);
            transform.LookAt(target);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        lookatCoroutine = null;
        canRotate = true;
    }

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    private void Start() {
        angle_y = -20f;
        angle_x = -122.7f;
        foxRenderer = Fox.Instance.GetComponentInChildren<SkinnedMeshRenderer>();
    }

    private void FixedUpdate() {
        if (GameManager.IsNotGoing || GameManager.IsPaused) return;

        if (shouldFollowObject) {
            FollowObject();
        } else if (canRotate) {
            transform.RotateAround(lookAtPosition, Vector3.up, InputManager.Instance.GetAxisInput2D().x);
            
//            #if UNITY_IOS || UNITY_ANDROID
//            transform.RotateAround(lookAtPosition, transform.right, -InputManager.Instance.GetAxisInput2D().y);
//
//            float distance = Distance;
//            // prevent from going behind terrains
//            RaycastHit hit;
//            Vector3 basePos = lookAtPosition + Vector3.up * 0.4f;
//        
//            Ray ray = new Ray(basePos, (transform.position - basePos).normalized);
//            Physics.Raycast(ray, out hit, 5f, LayerMask.GetMask("Terrain"));
//        
//            if (hit.collider != null) {
//                distance = (hit.point - basePos).magnitude - 1.5f;
//                if (distance < 0.5f) distance = 0.5f;
//            }
//
//            transform.position = ray.origin + ray.direction * distance;
//            CullFox();
//#endif
        }

        if (CutSceneMode) {
            Vector3 currentLookTarget = transform.forward * (LookTarget - transform.position).magnitude;
            Vector3 newLookTarget = Vector3.Lerp(currentLookTarget, LookTarget, Time.deltaTime);
            transform.LookAt(newLookTarget);
        }
    }

    private void FollowObject() {
        CullFox();
        Rotate();
        UpdateTargetPosition();
        // AdjustDistance();
        transform.position = Vector3.Lerp(transform.position, targetPosition, LerpFactor * Time.deltaTime);
    }

    private void CullFox() {
        if (Vector3.Distance(Fox.Instance.transform.position, transform.position) < 1f) {
            foxRenderer.enabled = false;
        } else {
            foxRenderer.enabled = true;
        }
    }

    private void Rotate() {
        angle_x += InputManager.Instance.GetAxisInput2D().x * RotationSpeedX * Time.deltaTime * Settings.Instance.LookSensitivity;
        if (angle_x < -360f) angle_x += 360f;
        if (angle_x > 360f) angle_x -= 360f;
        
        #if (UNITY_IOS || UNITY_ANDROID)
            angle_y += InputManager.Instance.GetAxisInput2D().y * RotationSpeedY * Time.deltaTime * Settings.Instance.LookSensitivity;
            angle_y = Mathf.Clamp(angle_y, -45f, 5f);
        #endif
    
        transform.localRotation = Quaternion.AngleAxis(angle_x, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(angle_y, Vector3.left);
    }

    private void AdjustDistance() {
        Distance -= ScrollSensitivity * InputManager.GetMouseScroll();
        Distance = Mathf.Min(Distance, DistanceRange.y);
        Distance = Mathf.Max(Distance, DistanceRange.x);
    }

    private void UpdateTargetPosition() {
        float angle_x_rad = angle_x * Mathf.Deg2Rad;
        Vector3 horiDirection = new Vector3(-Mathf.Sin(angle_x_rad), 0, -Mathf.Cos(angle_x_rad));
        horiDirection = horiDirection.normalized;

        float angle_y_rad = -angle_y * Mathf.Deg2Rad;
        
        float distance = Distance;

        // prevent from going behind terrains
        RaycastHit hit;
        Vector3 basePos = followedObject.transform.position + Vector3.up * 0.4f;
        
        Ray ray = new Ray(basePos, (transform.position - basePos).normalized);
        Physics.Raycast(ray, out hit, 5f, LayerMask.GetMask("Terrain"));
        
        if (hit.collider != null) {
            distance = (hit.point - basePos).magnitude - 1.5f;
            if (distance < 0.5f) distance = 0.5f;
        }

        Vector3 offsetVector = Vector3.up * distance * Mathf.Sin(angle_y_rad) +
                               horiDirection * distance * Mathf.Cos(angle_y_rad);
        targetPosition = basePos + offsetVector;
    }

}