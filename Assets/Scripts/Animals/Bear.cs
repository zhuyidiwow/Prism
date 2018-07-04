using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;
using Random = UnityEngine.Random;

public class Bear : AAnimal {

    [SerializeField] private ParticleSystem heart;
    [SerializeField] private Transform destination;
    [SerializeField] private Transform honeyPosition;

    private bool isRunningToDam;
    private bool hasArrived;
    [SerializeField] private float idleDuration;
    private float idleStartTime;

    private void OnEnable() {
        heart.Stop();
        agent = GetComponent<NavMeshAgent>();
        if (sourceRun == null) sourceRun = GetComponent<AudioSource>();
        Initialize();
        state = AnimalState.ROAMING;
        animator.SetTrigger("Run");
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
        SetNewDestination();
    }

    private void Update() {
        if (isRunningToDam && Vector3.Distance(transform.position, destination.position) < 1f) {
            animator.SetTrigger("Idle");
            agent.isStopped = true;
            hasArrived = true;
            isRunningToDam = false;
            woodLog.SetActive(true);
        } else {
            
            switch (state) {
                case AnimalState.ROAMING:
                    if (Vector3.Distance(transform.position, agent.destination) <= agent.stoppingDistance) {
                        idleStartTime = Time.time;
                        agent.isStopped = true;
                        animator.SetTrigger("Idle");
                        state = AnimalState.WAITING;
                        Audio.StopIfPlaying(sourceRun);
                    }
                    break;
                case AnimalState.WAITING:
                    if (Time.time - idleStartTime >= idleDuration) {
                        ContinueRoaming();
                    }
                    break;
                case AnimalState.IDLE:
                case AnimalState.TALKING:
                default:
                    break;
            }
        }
    }

    protected override bool CanInteract() {
        if (isRunningToDam || GameManager.IsInteracting || Fox.Instance.IsHowling) {
            return false;
        }

        return true;
    }
    
    protected override void Interact() {
        if (GameManager.IsWon) {
            StartDialogue("bearWon", true, true);
        }
        else if (!GameManager.IsBearStarted) {
            StopRoaming();
            StartDialogue("bearIntro");

            GameManager.IsBearStarted = true;
            GameManager.Instance.StartScenario("bear");
            GameManager.Instance.LogEvent("Dialogue Started", "Started Bear Dialogue", "Frequency", 1);
        } else if (GameManager.IsBearStarted && !GameManager.IsBearCompleted) {
            if (Fox.Instance.Honey == null) {
                StopRoaming();
                StartDialogue("bearHungry");
            } else if (Fox.Instance.Honey != null) {
                HideInteractHint();
                Fox.Instance.StopMoving();
                Fox.Instance.SmoothLookAt(transform.position);
                CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position, 0.6f);

                StopRoaming();
                SmoothLookAtFox();
                StartCoroutine(EatHoneyCoroutine());
                heart.Play();

                GameManager.IsInteracting = true;
                GameManager.IsBearCompleted = true;
                GameManager.Instance.CompleteScenario("bear");
            }
        } else if (GameManager.IsBearCompleted && hasArrived) {
            if (Fox.Instance.Honey != null) {
                StopRoaming();
                StartDialogue("bearThanks", true, true);
                heart.Play();
                    
                Destroy(Fox.Instance.Honey);
                Fox.Instance.Honey = null;
            } else {
                StopRoaming();
                StartDialogue("bearDam", true, true);
            }
        }
    }

    protected override void EndInteraction() {
        base.EndInteraction();
        if (!hasArrived) {
            ContinueRoaming();
        }
    }
    
    private void RunToDam() {
        agent.speed = 2.5f;
        agent.isStopped = false;
        agent.SetDestination(destination.position);
        isRunningToDam = true;
        animator.SetTrigger("Fast Run");
        
        OverloadManager.Instance.TurnToNight(Random.Range(3f, 6f));
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
        GameManager.Instance.LogEvent("Scenario Finished", "Finished Bear Scenario", "Frequency", 1);
    }
    
    private IEnumerator EatHoneyCoroutine() {
        GameObject honey = Fox.Instance.Honey;
        honey.transform.parent = transform;
        honey.transform.position = honeyPosition.position;
        honey.transform.rotation = honeyPosition.rotation;
        animator.ResetTrigger("Idle");
        animator.SetTrigger("Eat");

        float elapsedTime = 0f;
        const float duration = 5f;
        Vector3 startScale = honey.transform.localScale;
        while (elapsedTime <= duration) {
            honey.transform.localScale = Vector3.Lerp(startScale, startScale / 6f, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        Destroy(Fox.Instance.Honey);
        Fox.Instance.Honey = null;
        animator.SetTrigger("Idle");

        DialogueManager.Instance.StartDialogue("bearEnd", RunToDam);
    }
}