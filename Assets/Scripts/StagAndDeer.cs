using UnityEngine;
using UnityEngine.AI;

public class StagAndDeer : MonoBehaviour {

    [SerializeField] private Transform roamCenter;
    [SerializeField] private Transform stagDamDestination;
    [SerializeField] private Transform doeDamDestination;
    [SerializeField] private float waitDuration;
    [SerializeField] private float roamRadius;
    [SerializeField] private GameObject[] woodLogs;
    
    private GameObject stag;
    private GameObject doe;
    private Fawn fawn;

    private bool hasThanked;
    private SphereCollider interactionArea;

    private NavMeshAgent stagAgent;
    private NavMeshAgent doeAgent;
    private Animator stagAnimator;
    private Animator doeAnimator;
    private Vector3 stagRoamDestination;
    private AnimalState state;
    private float waitStartTime;
    private Vector3 doeOffset;
    private bool isRunningToDam;
    
    #if (UNITY_IOS || UNITY_ANDROID)
    public void RespondToRay() {
        if (CanInteract()) {
            Interact();
        }
    }
    #endif

    private void OnEnable() {
        stag = GameManager.Instance.Stag;
        stagAgent = stag.GetComponent<NavMeshAgent>();
        stagAnimator = stag.GetComponent<Animator>();
        doe = GameManager.Instance.Deer;
        doeAgent = doe.GetComponent<NavMeshAgent>();
        doeAnimator = doe.GetComponent<Animator>();
        fawn = GameManager.Instance.Fawn.GetComponent<Fawn>();
        interactionArea = GetComponent<SphereCollider>();
        stagRoamDestination = transform.position;
        doeOffset = doe.transform.position - stag.transform.position;
        ContinueRoaming();
        foreach (GameObject woodLog in woodLogs) {
            woodLog.SetActive(false);
        }
    }

    private void Update() {
        interactionArea.center = (stag.transform.localPosition + doe.transform.localPosition) / 2f;

        if (isRunningToDam) {
            if (Vector3.Distance(stag.transform.position, stagDamDestination.position) <= stagAgent.stoppingDistance + 0.2f) {
                isRunningToDam = false;
                stagAgent.isStopped = true;
                stagAnimator.SetTrigger("Idle");
                stag.GetComponent<NonInteractiveAnimal>().StopRunSound();

                doeAgent.isStopped = true;
                doeAnimator.SetTrigger("Idle");
                doe.GetComponent<NonInteractiveAnimal>().StopRunSound();

                fawn.Stop();
                foreach (GameObject woodLog in woodLogs) {
                    woodLog.SetActive(true);
                }
            }            
        } else {
            switch (state) {
                case AnimalState.ROAMING:
                    if (Vector3.Distance(stag.transform.position, stagRoamDestination) <= stagAgent.stoppingDistance + 0.3f) {
                        Stop();
                        waitStartTime = Time.time;
                        state = AnimalState.WAITING;
                    }

                    break;
                case AnimalState.WAITING:
                    if (Time.time - waitStartTime >= waitDuration) {
                        ContinueRoaming();
                    }

                    break;
                case AnimalState.TALKING:
                case AnimalState.IDLE:
                default:
                    break;
            }
        }
    }

    public void Stop() {
        stagAgent.isStopped = true;
        doeAgent.isStopped = true;

        if (state == AnimalState.ROAMING) {
            stagAnimator.SetTrigger("Idle");
            doeAnimator.SetTrigger("Idle");
        }

        stag.GetComponent<NonInteractiveAnimal>().StopRunSound();
        doe.GetComponent<NonInteractiveAnimal>().StopRunSound();
        state = AnimalState.IDLE;
    }

    private void ContinueRoaming() {
        RaycastHit hit;

        do {
            Physics.Raycast(
                roamCenter.position +
                Quaternion.AngleAxis(Random.Range(0, 360f), Vector3.up) * transform.forward * roamRadius * Random.Range(0.85f, 1f) +
                Vector3.up * 5f,
                Vector3.down,
                out hit, 10f, LayerMask.GetMask("Terrain"));
        } while (Vector3.Distance(hit.point, stag.transform.position) < 2f);

        stagRoamDestination = hit.point;

        stagAnimator.SetTrigger("Walk");
        doeAnimator.SetTrigger("Walk");
        stag.GetComponent<NonInteractiveAnimal>().PlayRunSound();
        doe.GetComponent<NonInteractiveAnimal>().PlayRunSound();
        stagAgent.isStopped = false;
        doeAgent.isStopped = false;
        stagAgent.SetDestination(stagRoamDestination);
        doeAgent.SetDestination(stagRoamDestination + doeOffset);

        state = AnimalState.ROAMING;
    }

