using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utilities;

public class Boar : AAnimal {
    [HideInInspector] public bool HasBerry;
    [HideInInspector] public bool HasHoney;
    [HideInInspector] public bool HasWheatGrass;

    [SerializeField] private ParticleSystem[] heartParticles;
    [SerializeField] private Transform destination;
    [SerializeField] private Transform berryTransform;
    [SerializeField] private Transform honeyTransform;
    [SerializeField] private Transform wheatGrassTransform;

    private bool isRunningToDam;
    private bool hasArrived;
    private bool isRabbitHintInitiated;

    private void OnEnable() {
        foreach (ParticleSystem particle in heartParticles) {
            particle.Stop();
        }

        if (sourceSpeak != null) {
            Audio.PlayAudioRandom(sourceSpeak, clipsSpeak, 1f, true);
        }
    }

    private void Start() {
        Initialize();
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update() {
        if (isRunningToDam && Vector3.Distance(transform.position, agent.destination) < agent.stoppingDistance + 0.3f) {
            agent.isStopped = true;
            animator.SetTrigger("Idle");
            isRunningToDam = false;
            hasArrived = true;
            woodLog.SetActive(true);
        }
    }

    protected override bool CanInteract() {
        if (isRunningToDam || GameManager.IsInteracting) {
            return false;
        }

        return true;
    }
    
    protected override void Interact() {
        if (GameManager.IsWon) {
            StartDialogue("boarWon", true);
        } else 
        if (!GameManager.IsBoarStarted) {
            StartDialogue("boarIntro", false);
            GameManager.IsBoarStarted = true;
            GameManager.Instance.StartScenario("boar");
            GameManager.Instance.LogEvent("Dialogue Started", "Started Boar Dialogue", "Frequency", 1);
        } else if (GameManager.IsBoarStarted && !GameManager.IsBoarCompleted) {
            // creating dynamic dialogue
            Fox fox = Fox.Instance;
            
            if (fox.WheatGrass != null) {
                if (!HasWheatGrass) {
                    HasWheatGrass = true;
                    fox.WheatGrass.transform.parent = wheatGrassTransform;
                    fox.WheatGrass.transform.position = wheatGrassTransform.position;
                    fox.WheatGrass.transform.rotation = wheatGrassTransform.rotation;
                    fox.WheatGrass = null;
                } else {
                    fox.WheatGrass = null;
                }
            }
            
            if (fox.Berry != null) {
                if (!HasBerry) {
                    HasBerry = true;
                    fox.Berry.transform.parent = berryTransform;
                    fox.Berry.transform.position = berryTransform.position;
                    fox.Berry.transform.rotation = berryTransform.rotation;
                    fox.Berry = null;
                } else {
                    fox.Berry = null;
                }
            }

            if (fox.Honey != null) {
                if (!HasHoney) {
                    HasHoney = true;
                    fox.Honey.transform.parent = honeyTransform;
                    fox.Honey.transform.position = honeyTransform.position;
                    fox.Honey.transform.rotation = honeyTransform.rotation;
                    fox.Honey = null;
                } else {
                    fox.Honey = null;
                }
            }

            List<LanguageManager.Line> lines = new List<LanguageManager.Line>();
            LanguageManager.Line needLine = new LanguageManager.Line("<sprite name=\"boar_face\"> Boar", "");

            if (!HasWheatGrass) {
                needLine = new LanguageManager.Line(needLine.Character, needLine.Sentence + "<size=300%><sprite name=\"wheatgrass\"></size>");
            }
            
            if (!HasBerry) {
                needLine = new LanguageManager.Line(needLine.Character, needLine.Sentence + "<size=300%><sprite name=\"berry\"></size>");
            }

            if (!HasHoney) {
                needLine = new LanguageManager.Line(needLine.Character, needLine.Sentence + "<size=300%><sprite name=\"honey\"></size>");
            }

            lines.Add(needLine);
            LanguageManager.Dialogue dialogue = new LanguageManager.Dialogue(lines, "NULL", "NULL");

            if (HasBerry && HasHoney && HasWheatGrass) {
                StartDialogue("boarEnd1", GetCured, false);
            } else {
                // start a customized dialogue
                Fox.Instance.StopMoving();
                GameManager.IsInteracting = true;
                CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position);
                HideInteractHint();
                DialogueManager.Instance.StartDialogue(dialogue, EndInteraction);
            }
        } else if (hasArrived) {
            StartDialogue("boarWork", EndInteraction);
        }
    }
    
    protected override void EndInteraction() {
        base.EndInteraction();
        if (!GameManager.IsBoarCompleted && HasHoney && HasBerry && !HasWheatGrass && !isRabbitHintInitiated) {
            isRabbitHintInitiated = true;
            Invoke("ShowRabbitHint", 30f);
        }
    }

    private void GetCured() {
        StartCoroutine(CureCoroutine());
    }

    private void RunToDam() {
        EndInteraction();
        agent.isStopped = false;
        agent.SetDestination(destination.position);
        animator.SetTrigger("Run");
        GameManager.Instance.LogEvent("Scenario Finished", "Finished Boar Scenario", "Frequency", 1);

        isRunningToDam = true;
        GameManager.IsBoarCompleted = true;
        GameManager.Instance.CompleteScenario("boar");
        OverloadManager.Instance.TurnToNight(Random.Range(5f, 10f));
    }

    private void ShowRabbitHint() {
        if (!HasWheatGrass && Fox.Instance.WheatGrass == null && !GameManager.IsRabbitStarted) {
            GameManager.Instance.ShowHint("rabbit");
        }
    }

    private IEnumerator CureCoroutine() {
        Audio.StopIfPlaying(sourceSpeak);
        yield return new WaitForSeconds(0.6f);
        Destroy(berryTransform.GetChild(0).gameObject);
        heartParticles[0].Play();

        yield return new WaitForSeconds(0.6f);
        Destroy(honeyTransform.GetChild(0).gameObject);
        heartParticles[1].Play();

        yield return new WaitForSeconds(0.6f);
        Destroy(wheatGrassTransform.GetChild(0).gameObject);
        heartParticles[2].Play();

        yield return new WaitForSeconds(1.5f);
        animator.SetTrigger("Stand Up");
        yield return new WaitForSeconds(2f);
        SmoothLookAtFox();
        animator.SetTrigger("Idle");
        yield return new WaitForSeconds(0.8f);

        StartDialogue("boarEnd2", RunToDam);
    }
}