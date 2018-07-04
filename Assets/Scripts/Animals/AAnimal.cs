using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

public abstract class AAnimal : MonoBehaviour {

    [SerializeField] protected Transform roamCenter;
    [SerializeField] protected float moveRadius;
    [SerializeField] protected AudioClip[] clipsSpeak;
    [SerializeField] protected AudioClip clipRun;
    [SerializeField] protected GameObject woodLog;
    
    protected Animator animator;
    protected AnimalState state;
    protected NavMeshAgent agent;
    protected AudioSource sourceRun;
    
    protected AudioSource sourceSpeak;
    private bool isSpeaking;
    private Coroutine smoothLookCoroutine;

#if (UNITY_IOS || UNITY_ANDROID)
    public void RespondToRay() {
        if (CanInteract()) {
            Interact();
        }
    }
#endif
    
    public void SmoothLookAt(Vector3 target) {
        target.y = transform.position.y;
        if (smoothLookCoroutine != null) StopCoroutine(smoothLookCoroutine);
        smoothLookCoroutine = StartCoroutine(SmoothLookAtCoroutine(target));
    }

    public void SmoothLookAtFox() {
        Vector3 target = Fox.Instance.transform.position;
        target.y = transform.position.y;
        if (smoothLookCoroutine != null) StopCoroutine(smoothLookCoroutine);
        smoothLookCoroutine = StartCoroutine(SmoothLookAtCoroutine(target));
    }

    public void StartSpeaking() {
        if (!isSpeaking) {
            Audio.PlayAudioRandom(sourceSpeak, clipsSpeak);
            animator.SetTrigger("Talk");
            isSpeaking = true;
        }
    }

    public void StopSpeaking(bool stopSound = true) {
        if (isSpeaking) {
            if (stopSound) sourceSpeak.Stop();
            animator.SetTrigger("Idle");
            isSpeaking = false;
        }
    }

    protected virtual bool CanInteract() {
        return true;
    }

    protected abstract void Interact();

    protected virtual void ContinueRoaming() {
        animator.SetTrigger("Run");
        SetNewDestination();
        state = AnimalState.ROAMING;
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
    }

    protected virtual void EndInteraction() {
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
    }

    protected virtual void OnTriggerEnter(Collider other) {
        if (!CanInteract()) return;
        if (other.CompareTag("Player")) {
            ShowInteractHint();
        }
    }

    protected virtual void OnTriggerStay(Collider other) {
        if (!CanInteract()) return;
        if (other.CompareTag("Player") && InputManager.GetInteractKeyDown()) {
            Interact();
        }
    }

    protected virtual void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            HideInteractHint();
        }
    }

    protected void Initialize() {
        animator = GetComponent<Animator>();
        if (sourceSpeak == null) sourceSpeak = gameObject.AddComponent<AudioSource>();
        sourceSpeak.spatialBlend = 1f;
        if (woodLog != null) woodLog.SetActive(false);
    }

    protected void SetNewDestination() {
        RaycastHit hit;
        Physics.Raycast(
            roamCenter.position + Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.up) * transform.forward * moveRadius + Vector3.up * 5f,
            Vector3.down,
            out hit, 10f, LayerMask.GetMask("Terrain"));

        agent.isStopped = false;
        agent.SetDestination(hit.point);
    }

    protected void StopRoaming() {
        agent.isStopped = true;
        animator.ResetTrigger("Run");
        Audio.StopIfPlaying(sourceRun);
        
        // to make sure don't double set "idle"
        if (state == AnimalState.ROAMING) {
            animator.SetTrigger("Idle");
        }
        state = AnimalState.IDLE;
    }

    protected void ShowInteractHint() {
        GameManager.Instance.ShowInteractHint(name);
    }

    protected void HideInteractHint() {
        GameManager.Instance.HideInteractHint();
    }

    protected void StartDialogue(string dialogueKey, Action action, bool lookAtFox = true, bool isAnimalWorking = false) {
        if (lookAtFox) SmoothLookAtFox();
        Fox.Instance.StopMoving();
        Fox.Instance.SmoothLookAt(transform.position);
        GameManager.IsInteracting = true;
        CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position);
        HideInteractHint();
        DialogueManager.Instance.StartDialogue(dialogueKey, action, isAnimalWorking);
    }

    protected void StartDialogue(string dialogueKey, bool lookAtFox = true, bool isAnimalWorking = false) {
        if (lookAtFox) SmoothLookAtFox();
        Fox.Instance.StopMoving();
        Fox.Instance.SmoothLookAt(transform.position);
        GameManager.IsInteracting = true;
        CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position);
        HideInteractHint();
        DialogueManager.Instance.StartDialogue(dialogueKey, EndInteraction, isAnimalWorking);
    }

    protected void InstantLookAtFox() {
        Vector3 foxPos = Fox.Instance.transform.position;
        foxPos.y = transform.position.y;
        transform.LookAt(foxPos);
    }

    private IEnumerator SmoothLookAtCoroutine(Vector3 target, float duration = 0.5f) {
        Vector3 startLookAt = transform.position + transform.forward;
        target.y = transform.position.y;
        Vector3 endLookAt = transform.position + (target - transform.position).normalized;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            transform.LookAt(Vector3.Lerp(startLookAt, endLookAt, elapsedTime / duration));
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        smoothLookCoroutine = null;
    }
}