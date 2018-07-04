using UnityEngine;
using UnityEngine.Events;


public class IntroTrigger : MonoBehaviour {
    public UnityEvent Event;

    private bool isTriggered = false;

    private void OnTriggerExit(Collider other) {
        if (isTriggered || GameManager.IsIntroShown) return;

        if (other.CompareTag("Player")) {
            isTriggered = true;
            Event.Invoke();
        }
    }
}

