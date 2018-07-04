using UnityEngine;
using UnityEngine.Events;

public class Trigger : MonoBehaviour {
	public enum ETrigger {
		ENTER, EXIT
	}

	public UnityEvent Event;
	public ETrigger Type = ETrigger.ENTER;
	public bool Repetitive;

	private bool isTriggered = false;

	private void OnTriggerEnter(Collider other) {
		if (Type != ETrigger.ENTER) return;
		if (!Repetitive && isTriggered) return;

		if (other.CompareTag("Player")) {
			isTriggered = true;
			Event.Invoke();
		}
	}

	private void OnTriggerExit(Collider other) {
		if (Type != ETrigger.EXIT) return;
		if (!Repetitive && isTriggered) return;

		if (other.CompareTag("Player") && !GameManager.IsIntroShown) {
			isTriggered = true;
			Event.Invoke();
		}
	}
}
