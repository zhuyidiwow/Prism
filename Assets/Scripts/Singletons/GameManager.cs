using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {
    [Serializable]
    public struct ScenarioInfo {
        public string Name;
        public bool IsCompleted;
        public Sprite StartedSprite;
        public Sprite CompletedSprite;

        public ScenarioInfo(string name, bool isCompleted, Sprite startedSprite,
            Sprite completedSprite) {
            Name = name;
            IsCompleted = isCompleted;
            StartedSprite = startedSprite;
            CompletedSprite = completedSprite;
        }
    }

    public static GameManager Instance;
    public bool DebugMode;
    public GoogleAnalyticsV4 GoogleAnalytics;
    public GameObject Dam;
    public GameObject Flower;
    [SerializeField] private GameObject wayPoint;

    [Header("Animals")] public GameObject Owl;
    public GameObject Wolf;
    public GameObject Bear;
    public GameObject Stag;
    public GameObject Deer;
    public GameObject StagNDeer;
    public GameObject Fawn;
    public GameObject Boar;
    public GameObject Rabbit;
    public GameObject Moose;
    public GameObject Fireflies;
    public GameObject IntroScenario;
    public GameObject FawnScenario;
    public GameObject MooseScenario;
    public GameObject BoarScenario;
    public GameObject BearScenario;

    [Header("UI")] public GameObject WelcomeCanvas;
    public GameObject CameraCanvas;
    public GameObject HUDCanvas;
    public GameObject EndCanvas;
    public Fader PauseCanvas;
    public GameObject PauseButton;

    // one of the following two will be destroyed depending on platform
    public GameObject TouchControlHint;
    public GameObject KeyboardControlHint;

    [SerializeField] private TextMeshProUGUI pauseText;
    [SerializeField] private GameObject buttonHighlighter;
    [SerializeField] private GameObject[] highlighterArrows;
    [SerializeField] private GameObject[] scenarioUI;
    [SerializeField] private List<ScenarioInfo> allScenarioInfo;
    [SerializeField] private InputField inputField;
    [SerializeField] private Text nameInput;
    [SerializeField] private Text namePlaceholder;

    [Header("State management")] public static bool IsNotGoing;
    public static bool IsInteracting;
    public static bool IsPaused;
    public static bool IsIntroShown;
    public static bool IsFawnStarted;
    public static bool IsFawnCompleted;
    public static bool IsBearStarted;
    public static bool IsBearCompleted;
    public static bool IsBoarStarted;
    public static bool IsBoarCompleted;
    public static bool IsRabbitStarted;
    public static bool IsRabbitCompleted;
    public static bool IsMooseStarted;
    public static bool IsMooseCompleted;
    public static bool IsDay;
    public static bool IsOwlIn;
    public static bool IsWon;

    public static float LastDialogueTime = 0f;
    [HideInInspector] public float FrameRate;

    private bool isNightTimeReflectionDone;
    private List<ScenarioInfo> activeScenarios = new List<ScenarioInfo>();
    private List<GameObject> damPieces = new List<GameObject>();
    private int workingAnimalCount;
    private int highlightedButtonIndex;

    public void ResetInput() {
        namePlaceholder.enabled = false;
        nameInput.enabled = true;
    }

    public void SetName() {
        if (LanguageManager.Instance.IsLegal(nameInput.text)) {
            // start game
            StartGame();
        } else {
            // prompt the player to try again
            nameInput.text = string.Empty;
            nameInput.enabled = false;

            namePlaceholder.enabled = true;
            namePlaceholder.color = new Color(255f / 255f, 39f / 255f, 39f / 255f, 186f / 255f);
            namePlaceholder.text = LanguageManager.Instance.GetUI("illegalName");
        }
    }

    public void LogEvent(string category, string action, string label, long value) {
#if UNITY_ANDROID
        return;
#endif
        DebugUtility.Instance.Log("IN ANALYTICS");
        GoogleAnalytics.LogEvent(category, action, label, value);
        DebugUtility.Instance.Log("[Analytics] " + action);
    }

    public void LogTiming(string category, long interval, string timingName, string label) {
#if UNITY_ANDROID
        return;
#endif
        GoogleAnalytics.LogTiming(category, interval, timingName, label);
        DebugUtility.Instance.Log("[Analytics] " + timingName);
    }

    public void ToggleHelpMenu() {
        if (IsPaused) {
            Continue();
        } else {
            Pause();
            LogEvent("Key Pressed", "H, Pause Menu Popped", "Frequency", 1);
        }
    }

    public void StartNightTimeReflection() {
        if (!isNightTimeReflectionDone) {
            isNightTimeReflectionDone = true;
            IsOwlIn = true;
            StartCoroutine(NightTimeReflectionCoroutine());
        }
    }

    public void ShowDayScenarios() {
        BearScenario.SetActive(true);
        BoarScenario.SetActive(true);
        if (!IsFawnCompleted) FawnScenario.SetActive(false);
        if (!IsMooseCompleted) MooseScenario.SetActive(false);

        for (int i = 0; i < Fireflies.transform.childCount; i++) {
            Fireflies.transform.GetChild(i).gameObject.SetActive(false);
        }
    }

    public void ShowNightScenarios() {
        FawnScenario.SetActive(true);
        MooseScenario.SetActive(true);
        if (!IsBearCompleted) BearScenario.SetActive(false);
        if (!IsBoarCompleted) BoarScenario.SetActive(false);

        for (int i = 0; i < Fireflies.transform.childCount; i++) {
            Fireflies.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    public void StartScenario(string scenarioName) {
        foreach (ScenarioInfo scenarioUi in allScenarioInfo) {
            if (string.Compare(scenarioUi.Name, scenarioName, StringComparison.Ordinal) == 0) {
                allScenarioInfo.Remove(scenarioUi);
                activeScenarios.Add(scenarioUi);
                break;
            }
        }
    }

    public void CompleteScenario(string scenarioName) {
        for (int i = 0; i < activeScenarios.Count; i++) {
            if (string.Compare(activeScenarios[i].Name, scenarioName, StringComparison.Ordinal) == 0) {
                activeScenarios[i] = new ScenarioInfo(
                    activeScenarios[i].Name,
                    true,
                    activeScenarios[i].StartedSprite,
                    activeScenarios[i].CompletedSprite);
            }
        }

        workingAnimalCount++;
        if (workingAnimalCount == 2) {
            StartCoroutine(BuildDamCoroutine());
        }
    }

    public void ShowCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void HideCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // not used for now
    public void EndGame() {
        IsNotGoing = true;
        WelcomeCanvas.SetActive(false);
        HUDCanvas.SetActive(false);
        EndCanvas.SetActive(true);
        OverloadManager.Instance.TurnToNight();
    }

#if (UNITY_IOS || UNITY_ANDROID)

    public void PlaceWayPoint(Vector3 position) {
        wayPoint.SetActive(true);
        wayPoint.transform.position = position;
    }

    public void HideWayPoint() {
        wayPoint.SetActive(false);
    }
#endif

    private void OnEnable() {
        if (Instance == null) Instance = this;
    }

    private void Start() {
#if (UNITY_IOS || UNITY_ANDROID)
        PauseButton.SetActive(false);
        Destroy(KeyboardControlHint);
        CameraManager.Instance.LerpFactor = 10f;
        foreach (GameObject arrow in highlighterArrows) {
            Destroy(arrow);
        }
#else
        IsNotGoing = true;
        Destroy(PauseButton);
        Destroy(TouchControlHint);
        foreach (GameObject arrow in highlighterArrows) {
            arrow.SetActive(true);
        }
        Destroy(wayPoint);
#endif

        StartCoroutine(QualitySettingCoroutine());
        WelcomeCanvas.SetActive(true);
        CameraCanvas.SetActive(true);
        HUDCanvas.SetActive(false);
        EndCanvas.SetActive(false);
        creditsCanvas.SetActive(false);
        PauseCanvas.gameObject.SetActive(false);
        interactHint.gameObject.SetActive(false);
        soothHint.gameObject.SetActive(false);

#if UNITY_WEBGL
        inputField.ActivateInputField();
#endif
        Fox.Instance.GetComponent<Animator>().SetTrigger("Sleep");
        Fox.Instance.ReceiveInput = false;
        CameraManager.Instance.StopFollowingObject();

        BearScenario.SetActive(true);
        FawnScenario.SetActive(false);
        MooseScenario.SetActive(false);

        for (int i = 0; i < Fireflies.transform.childCount; i++) {
            Fireflies.transform.GetChild(i).gameObject.SetActive(false);
        }

        for (int i = 0; i < Dam.transform.childCount; i++) {
            damPieces.Add(Dam.transform.GetChild(i).gameObject);
            damPieces[i].SetActive(false);
        }

        if (SystemInfo.deviceModel.Contains("iPad")) {
            CameraCanvas.transform.localScale *= 0.8f;
        }

        Application.targetFrameRate = 60;
    }

    private void Update() {
        if (IsNotGoing) {
            if (Input.GetKeyDown(KeyCode.Return)) {
                SetName();
            }
        } else {
            if (Input.GetKeyDown(KeyCode.H)) {
                ToggleHelpMenu();
            }

            if (IsPaused) {
                if (Input.GetKeyDown(KeyCode.LeftArrow) ||
                    Input.GetKeyDown(KeyCode.A)) {
                    SelectLastScenario();
                }

                if (Input.GetKeyDown(KeyCode.RightArrow) ||
                    Input.GetKeyDown(KeyCode.D)) {
                    SelectNextScenario();
                }
            }
        }
    }

    private void UpdateScenarioUI() {
        buttonHighlighter.SetActive(false);
        pauseText.text = LanguageManager.Instance.GetUI("clickHint");

        // show active scenario icons
        for (int i = 0; i < activeScenarios.Count; i++) {
            ScenarioInfo scenario = activeScenarios[i];
            GameObject ui = scenarioUI[i];
            Button button = ui.GetComponentInChildren<Button>();
            Image image = button.GetComponent<Image>();
            image.sprite = scenario.IsCompleted
                ? scenario.CompletedSprite
                : scenario.StartedSprite;

            var i1 = i;
            button.onClick.AddListener(() => {
                if (scenario.Name == "boar" && !IsBoarCompleted) {
                    ShowInPauseMenu(GetBoarHint());
                } else if (scenario.Name == "bear" &&
                           Fox.Instance.Honey != null) {
                    ShowInPauseMenu(
                        LanguageManager.Instance.GetUI("bearHintWhenHasHoney"));
                } else {
                    ShowInPauseMenu(scenario.IsCompleted
                        ? LanguageManager.Instance.GetUI(
                            scenario.Name.ToLower() + "Description")
                        : LanguageManager.Instance.GetUI(
                            scenario.Name.ToLower() + "Hint"));
                }

                HighlightButton(button, i1);
            });
        }

        // show question marks
        for (int i = activeScenarios.Count; i < scenarioUI.Length; i++) {
            GameObject ui = scenarioUI[i];
            Button button = ui.GetComponentInChildren<Button>();
            string uiKey;
            if (!IsIntroShown) {
                uiKey = "intro";
                LogEvent("Hint Display", "Intro", "Frequency", 1);
            } else if (IsIntroShown && !IsBearStarted) {
                uiKey = "bearLocation";
                LogEvent("Hint Display", "Bear Location", "Frequency", 1);
            } else if (IsDay && !IsBoarStarted) {
                uiKey = "boarLocation";
                LogEvent("Hint Display", "Boar Location", "Frequency", 1);
            } else if (!IsDay && !IsFawnStarted) {
                uiKey = "fawnLocation";
                LogEvent("Hint Display", "Fawn Location", "Frequency", 1);
            } else if (!IsDay && !IsMooseStarted) {
                uiKey = "mooseLocation";
                LogEvent("Hint Display", "Moose Location", "Frequency", 1);
            } else {
                uiKey = "findAnimal";
                LogEvent("Hint Display", "Find Animal", "Frequency", 1);
            }

            var i1 = i;
            button.onClick.AddListener(() => {
                ShowInPauseMenu(LanguageManager.Instance.GetUI(uiKey));
                HighlightButton(button, i1);
            });
        }

        if ((activeScenarios.Count >= 1 && !activeScenarios[activeScenarios.Count - 1].IsCompleted)
            || activeScenarios.Count == 6) {
            scenarioUI[activeScenarios.Count - 1].GetComponentInChildren<Button>().onClick.Invoke();
        } else {
            StartCoroutine(HighlightQuestionMarkCoroutine());
        }
    }

    //a.k.a Sometimes Unity doesn't function the way you expect it to so you need some stupid hacks to make it work
    private IEnumerator HighlightQuestionMarkCoroutine() {
        yield return new WaitForSeconds(0.2f);
        scenarioUI[activeScenarios.Count].GetComponentInChildren<Button>().onClick.Invoke();
    }

    private void ShowInPauseMenu(string str) {
        pauseText.text = str;
    }

    private void HighlightButton(Button button, int index) {
        buttonHighlighter.SetActive(true);
        Color color = buttonHighlighter.GetComponent<Image>().color;
        color.a = 1f;
        List<Image> images = new List<Image>();
        images.Add(buttonHighlighter.GetComponent<Image>());

#if UNITY_WEBGL
        images.AddRange(buttonHighlighter.GetComponentsInChildren<Image>());
#endif
        foreach (Image image in images) {
            image.color = color;
        }

        buttonHighlighter.GetComponent<Highlighter>().SetPosition(button.GetComponent<RectTransform>().position);
        highlightedButtonIndex = index;
    }

    private string GetBoarHint() {
        Fox fox = Fox.Instance;
        Boar boar = Boar.GetComponent<Boar>();
        string str;
        string strInjured = LanguageManager.Instance.GetUI("boarHint0") + "\n";
        string strFound = LanguageManager.Instance.GetUI("boarHint1");
        string strNeed = LanguageManager.Instance.GetUI("boarHint2");

        if (fox.WheatGrass != null || boar.HasWheatGrass) {
            strFound += "<size=150%><sprite name=\"wheatgrass\"></size>";
        } else {
            strNeed += "<size=150%><sprite name=\"wheatgrass\"></size>";
        }

        if (fox.Berry != null || boar.HasBerry) {
            strFound += "<size=150%><sprite name=\"berry\"></size>";
        } else {
            strNeed += "<size=150%><sprite name=\"berry\"></size>";
        }

        if (fox.Honey != null || boar.HasHoney) {
            strFound += "<size=150%><sprite name=\"honey\"></size>";
        } else {
            strNeed += "<size=150%><sprite name=\"honey\"></size>";
        }

        if (!(fox.WheatGrass != null || boar.HasWheatGrass)) {
            strNeed += "\n" + LanguageManager.Instance.GetUI("boarHintRabbit") + " <size=150%><sprite name=\"wheatgrass\"></size>";
        }

        if (fox.Berry != null || boar.HasBerry ||
            fox.Honey != null || boar.HasHoney ||
            fox.WheatGrass != null || boar.HasWheatGrass) {
            str = strInjured + strFound + "\n" + strNeed;
        } else {
            str = strInjured + strNeed;
        }

        if ((fox.Berry != null || boar.HasBerry) &&
            (fox.Honey != null || boar.HasHoney) &&
            (fox.WheatGrass != null || boar.HasWheatGrass)) {
            str = LanguageManager.Instance.GetUI("boarHintFinished");
        }

        return str;
    }

    // called by the Begin button
    private void StartGame() {
        StartCoroutine(SoothHintCoroutine());
        StartCoroutine(DetectWinConditionCoroutine());
        StartCoroutine(HelpHintCoroutine());

        CameraManager.Instance.ContinueFollowingObject();
        Fox.Instance.StandUp();
        HideCursor();

        IsNotGoing = false;

        Destroy(WelcomeCanvas);
        HUDCanvas.SetActive(true);


#if (UNITY_IOS || UNITY_ANDROID)
        PauseButton.SetActive(true);
        ShowHint("moveTouch", 6f);
#else
        ShowHint("move");
#endif
    }

    private void Pause() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Fox.Instance.StopMoving();
        PauseCanvas.gameObject.SetActive(true);
        UpdateScenarioUI();

        PauseCanvas.FadeIn();
        IsPaused = true;

#if (UNITY_IOS || UNITY_ANDROID)
        PauseButton.SetActive(false);
#endif
    }

    private void Continue() {
        Cursor.visible = false;
        Fox.Instance.RegainControl();

        if (buttonHighlighter.activeSelf) {
            buttonHighlighter.SetActive(false);
        }

        PauseCanvas.FadeOut();
        IsPaused = false;
#if (UNITY_IOS || UNITY_ANDROID)
        PauseButton.SetActive(true);
        Fox.Instance.Stop();
#endif
    }

    private void Ending() {
        StartCoroutine(EndingCoroutine());
    }

    private void SelectLastScenario() {
        int index = highlightedButtonIndex - 1;
        if (index < 0) index = scenarioUI.Length - 1;
        scenarioUI[index].GetComponentInChildren<Button>().onClick.Invoke();
    }

    private void SelectNextScenario() {
        int index = highlightedButtonIndex + 1;
        if (index > scenarioUI.Length - 1) index = 0;
        scenarioUI[index].GetComponentInChildren<Button>().onClick.Invoke();
    }

    private IEnumerator NightTimeReflectionCoroutine() {
        //TODO: put all constants together 
        yield return new WaitForSeconds(Random.Range(4f, 6f));
        Owl.SetActive(true);
        LanguageManager.Dialogue dialogue = LanguageManager.Instance.GetDialogue("owlFirstNight");
        List<LanguageManager.Line> lines = dialogue.Lines;

        if (!IsBearStarted) {
            lines.Add(new LanguageManager.Line("<sprite name=\"owl_face\"> Owl", LanguageManager.Instance.GetHint("bearLocation")));
        } else if (!IsBoarStarted) {
            lines.Add(new LanguageManager.Line("<sprite name=\"owl_face\"> Owl", LanguageManager.Instance.GetHint("boarLocation")));
        }

        dialogue = new LanguageManager.Dialogue(lines, dialogue.Key, dialogue.ChoiceKey);

        Owl.GetComponent<Owl>().FlyIn(dialogue);
    }

    #region Hint

    [Header("Hint")] [SerializeField] private GameObject hintPanel;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private Fader interactHint;
    [SerializeField] private Fader soothHint;

    // To make sure specific hints don't get hidden before timeout
    [HideInInspector] public string HintShown;

    private int interactHintCount;
    private Coroutine hintCoroutine;

    public void ShowJumpHint() {
#if UNITY_WEBGL
        ShowHint("jump");
#endif
    }

    public void ShowInteractHint(string objectName) {
        interactHintCount++;

#if (UNITY_IOS || UNITY_ANDROID)
        if (interactHintCount <= 5) {
            ShowHintStr(LanguageManager.Instance.GetHint("interactTouch") + " on the " + objectName.ToLower() + ".");
        }
#else
        if (interactHintCount >= 3) {
            interactHint.gameObject.SetActive(true);
            interactHint.FadeIn(0.5f);
        } else {
            ShowHintStr(LanguageManager.Instance.GetHint("interact") + " with the " + objectName.ToLower() + ".");
        }
#endif
    }

    // set to 20f to make sure players can see it
    public void ShowHintStr(string hintToShow, float duration = 20f) {
        if (hintCoroutine != null) StopCoroutine(hintCoroutine);
        hintCoroutine = StartCoroutine(ShowHintCoroutine(hintToShow, duration));
    }

    public void ShowHint(string hintKey, float stayTime = 3f) {
        if (hintCoroutine != null) StopCoroutine(hintCoroutine);
        hintCoroutine = StartCoroutine(ShowHintCoroutine(LanguageManager.Instance.GetHint(hintKey), stayTime));
    }

    public void HideInteractHint() {
        if (interactHintCount >= 3) {
            if (interactHint.gameObject.activeSelf) {
                interactHint.FadeOut();
            }
        } else {
            hintPanel.SetActive(false);
        }
#if (UNITY_IOS || UNITY_ANDROID)
        hintPanel.SetActive(false);
#endif
    }

    public void HideSootheHint() {
#if (UNITY_IOS || UNITY_ANDROID)
        hintPanel.SetActive(false);
#endif
        if (soothHint.gameObject.activeSelf) {
            soothHint.FadeOut();
        }
    }

    private IEnumerator ShowHintCoroutine(string hint, float stayTime) {
        hintPanel.SetActive(true);
        hintText.text = hint;
        HintShown = hint;
        Color startColor = new Color(255 / 255f, 1f, 1f, 0f);
        Color targetColor = new Color(255 / 255f, 1f, 1f, 230 / 255f);
        Color textStartColor = new Color(1f, 1f, 1f, 0f);
        Color textTargetColor = new Color(1f, 1f, 1f, 1f);
        Image panelImage = hintPanel.GetComponent<Image>();
        float elapsedTime = 0f;
        const float duration = 0.5f;
        while (elapsedTime <= duration) {
            float percentage = elapsedTime / duration;
            panelImage.color = Color.Lerp(startColor, targetColor, percentage);
            hintText.color = Color.Lerp(textStartColor, textTargetColor, percentage);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        yield return new WaitForSeconds(stayTime);

        elapsedTime = 0f;
        while (elapsedTime <= duration) {
            float percentage = elapsedTime / duration;
            panelImage.color = Color.Lerp(targetColor, startColor, percentage);
            hintText.color = Color.Lerp(textTargetColor, textStartColor, percentage);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        hintPanel.SetActive(false);
    }

    private IEnumerator HelpHintCoroutine() {
        int count = 0;
        while (true) {
            yield return new WaitUntil(() =>
                !hintPanel.activeSelf && !IsInteracting && Time.time - LastDialogueTime >= 60f && activeScenarios.Count < 4);
#if (UNITY_IOS || UNITY_ANDROID)
            ShowHint("helpTouch");
#else
            ShowHint("help");
#endif
            count++;
            
            if (count > 6) {
                break;
            }

            yield return new WaitForSeconds(30f);
        }
    }

    private IEnumerator SoothHintCoroutine() {
        int count = 3;
        while (true) {
            yield return new WaitForSeconds(Random.Range(20f, 30f));
            yield return new WaitUntil(() => !OverloadManager.Instance.IsSoothed && OverloadManager.Instance.IsOverloaded && IsDay);
#if (UNITY_IOS || UNITY_ANDROID)
            ShowHint("sootheTouch");
#else
            soothHint.gameObject.SetActive(true);
            soothHint.FadeIn(0.5f);
//            float startTime = Time.time;
//            yield return new WaitUntil(() => Time.time - startTime > 5f || OverloadManager.Instance.IsSoothed);
            yield return new WaitUntil(() => OverloadManager.Instance.IsSoothed || !IsDay);
            if (soothHint.gameObject.activeSelf) {
                soothHint.FadeOut();
            }
#endif
            count++;
            if (count > 5) {
                break;
            }
        }
    }

    #endregion

    private IEnumerator DetectWinConditionCoroutine() {
        while (true) {
            if (IsBearCompleted && IsBoarCompleted && IsFawnCompleted && IsMooseCompleted) {
                break;
            }

            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(3f);
        OverloadManager.Instance.TurnToNight();
        yield return new WaitForSeconds(3f);

        Owl.SetActive(true);
        Wolf.GetComponent<Wolf>().RunToFox();
        Owl.GetComponent<Owl>().FlyIn(LanguageManager.Instance.GetDialogue("ending"), Ending);
        yield return new WaitForSeconds(1f);
        RaycastHit hit;
        Physics.Raycast(Fox.Instance.transform.position + 2.5f * Fox.Instance.transform.forward + Vector3.up, Vector3.down, out hit, 5f,
            LayerMask.GetMask("Terrain"));
        Flower = Instantiate(Flower, hit.point, Quaternion.identity);
        yield return new WaitForSeconds(9.5f);
        Fox.Instance.SmoothLookAt((Owl.transform.position + Wolf.transform.position) / 2f);
    }

    private IEnumerator BuildDamCoroutine() {
        float baseInterval = 60f * 30f / damPieces.Count;
        while (damPieces.Count > 0) {
            int index = Random.Range(0, damPieces.Count);
            GameObject piece = damPieces[index];
            damPieces.RemoveAt(index);
            piece.SetActive(true);
            yield return new WaitForSeconds(baseInterval / workingAnimalCount);
        }
    }

    private IEnumerator QualitySettingCoroutine() {
        yield return new WaitForSeconds(2f);

        List<float> frameRates = new List<float>();
        int frameRateCount = 180;
        float targetFrameRate = 45f;
        while (frameRates.Count < frameRateCount) {
            frameRates.Add(1f / Time.deltaTime);
            yield return null;
        }

        float sum = 0f;
        foreach (float frame in frameRates) {
            sum += frame;
        }

        FrameRate = sum / frameRateCount;
        DebugUtility.Instance.Log("[Quality] Framerate: " + FrameRate);
        int qualityLevel = QualitySettings.GetQualityLevel();
        DebugUtility.Instance.Log("[Quality] Level: " + qualityLevel);

#if (UNITY_WEBGL || UNITY_IOS || UNITY_ANDROID)
        if (FrameRate < targetFrameRate) {
            while (FrameRate < targetFrameRate && qualityLevel > 0) {
                QualitySettings.SetQualityLevel(--qualityLevel);
                DebugUtility.Instance.Log("[Quality] Quality set to: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
                frameRates.Clear();

                while (frameRates.Count < frameRateCount) {
                    frameRates.Add(1f / Time.deltaTime);
                    yield return null;
                }

                sum = 0f;
                foreach (float frame in frameRates) {
                    sum += frame;
                }

                FrameRate = sum / frameRateCount;
                DebugUtility.Instance.Log("[Quality] Framerate: " + FrameRate);
            }
        } else {
            while (FrameRate > targetFrameRate && qualityLevel < 5) {
                QualitySettings.SetQualityLevel(++qualityLevel);
                DebugUtility.Instance.Log("[Quality] Quality set to: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
                frameRates.Clear();

                while (frameRates.Count < frameRateCount) {
                    frameRates.Add(1f / Time.deltaTime);
                    yield return null;
                }

                sum = 0f;
                foreach (float frame in frameRates) {
                    sum += frame;
                }

                FrameRate = sum / frameRateCount;
                DebugUtility.Instance.Log("[Quality] Framerate: " + FrameRate);
            }
        }
#endif
    }

    #region credits

    [SerializeField] private GameObject creditsCanvas;
    [SerializeField] private Image creditsBackground;
    [SerializeField] private Image imageMain;
    [SerializeField] private Image imageStandy;
    [SerializeField] private Sprite[] creditSprites;
    [SerializeField] private GameObject continueButton;
    [SerializeField] private Text continueButtonText;
    [SerializeField] private Image prismLogo;
    [SerializeField] private TextMeshProUGUI prismName;

    private IEnumerator EndingCoroutine() {
        Fox.Instance.Heart.Play();
        IsNotGoing = true;
        yield return new WaitForSeconds(1.5f);

        MusicManager.Instance.PlayCreditMusic();
        LogEvent("Finished Game", "Playing Credit", "Frequency", 1);
        float elapsedTime = 0f;
        float duration = 3f;

        Transform cam = CameraManager.Instance.transform;
        Transform camEndTransform = CameraManager.Instance.EndingTransform;
        Vector3 camStartPos = cam.position;
        Quaternion camStartRot = cam.rotation;

        while (elapsedTime <= duration) {
            cam.position = Vector3.Lerp(camStartPos, camEndTransform.position, elapsedTime / duration);
            cam.rotation = Quaternion.Lerp(camStartRot, camEndTransform.rotation, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        yield return new WaitForSeconds(5f);

        creditsCanvas.SetActive(true);
        prismLogo.color = Color.clear;
        prismName.color = Color.clear;
        imageMain.sprite = null;
        imageMain.color = Color.clear;
        imageStandy.sprite = null;
        imageStandy.color = Color.clear;

        continueButton.SetActive(false);
        Color transparent = new Color(0f, 0f, 0f, 0f);

        elapsedTime = 0f;
        duration = 3f;
        while (elapsedTime <= duration) {
            creditsBackground.color = Color.Lerp(transparent, Color.black, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        yield return new WaitForSeconds(1.5f);

        Image showingImage, vanishingImage;
        showingImage = imageMain;
        elapsedTime = 0f;
        duration = 1f;
        showingImage.sprite = creditSprites[0];
        while (elapsedTime < duration) {
            showingImage.color = Color.Lerp(Color.clear, Color.white, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        yield return new WaitForSeconds(4f);

        // 17.5
        for (int i = 1; i < creditSprites.Length; i++) {
            Sprite creditSprite = creditSprites[i];

            if (imageMain.sprite == null) {
                showingImage = imageMain;
                vanishingImage = imageStandy;
            } else {
                showingImage = imageStandy;
                vanishingImage = imageMain;
            }

            float localElapsed = 0f;
            float localDuration = 1f;
            showingImage.sprite = creditSprite;

            while (localElapsed < localDuration) {
                showingImage.color = Color.Lerp(Color.clear, Color.white, localElapsed / localDuration);
                vanishingImage.color = Color.Lerp(Color.white, Color.clear, localElapsed / localDuration);
                yield return null;
                localElapsed += Time.deltaTime;
            }

            vanishingImage.sprite = null;

            yield return new WaitForSeconds(4f);
        }

        vanishingImage = imageMain.sprite == null ? imageStandy : imageMain;

        elapsedTime = 0f;
        duration = 1f;
        while (elapsedTime < duration) {
            vanishingImage.color = Color.Lerp(Color.white, Color.clear, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        yield return new WaitForSeconds(1f);

        elapsedTime = 0f;
        duration = 3f;
        Color semiTransparent = new Color(0f, 0f, 0f, 0.85f);
        while (elapsedTime <= duration) {
            creditsBackground.color = Color.Lerp(Color.black, semiTransparent, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        continueButton.SetActive(true);
        elapsedTime = 0f;
        Image continueButtonImage = continueButton.GetComponent<Image>();
        Color buttonColor = continueButtonImage.color;
        Color textColor = continueButtonText.color;

        while (elapsedTime < duration) {
            continueButtonImage.color = Color.Lerp(transparent, buttonColor, elapsedTime / duration);
            continueButtonText.color = Color.Lerp(transparent, textColor, elapsedTime / duration);
            prismName.color = Color.Lerp(Color.clear, Color.white, elapsedTime / duration);
            prismLogo.color = Color.Lerp(Color.clear, Color.white, elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        ShowCursor();
    }

    public void GoBackAfterCredits() {
        StartCoroutine(PostCreditsCoroutine());
    }

    private IEnumerator PostCreditsCoroutine() {
        IsNotGoing = false;
        IsWon = true;
        HideInteractHint();
        MusicManager.Instance.PlayNightMusic();
        creditsCanvas.SetActive(false);
        HideCursor();
        CameraManager.Instance.ContinueFollowingObject();

        yield return new WaitForSeconds(2f);
        Owl.GetComponent<Owl>().FlyAway();
        Wolf.GetComponent<Wolf>().StartFinalDialogue();

//        CameraManager.Instance.ContinueFollowingObject();
//        IsInteracting = false;
//        Fox.Instance.RegainControl();
    }

    #endregion

    public void EnlargeFlower() {
        StartCoroutine(EnlargeFlowerCoroutine());
    }

    private IEnumerator EnlargeFlowerCoroutine() {
        float elapsedTime = 0f;
        float duration = 0.5f;
        Vector3 oriScale = Flower.transform.localScale;
        while (elapsedTime < duration) {
            Flower.transform.localScale = oriScale * (1f + 0.5f * elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        elapsedTime = 0f;
        while (elapsedTime < duration) {
            Flower.transform.localScale = oriScale * (1.5f - 0.5f * elapsedTime / duration);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
    }
}