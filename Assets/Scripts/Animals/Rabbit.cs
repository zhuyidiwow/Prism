using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class Rabbit : AAnimal {
    [SerializeField] private GameObject wheatGrassPrefab;
    [SerializeField] private Vector2 FindWheatGrassTime;
    [SerializeField] private GameObject wheatGrassModel;

    private bool hasFoundGrass;

    private void OnEnable() {
        sourceRun = GetComponent<AudioSource>();
        agent = GetComponent<NavMeshAgent>();
        Initialize();
        state = AnimalState.ROAMING;
        animator.SetTrigger("Run");
        Audio.PlayAudio(sourceRun, clipRun, 0.8f, true);
        SetNewDestination();
        wheatGrassModel.SetActive(false);
    }

    private void Update() {
        if (state == AnimalState.ROAMING &&
            Vector3.Distance(transform.position, agent.destination) < agent.stoppingDistance &&
            !GameManager.IsInteracting) {
            SetNewDestination();
        }
    }

    protected override bool CanInteract() {
        if (GameManager.IsInteracting) {
            return false;
        }

        return true;
    }

    protected override void Interact() {
        LookAway();
        if (!GameManager.IsBoarStarted) {
            StartDialogue("rabbitPreRepetition", EndInteraction, false);
            StopRoaming();
        } else if (GameManager.IsBoarStarted && !GameManager.IsRabbitStarted) {
            GameManager.IsRabbitStarted = true;
            StartDialogue("rabbitIntro", StartRabbitQuest, false);
            GameManager.Instance.LogEvent("Dialogue Started", "Started Rabbit Dialogue", "Frequency", 1);
            GameManager.Instance.LogTiming("Rabbit", (long)Time.time, "Time Starting Rabbit Scenario", "Starting Time");
            StopRoaming();
            GameManager.Instance.StartScenario("rabbit");
        } else if (GameManager.IsRabbitStarted && !hasFoundGrass) {
            StartDialogue("rabbitRepetition", EndInteraction, false);
            StopRoaming();
        } else if (hasFoundGrass & !GameManager.IsRabbitCompleted) {
            GameManager.IsRabbitCompleted = true;
            StartDialogue("rabbitEnd", DeliverWhiteGrass, false);
        } else if (GameManager.IsRabbitCompleted) {
            StartDialogue("rabbitEndRepetition", EndInteraction, false);
            StopRoaming();
            GameManager.Instance.LogEvent("Scenario Finished", "Finished Rabbit Scenario", "Frequency", 1);
            GameManager.Instance.LogTiming("Rabbit", (long)Time.time, "Time Starting Moose Scenario", "Starting Time");
        }
    }

    protected override void EndInteraction() {
        base.EndInteraction();
        ContinueRoaming();
    }

    private void LookAway() {
        Vector3 target = transform.position + (transform.position - Fox.Instance.transform.position).normalized;
        target.y = transform.position.y;
        target = Quaternion.AngleAxis(90f, Vector3.up) * target;
        SmoothLookAt(target);
    }

    private void DeliverWhiteGrass() {
        EndInteraction();
        Fox fox = Fox.Instance;
        if (fox.WheatGrass == null) {
            GameObject grass = Instantiate(wheatGrassPrefab);
            fox.WheatGrass = grass;
            Transform slot = fox.GetAvailableItemSlot();

            grass.transform.parent = slot;
            if (slot.name.Contains("1")) {
                grass.transform.rotation = fox.MouthWheatGrass.rotation;
                grass.transform.position = fox.MouthWheatGrass.position;
            } else {
                grass.transform.rotation = slot.rotation;
                grass.transform.position = slot.position;
            }

            Fox.Instance.PlayCollectItemSound();
            wheatGrassModel.SetActive(false);
        }

        GameManager.Instance.CompleteScenario("rabbit");
    }

    private void GoFindWheatGrass() {
        StartCoroutine(FineWheatGrassCoroutine());
    }

    private IEnumerator FineWheatGrassCoroutine() {
        yield return new WaitForSeconds(Random.Range(FindWheatGrassTime.x, FindWheatGrassTime.y));
        hasFoundGrass = true;
        wheatGrassModel.SetActive(true);
        if (state == AnimalState.ROAMING) {
            StopRoaming();
        }
    }

    private void StartRabbitQuest() {
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
        ContinueRoaming();
        GoFindWheatGrass();
    }
}