using System.Collections;
using UnityEngine;

public class OverloadManager : MonoBehaviour {
    public static OverloadManager Instance;

    public float EaseStep;
    public float WorsenSpeed;
    public float WorsenTimerCap;
    public bool IsOverloaded;
    public bool IsSoothed;
    [SerializeField] private Vector2 factorRange;
    
    private Coroutine eastOverloadCoroutine;
    private LightManager lightManager;
    private MusicManager musicManager;
    private float overloadFactor; // (0, 1)
    private PostProcManager postManager;

    private float worsenTimer;

    public void Soothe() {
        if (eastOverloadCoroutine == null) {
            GameManager.Instance.HideSootheHint();
            eastOverloadCoroutine = StartCoroutine(SootheCoroutine());
        }
    }

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    private void Start() {
        worsenTimer = WorsenTimerCap;

        musicManager = MusicManager.Instance;
        lightManager = LightManager.Instance;
        postManager = PostProcManager.Instance;
    }

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.O) && !IsOverloaded) TurnToDay();

        if (Input.GetKeyDown(KeyCode.P) && IsOverloaded) TurnToNight();
#endif

        if (Input.GetKeyDown(Settings.Instance.EaseOverloadKey) && IsOverloaded && !IsSoothed && !Fox.Instance.IsHowling && !Fox.Instance.IsSwimmingOut) {
            Soothe();
        }

        if (IsOverloaded) {
            worsenTimer -= Time.deltaTime;
            if (worsenTimer <= 0f) worsenTimer = 0f;

            if (worsenTimer <= 0f && IsSoothed) {
                IsSoothed = false;
                RestartOverload();
            }
        }
    }

    public void TurnToDay(float delay = 0f) {
        StartCoroutine(TurnToDayCoroutine(delay));
        DebugUtility.Instance.Log("[Overload] Turning to day in " + delay + " s");
    }

    public void TurnToNight(float delay = 0f) {
        StartCoroutine(TurnToNightCoroutine(delay));
        DebugUtility.Instance.Log("[Overload] Turning to night in " + delay + " s");
    }

    private void StartOverload() {
        StartCoroutine(StartOverloadCoroutine());
    }

    // called once when timer is zero
    private void RestartOverload() {
        StartCoroutine(RestartOverloadCoroutine());
    }

    private void StopOverload() {
        StartCoroutine(StopOverloadCoroutine());
        DebugUtility.Instance.Log("[Overload] Stopping overload");
    }

    private IEnumerator TurnToNightCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        lightManager.ChangeToNightLight();
        yield return new WaitForSeconds(3f);
        StopOverload();
    }

    private IEnumerator TurnToDayCoroutine(float delay) {
        yield return new WaitForSeconds(delay);
        lightManager.ChangeToDayLight();
        yield return new WaitForSeconds(3f);
        StartOverload();
    }

    private IEnumerator StartOverloadCoroutine() {
        overloadFactor = 1f;
        musicManager.StartDistortion();
        postManager.StartDistortion();
        yield return new WaitForSeconds(3f);
        IsOverloaded = true;
        IsSoothed = false;
    }

    private IEnumerator RestartOverloadCoroutine() {
        float elapsedTime = 0f;
        const float duration = 4f;
        musicManager.StartDistortion();

        while (elapsedTime < duration) {
            overloadFactor = Mathf.Lerp(factorRange.x, factorRange.y, elapsedTime / duration);
            postManager.RespondToOverloadFactor(overloadFactor);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }

    private IEnumerator StopOverloadCoroutine() {
        overloadFactor = 0f;
        IsOverloaded = false;
        MusicManager.Instance.PlayNightMusic();
        postManager.StopDistortion();
        yield return new WaitForSeconds(3f);
        
    }

    private IEnumerator SootheCoroutine() {
        Fox.Instance.StopMoving();
        Fox.Instance.IsHowling = true;
        yield return new WaitForSeconds(0.5f);
        Fox.Instance.StartHowling();
        float elapsedTime = 0f;
        float duration = 4f;
        musicManager.StopDistortion();
        
        while (elapsedTime < duration) {
            overloadFactor = Mathf.Lerp(factorRange.y, factorRange.x, elapsedTime / duration);
            postManager.RespondToOverloadFactor(overloadFactor);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        Fox.Instance.StopHowling();
        yield return new WaitForSeconds(0.3f);

        worsenTimer = Random.Range(WorsenTimerCap * 0.8f, WorsenTimerCap * 1.3f);
        eastOverloadCoroutine = null;
        IsSoothed = true;
        Fox.Instance.IsHowling = false;
        Fox.Instance.RegainControl();
    }
}