    private bool CanInteract() {
        if (GameManager.IsInteracting || isRunningToDam) {
            return false;
        }

        return true;
    }

    private void Interact() {
        Stop();
        if (GameManager.IsWon) {
            DialogueManager.Instance.StartDialogue("stagWon", EndInteraction);
            fawn.GetComponent<Fawn>().SmoothLookAtFox();
            
            Fox.Instance.StopMoving();
            Fox.Instance.SmoothLookAt(fawn.transform.position);
            GameManager.IsInteracting = true;
            GameManager.Instance.HideInteractHint();
            CameraManager.Instance.SmoothLookAt((Fox.Instance.transform.position + stag.transform.position + doe.transform.position) / 3f, 0.5f);
            return;
        }
        
        if (!GameManager.IsFawnStarted) {
            DialogueManager.Instance.StartDialogue("stagIntro", EndInteraction);
            GameManager.Instance.LogEvent("Dialogue Started", "Started Stag and Deer Dialogue", "Frequency", 1);
        } else if (GameManager.IsFawnStarted && !GameManager.IsFawnCompleted) {
            DialogueManager.Instance.StartDialogue("stagIntroRepetition", EndInteraction);
        } else if (GameManager.IsFawnCompleted && !hasThanked) {
            DialogueManager.Instance.StartDialogue("stagThanks", RunToDam);
            hasThanked = true;
            GameManager.Instance.LogEvent("Scenario Finished", "Finished Stag and Deer Scenario", "Frequency", 1);
        } else if (GameManager.IsFawnCompleted && hasThanked) {
            DialogueManager.Instance.StartDialogue("stagThanksRepetition", EndInteraction);
        }

        Fox.Instance.StopMoving();
        Fox.Instance.SmoothLookAt(stag.transform.position * 0.5f + doe.transform.position * 0.5f);
        GameManager.IsInteracting = true;
        GameManager.Instance.HideInteractHint();
        stag.GetComponent<AAnimal>().SmoothLookAtFox();
        doe.GetComponent<AAnimal>().SmoothLookAtFox();
        CameraManager.Instance.SmoothLookAt((Fox.Instance.transform.position + stag.transform.position + doe.transform.position) / 3f, 0.5f);
    }
    
    private void EndInteraction() {
        CameraManager.Instance.ContinueFollowingObject();
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();

        if (!GameManager.IsFawnStarted) {
            GameManager.IsFawnStarted = true;
            GameManager.Instance.StartScenario("fawn");
        }

        if (!GameManager.IsFawnCompleted) {
            ContinueRoaming();
        }
    }

    private void RunToDam() {
        EndInteraction();

        stagAgent.isStopped = false;
        stagAgent.speed = 2f;
        stagAgent.SetDestination(stagDamDestination.position);
        stagAnimator.SetTrigger("Run");
        stag.GetComponent<NonInteractiveAnimal>().PlayRunSound();
        
        doeAgent.isStopped = false;
        doeAgent.speed = 2f;
        doeAgent.SetDestination(doeDamDestination.position);
        doeAnimator.SetTrigger("Run");
        doe.GetComponent<NonInteractiveAnimal>().PlayRunSound();
        
        fawn.RunToDam();
        isRunningToDam = true;
        
        GameManager.Instance.StartNightTimeReflection();
    }
    
    private void OnTriggerEnter(Collider other) {
        if (!CanInteract()) return;
        if (other.CompareTag("Player")) {
            GameManager.Instance.ShowInteractHint(name);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!CanInteract()) return;

        if (other.CompareTag("Player") && InputManager.GetInteractKeyDown()) {
            Interact();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            GameManager.Instance.HideInteractHint();
        }
    }
}