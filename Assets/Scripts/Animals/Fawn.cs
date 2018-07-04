using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class Fawn : AAnimal {
    [HideInInspector] public bool ShouldGoHome;
    [SerializeField] private ParticleSystem heart;
    
    private bool isRunningHome;
    private bool isRunningToDam;
    private bool hasArrived;
    private int interactionCount;
    private int interactionCountBeforeParent;
    private NavMeshAgent navMeshAgent;
    private StagAndDeer parents;

    public void RunToDam() {
        navMeshAgent.isStopped = false;
        navMeshAgent.speed = 2f;
        animator.SetTrigger("Run");
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
        isRunningToDam = true;
    }

    public void Stop() {
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Idle");
        Audio.StopIfPlaying(sourceRun);    
        isRunningToDam = false;
    }

    private void OnEnable() {
        heart.Stop();
    }

    private void Start() {
        if (sourceRun == null) sourceRun = GetComponent<AudioSource>();
        Initialize();
        navMeshAgent = GetComponent<NavMeshAgent>();
        parents = GameManager.Instance.StagNDeer.GetComponent<StagAndDeer>();
    }

    private void Update() {
        if (isRunningHome && !hasArrived) {
            navMeshAgent.SetDestination(GetParentsPosition());
            float distance = Vector3.Distance(transform.position, navMeshAgent.destination);
            if (distance < 0.6f) {
                GameManager.Instance.StagNDeer.GetComponent<StagAndDeer>().Stop();
                GameManager.Instance.Deer.GetComponent<AAnimal>().SmoothLookAt(transform.position);
                GameManager.Instance.Stag.GetComponent<AAnimal>().SmoothLookAt(transform.position);
                navMeshAgent.SetDestination(transform.position);
                navMeshAgent.isStopped = true;
                isRunningHome = false;
                hasArrived = true;
                Audio.StopIfPlaying(sourceRun);
                animator.SetTrigger("Idle");
                heart.Play();
                SmoothLookAt(GameManager.Instance.Deer.transform.position);
            } else if (distance < 10f) {
                if (!GameManager.IsFawnCompleted) {
                    GameManager.IsFawnCompleted = true;
                    GameManager.Instance.CompleteScenario("fawn");
                }
            }
        } else if (isRunningToDam) {
            navMeshAgent.SetDestination(GetParentsPosition());
        }
    }

    protected override void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            if (!GameManager.Instance.HintShown.Contains(LanguageManager.Instance.GetUI("parents"))) {
                HideInteractHint();
            }
        }
    }

    protected override bool CanInteract() {
        if (isRunningHome || GameManager.IsInteracting || hasArrived || GameManager.IsFawnCompleted) {
            return false;
        }

        return true;
    }
    
    protected override void Interact() {
        if (GameManager.IsFawnStarted && !GameManager.IsFawnCompleted) {
            interactionCount++;
            if (interactionCount <= 3) {
                StartDialogue("fawnIntro" + interactionCount, false);
            } else {
                StartDialogue("fawnIntro3", false);
            }

            GameManager.Instance.LogEvent("Dialogue Started", "Started Fawn Dialogue", "Frequency", 1);
        } else {
            interactionCountBeforeParent++;
            StartDialogue("fawnTwinkle", false);
        }
    }

    protected override void EndInteraction() {
        base.EndInteraction();

        if (ShouldGoHome) {
            GoHome();
        } else {
            if (!GameManager.IsFawnStarted) {
                if (interactionCountBeforeParent >= 2) {
                    GameManager.Instance.ShowHint("fawn", 5f);
                }
            }
            else if (GameManager.IsFawnStarted && !GameManager.IsFawnCompleted) {
                ShowInteractHint();
            }
        }
    }

    private void GoHome() {
        isRunningHome = true;
        animator.SetTrigger("Run");
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(GetParentsPosition());
        GameManager.Instance.LogEvent("Scenario Finished", "Finished Fawn Scenario", "Frequency", 1);
    }
    
    private Vector3 GetParentsPosition() {
        return (parents.transform.GetChild(0).position + parents.transform.GetChild(1).position) / 2f;
    }
}