using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class Moose : AAnimal {
    [SerializeField] private ParticleSystem heart;
    [SerializeField] private Transform destination;

    [HideInInspector] public bool ShouldRespond;
    private bool isRunningToDam;
    private bool hasArrived;
    private int interactionCount;


    private void OnEnable() {
        agent = GetComponent<NavMeshAgent>();
        sourceRun = GetComponent<AudioSource>();
        Initialize();

        state = AnimalState.ROAMING;
        animator.SetTrigger("Run");
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
        SetNewDestination();
        heart.Stop();
    }

    private void Update() {
        if (isRunningToDam && Vector3.Distance(transform.position, destination.position) < agent.stoppingDistance + 0.2f) {
            isRunningToDam = false;
            hasArrived = true;
            agent.isStopped = true;
            animator.ResetTrigger("Run");
            Audio.StopIfPlaying(sourceRun);
            animator.SetTrigger("Idle");
            state = AnimalState.IDLE;
            woodLog.SetActive(true);
        } else if (state == AnimalState.ROAMING) {
            if (Vector3.Distance(transform.position, agent.destination) < agent.stoppingDistance) {
                SetNewDestination();
            }
        }
    }

    protected override bool CanInteract() {
        if (GameManager.IsInteracting) {
            return false;
        }

        if (isRunningToDam && !hasArrived) {
            return false;
        }

        return true;
    }

    protected override void Interact() {
        interactionCount++;
        GameManager.Instance.LogEvent("Interaction Frequency", "Times Interacted with Moose", "Frequency", interactionCount);
        if (GameManager.IsWon) {
            StartCoroutine(MooseWonInteractionSequence());
        } else if (!GameManager.IsMooseStarted) {
            GameManager.IsMooseStarted = true;
            StopRoaming();
            StartDialogue("mooseNoReaction", EndInteraction, false);
            GameManager.Instance.StartScenario("moose");
            GameManager.Instance.LogEvent("Dialogue Started", "Started Moose Dialogue", "Frequency", 1);
        } else if (GameManager.IsMooseStarted && !GameManager.IsMooseCompleted) {
            if (interactionCount >= 3) {
                StopRoaming();
                StartDialogue("mooseSilentTalk", EndFromChoice);
            } else {
                StopRoaming();
                StartDialogue("mooseNoReaction", EndInteraction, false);
            }
        } else if (GameManager.IsMooseCompleted && hasArrived) {
            StartDialogue("mooseEndRepetition", EndInteractionWhenWorking, false);
            GameManager.Instance.LogEvent("Scenario Finished", "Finished Moose Scenario", "Frequency", 1);
        }
    }

    protected override void EndInteraction() {
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();

        RaycastHit hit;
        Physics.Raycast(
            transform.position + (transform.position - Fox.Instance.transform.position) * 5f + Vector3.up * 5f,
            Vector3.down,
            out hit, 10f, LayerMask.GetMask("Terrain"));
        agent.isStopped = false;
        agent.SetDestination(hit.point);
        ContinueRoaming();
        if (interactionCount <= 2) {
            GameManager.Instance.ShowHint("moose" + interactionCount);
        }
    }

    protected override void ContinueRoaming() {
        animator.SetTrigger("Run");
        state = AnimalState.ROAMING;
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
    }

    private void EndFromChoice() {
        if (ShouldRespond) {
            StartCoroutine(MooseInteractionSequence());
        } else {
            EndInteraction();
        }
    }

    private void EndInteractionWhenWorking() {
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
    }

    private IEnumerator MooseInteractionSequence() {
        StopRoaming();
        HideInteractHint();
        Fox.Instance.StopMoving();
        GameManager.IsInteracting = true;
        CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position);
        GameManager.Instance.LogTiming("Moose", (long) Time.time, "Time Starting Moose Scenario", "Starting Time");
        yield return new WaitForSeconds(1.5f);
        SmoothLookAtFox();
        Animator foxAnimator = Fox.Instance.GetComponent<Animator>();
        Animator mooseAnimator = animator;

        foxAnimator.SetTrigger("Sleep");
        yield return new WaitForSeconds(2f);
        mooseAnimator.SetTrigger("Sleep");

        yield return new WaitForSeconds(4f);

        foxAnimator.SetTrigger("Stand Up");
        yield return new WaitForSeconds(2f);
        mooseAnimator.SetTrigger("Stand Up");
        heart.Play();

        yield return new WaitForSeconds(2f);

        foxAnimator.SetTrigger("Nod");
        yield return new WaitForSeconds(2f);
        mooseAnimator.SetTrigger("Nod");

        yield return new WaitForSeconds(2f);
        heart.Play();
        yield return new WaitForSeconds(2f);

        foxAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(1f);
        mooseAnimator.SetTrigger("Idle");
        yield return new WaitForSeconds(0.5f);
        heart.Play();
        yield return new WaitForSeconds(3f);
        GameManager.IsMooseCompleted = true;
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();

        GameManager.Instance.StartNightTimeReflection();
        GameManager.Instance.LogTiming("Moose", (long) Time.time, "Time Finishing Moose Scenario", "Finishing Time");
        GameManager.Instance.CompleteScenario("moose");

        agent.speed = 2.5f;
        agent.isStopped = false;
        agent.SetDestination(destination.position);
        Audio.PlayAudio(sourceRun, clipRun, 0.6f, true);
        animator.SetTrigger("Fast Run");
        isRunningToDam = true;
    }
    
    private IEnumerator MooseWonInteractionSequence() {
        StopRoaming();
        HideInteractHint();
        Fox.Instance.StopMoving();
        GameManager.IsInteracting = true;
        CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position);
        SmoothLookAtFox();
        Animator foxAnimator = Fox.Instance.GetComponent<Animator>();
        Animator mooseAnimator = animator;

        yield return new WaitForSeconds(1f);

        mooseAnimator.SetTrigger("Nod");
        heart.Play();
        yield return new WaitForSeconds(1f);
        foxAnimator.SetTrigger("Nod");

        yield return new WaitForSeconds(3f);
        
        foxAnimator.SetTrigger("Idle");
        mooseAnimator.SetTrigger("Idle");
        
        yield return new WaitForSeconds(1f);
        
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
    }
}