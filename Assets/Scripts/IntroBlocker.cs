using UnityEngine;
using UnityEngine.Events;

public class IntroBlocker : MonoBehaviour {
	
	void Start() {
		GameManager.Instance.IntroScenario.GetComponent<IntroCutscene>().OnIntroShown += OnIntroShown;
	}
	
	private void OnIntroShown() {
		Destroy(gameObject);
	}
	
	private void OnCollisionEnter (Collision other) {
		if (other.gameObject.CompareTag("Player")) {
			GameManager.Instance.Wolf.GetComponent<Wolf>().StopPlayer();
			transform.localScale *= 1.04f;
		}
	}
}
