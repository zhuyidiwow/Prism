using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class Owl : NonInteractiveAnimal {

    [SerializeField] private Transform feetTransform;
    
    private void Start() {
        Initialize();
    }

    public void FlyIn(LanguageManager.Dialogue dialogue, Action callback = null) {
#if (UNITY_IOS || UNITY_ANDROID)
        Fox.Instance.Stop();
#endif
        if (callback == null) {
            StartCoroutine(OwlFlyInCoroutine(dialogue, End));
        } else {
            StartCoroutine(OwlFlyInCoroutine(dialogue, callback));
        }
    }

    public void FlyAway() {
        StartCoroutine(FlyAwayCoroutine());
    }

    private void End() {
        CameraManager.Instance.ContinueFollowingObject();
        OverloadManager.Instance.TurnToDay(Random.Range(3f, 6f));
        GameManager.IsInteracting = false;
        StartCoroutine(FlyAwayCoroutine());
    }

    public Vector3 GetOriginToFeet() {
        return feetTransform.position - transform.position;
    }
    
    // A lot of calculations in this coroutine will look stupid, because the owl pivot is set to a really weird position
    private IEnumerator OwlFlyInCoroutine(LanguageManager.Dialogue dialogue, Action callback) {
        Fox fox = Fox.Instance;
        fox.StopMoving();
        GameManager.IsInteracting = true;
        CameraManager.Instance.StopFollowingObject();
        animator.SetTrigger("Fly");
        
        RaycastHit hit;
        Physics.Raycast(fox.transform.position + fox.transform.forward * 2f + fox.transform.right * 1f + Vector3.up * 2f, Vector3.down, out hit, 10f,
            LayerMask.GetMask("Terrain"));
        
        Vector3 owlTargetPos = hit.point + Vector3.up * 2f; // to make sure feet are always above terrain
        
        SkinnedMeshRenderer skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMeshRenderer.enabled = false;
        transform.position = owlTargetPos;
        InstantLookAtFox();
        Vector3 feetTargetPos = transform.position + GetOriginToFeet();

        Physics.Raycast(feetTargetPos, Vector3.down, out hit, 10f, LayerMask.GetMask("Terrain"));
        Vector3 offset = feetTargetPos - hit.point;
        owlTargetPos -= offset;
        
        Vector3 owlStartPos = fox.transform.position + fox.transform.forward * 15f + Vector3.up * 5f;
        Vector3 owlFlyDir = (owlTargetPos - owlStartPos).normalized;
        
        Transform camTransform = CameraManager.Instance.transform;
        Vector3 camOriginalTarget = camTransform.position + camTransform.forward * Vector3.Distance(owlTargetPos, camTransform.position);
        Vector3 camFinalTarget = owlStartPos;
        
        yield return null;
        
        transform.position = owlStartPos;
        skinnedMeshRenderer.enabled = true;
        transform.LookAt(transform.position + owlFlyDir);

        float elapsedTime = 0f;
        float duration = 1.5f;
        
        while (elapsedTime < duration) {
            camTransform.LookAt(Vector3.Lerp(camOriginalTarget, camFinalTarget, elapsedTime / duration));
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        elapsedTime = 0f;
        duration = 6f;
        while (elapsedTime < duration) {
            transform.LookAt(transform.position + owlFlyDir);
            transform.position = Vector3.Lerp(owlStartPos, owlTargetPos, elapsedTime / duration);
            CameraManager.Instance.transform.LookAt(transform.position);
            Vector3 foxLookAt = transform.position;
            foxLookAt.y = fox.transform.position.y;
            fox.transform.LookAt(foxLookAt);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        animator.SetTrigger("Land");
        CameraManager.Instance.SmoothLookAt(transform.position, fox.transform.position);
        yield return new WaitForSeconds(2.8f);
        animator.SetTrigger("Idle");
        Vector3 target = Fox.Instance.transform.position;
        target.y = transform.position.y;
        SmoothLookAt(target);
        DialogueManager.Instance.StartDialogue(dialogue, callback);
    }

    private IEnumerator FlyAwayCoroutine() {
        GameManager.IsOwlIn = false;
        Fox.Instance.RegainControl();
        animator.SetTrigger("Fly");
        float elapsedTime = 0f;
        const float duration = 3f;
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = transform.position + transform.forward * 5f + Vector3.up * 10f;
        while (elapsedTime < duration) {
            transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        animator.SetTrigger("Land");
        yield return new WaitForSeconds(3f);
        animator.SetTrigger("Idle");
        yield return new WaitForSeconds(2f);
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
        gameObject.SetActive(false);
    }
}