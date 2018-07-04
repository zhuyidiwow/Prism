using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUtility : MonoBehaviour {
    public static DebugUtility Instance;

    [SerializeField] private GameObject debugCanvas;
    [SerializeField] private GameObject debugPointPrefab;

    private TextMeshProUGUI text;
    private List<string> strList = new List<string>();

    private void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(this);
    }

    private void Start() {
        text = debugCanvas.GetComponentInChildren<TextMeshProUGUI>();
    }


#if (UNITY_IOS || UNITY_ANDROID)
	private void Update() {
		if (Input.touchCount >= 3 && Input.GetTouch(2).phase == TouchPhase.Began) {
			debugCanvas.SetActive(!debugCanvas.activeSelf);
		}
	}
#else
    private void Update() {
        if (Input.GetKey(KeyCode.Alpha1) && Input.GetKey(KeyCode.Alpha2) && Input.GetKey(KeyCode.Alpha3) && Input.GetKeyDown(KeyCode.Alpha8)) {
            debugCanvas.SetActive(!debugCanvas.activeSelf);
            PostProcManager.Instance.StopDistortion();
        }
    }
#endif

    public void Log(string str) {
        /*
        if (!GameManager.Instance.DebugMode) return;

        if (strList.Count > 10) {
            strList.RemoveAt(0);
        }

        strList.Add(str);

        string strToShow = "";

        foreach (string s in strList) {
            strToShow += s + "\n";
        }

        text.text = strToShow;
#if UNITY_EDITOR
        Debug.Log(str);
#endif
*/
    }

    public void ShowPoint(Vector3 position) {
        /*
        if (!GameManager.Instance.DebugMode) return;

        Instantiate(debugPointPrefab, position, Quaternion.identity, transform);
        Log("A point is shown at: " + position);
        */
    }
}