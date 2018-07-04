using System.Collections;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FrameReporter : MonoBehaviour {

	private TextMeshProUGUI text;
	
	void Start () {
		text = GetComponent<TextMeshProUGUI>();
		StartCoroutine(ReportCoroutine());
	}


	IEnumerator ReportCoroutine() {
		while (true) {
			text.text = (1f / Time.deltaTime).ToString(CultureInfo.CurrentCulture).Substring(0, 2);
			yield return new WaitForSeconds(0.5f);
		}
		
	}
	

}
