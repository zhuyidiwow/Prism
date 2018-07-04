using System.Collections;
using UnityEngine;

public class IntroCutscene : MonoBehaviour {
    public delegate void ScenarioDelegate();

    public event ScenarioDelegate OnIntroShown;

    [SerializeField] private Transform wolfEndPoint;

    [SerializeField] private Transform owlStartPoint;
    [SerializeField] private Transform camTargetTransform;

    private Vector3 owlEndPoint;
    private Wolf wolf;
    private Owl owl;

    void Start() {
        wolf = GameManager.Instance.Wolf.GetComponent<Wolf>();
        owl = GameManager.Instance.Owl.GetComponent<Owl>();
        wolf.gameObject.SetActive(true);
        wolf.transform.position = wolfEndPoint.position;
        wolf.transform.rotation = wolfEndPoint.rotation;
        owl.gameObject.SetActive(false);
    }

    public void StartIntroCutscene() {
        StartCoroutine(IntroCoroutine());
    }

    public void EndIntro() {
        StartCoroutine(EndIntroCoroutine());
    }

    private Vector3 GetOwlEndPosition() {
        Fox fox = Fox.Instance;

        Vector3 targetPoint = wolf.transform.position * 0.9f + fox.transform.position * 0.1f +
                              Quaternion.AngleAxis(90f, Vector3.up) * (wolf.transform.position - fox.transform.position) * 0.8f;

        RaycastHit hit;
        Physics.Raycast(targetPoint + Vector3.up * 2f, Vector3.down, out hit, 10f,
            LayerMask.GetMask("Terrain"));

        Vector3 owlTargetPos = hit.point + Vector3.up * 2f; // to make sure feet are always above terrain

        SkinnedMeshRenderer skinnedMeshRenderer = owl.GetComponentInChildren<SkinnedMeshRenderer>();
        skinnedMeshRenderer.enabled = false;
        owl.transform.position = owlTargetPos;

        Vector3 foxPos = Fox.Instance.transform.position;
        foxPos.y = owl.transform.position.y;
        owl.transform.LookAt(foxPos);

        Vector3 feetTargetPos = owl.transform.position + owl.GetOriginToFeet();

        Physics.Raycast(feetTargetPos, Vector3.down, out hit, 10f, LayerMask.GetMask("Terrain"));
        Vector3 offset = feetTargetPos - hit.point;
        owlTargetPos -= offset;

        skinnedMeshRenderer.enabled = true;
        
        return owlTargetPos;
    }

    private IEnumerator IntroCoroutine() {
        owl.gameObject.SetActive(true);

        owlEndPoint = GetOwlEndPosition();
        StartCoroutine(OwlIntroCoroutine());
        yield return null;

        CameraManager cam = CameraManager.Instance;
        cam.StopFollowingObject();
        yield return null;

        Vector3 startPosition = cam.transform.position;
        Vector3 targetPosition = camTargetTransform.position;
        Vector3 cameraTarget = (owlEndPoint + wolfEndPoint.position) / 2f;
        Vector3 startTarget = cam.transform.position + cam.transform.forward * cameraTarget.magnitude;

        float elapsedTime = 0f;
        const float duration = 2f;
        while (elapsedTime < duration) {
            float percentage = elapsedTime / duration;
            Vector3 pos = Vector3.Lerp(startPosition, targetPosition, percentage);
            Vector3 target = Vector3.Lerp(startTarget, cameraTarget, percentage);
            cam.transform.position = pos;
            cam.transform.LookAt(target);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        CameraManager.Instance.SmoothLookAt(cameraTarget, 0.1f);
        Fox.Instance.SmoothLookAt(cameraTarget);
    }

    private IEnumerator OwlIntroCoroutine() {
        owl.transform.position = owlStartPoint.position;

        float elapsedTime = 0f;
        const float duration = 5f;
        float speed = (owlEndPoint - owlStartPoint.position).magnitude / duration;
        Vector3 dir = (owlEndPoint - owlStartPoint.position).normalized;
        owl.GetComponent<Animator>().SetTrigger("Fly");

        while (elapsedTime < duration) {
            owl.transform.Translate(dir * speed * Time.deltaTime, Space.World);
            owl.transform.LookAt(owl.transform.position + dir);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        owl.GetComponent<Animator>().SetTrigger("Land");
        yield return new WaitForSeconds(2.479f);
        owl.GetComponent<Animator>().SetTrigger("Idle");
        owl.GetComponent<Owl>().SmoothLookAtFox();

        DialogueManager.Instance.StartDialogue("intro", EndIntro);
    }

    private IEnumerator EndIntroCoroutine() {
        MusicManager.Instance.PlayMainMusic();
        owl.GetComponent<Owl>().FlyAway();
        yield return new WaitForSeconds(1f);
        OnIntroShown();
        GameManager.IsIntroShown = true;
        GameManager.Instance.StartScenario("owl");
        GameManager.Instance.CompleteScenario("owl");
        GameManager.IsInteracting = false;
        Fox.Instance.RegainControl();
        CameraManager.Instance.ContinueFollowingObject();
        wolf.GetComponent<Wolf>().IsInteracting = false;
        StartCoroutine(GiveHintCoroutine());
    }

    private IEnumerator GiveHintCoroutine() {
        yield return new WaitForSeconds(1f);
        GameManager.Instance.ShowHint("bear");
        yield return new WaitForSeconds(Random.Range(5f, 10f));
        GameManager.Instance.ShowHint("day", 3f);
        yield return new WaitForSeconds(2f);
        OverloadManager.Instance.TurnToDay();
        yield return new WaitForSeconds(6f);
        yield return new WaitUntil(() => OverloadManager.Instance.IsOverloaded && !OverloadManager.Instance.IsSoothed);
#if (UNITY_IOS || UNITY_ANDROID)
        GameManager.Instance.ShowHint("overloadTouch");
#else
        GameManager.Instance.ShowHint("overload");
#endif
    }
}