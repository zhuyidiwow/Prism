using System.Collections;
using System.Security.Cryptography.X509Certificates;
using UnityEngine;
using UnityEngine.AI;

public class Wolf : AAnimal {
    public bool IsInteracting;

    public void StartFinalDialogue() {
        StartDialogue("wolfCongrats");
    }
    
    public void StopPlayer() {
        SmoothLookAtFox();
        Fox.Instance.StopMoving();
        Fox.Instance.SmoothLookAt(transform.position);
        GameManager.IsInteracting = true;
        CameraManager.Instance.SmoothLookAt(transform.position);
        HideInteractHint();
        DialogueManager.Instance.StartDialogue("wolfStop", EndInteraction);
    }

    public void RunToFox() {
        StartCoroutine(RunInCoroutine());
    }

    private void Start() {
        Initialize();
    }

    protected override void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            if (!GameManager.Instance.HintShown.Contains(LanguageManager.Instance.GetUI("redTree"))) {
                HideInteractHint();
            }
        }
    }

    protected override void OnTriggerStay(Collider other) {
        if (!CanInteract()) return;

        if (other.CompareTag("Player") && InputManager.GetInteractKeyDown() && !IsInteracting) {
            Interact();
        }
    }

    protected override bool CanInteract() {
        if (GameManager.IsInteracting) {
            return false;
        }

        return true;
    }

    protected override void Interact() {
        IsInteracting = true;
        if (GameManager.IsWon) {
            StartDialogue("wolfWon");
        } else if (!GameManager.IsIntroShown) {
#if (UNITY_IOS || UNITY_ANDROID)
            StartDialogue("wolfIntroTouch", StartIntro);
#else
            StartDialogue("wolfIntro", StartIntro);
#endif
            GameManager.Instance.LogEvent("Dialogue Started", "Started Wolf and Owl Dialogue", "Frequency", 1);
        } else {
            if (GameManager.IsDay) {
                if (!GameManager.IsBearStarted) {
                    StartDialogue("wolfDayBear");
                } else if (!GameManager.IsBoarStarted) {
                    StartDialogue("wolfDayBoar");
                } else {
                    StartDialogue("wolfRepetition");
                }
            } else {
                StartDialogue("wolfNight");
            }
        }
    }

    protected override void EndInteraction() {
        IsInteracting = false;
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
    }

    private void StartIntro() {
        transform.parent.GetComponent<IntroCutscene>().StartIntroCutscene();
    }

    private IEnumerator RunInCoroutine() {
        Fox fox = Fox.Instance;
        NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
        RaycastHit hit;

        // finding the starting point for wolf
        Physics.Raycast(fox.transform.position + fox.transform.forward * 5f - fox.transform.right * 10f + Vector3.up * 8f, Vector3.down, out hit, 15f,
            LayerMask.GetMask("Terrain"));

        // this is to make sure wolf does go into the water
        if (hit.point.y < -0.5f) {
            Physics.Raycast(fox.transform.position - fox.transform.forward * 5f - fox.transform.right * 10f + Vector3.up * 8f, Vector3.down, out hit,
                15f,
                LayerMask.GetMask("Terrain"));
        }

        navMeshAgent.enabled = false;
        transform.position = hit.point;

        // find a position to run to
        Physics.Raycast(fox.transform.position + fox.transform.forward * 2f - fox.transform.right * 1f + Vector3.up * 2f, Vector3.down, out hit, 10f,
            LayerMask.GetMask("Terrain"));

        navMeshAgent.enabled = true;
        navMeshAgent.isStopped = false;
        navMeshAgent.SetDestination(hit.point);
        animator.SetTrigger("Run");

        yield return new WaitUntil(() => Vector3.Distance(transform.position, hit.point) < 0.5f);
        navMeshAgent.isStopped = true;
        animator.SetTrigger("Idle");
        SmoothLookAtFox();
    }
}