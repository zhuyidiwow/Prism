using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flower : MonoBehaviour {

	private void ShowInteractHint() {
		GameManager.Instance.ShowInteractHint(name);
	}

	private void HideInteractHint() {
		GameManager.Instance.HideInteractHint();
	}

	private bool CanInteract() {
		if (GameManager.IsInteracting) {
			return false;
		}

		return true;
	}

	private void Interact() {
		Fox.Instance.StopMoving();
		Fox.Instance.SmoothLookAt(transform.position);
		GameManager.IsInteracting = true;
		CameraManager.Instance.SmoothLookAt(transform.position, Fox.Instance.transform.position);
		HideInteractHint();
		DialogueManager.Instance.StartDialogue("flowerWon", EndInteraction, false);
	}
	
	private void EndInteraction() {
		GameManager.IsInteracting = false;
		Fox.Instance.RegainControl();
		CameraManager.Instance.ContinueFollowingObject();
	}
	
	private void OnTriggerEnter(Collider other) {
		if (!CanInteract()) return;
		if (other.CompareTag("Player")) {
			ShowInteractHint();
		}
	}

	private void OnTriggerStay(Collider other) {
		if (!CanInteract()) return;
		if (other.CompareTag("Player") && InputManager.GetInteractKeyDown()) {
			Interact();
		}
	}

	private void OnTriggerExit(Collider other) {
		if (other.CompareTag("Player")) {
			HideInteractHint();
		}
	}
}
