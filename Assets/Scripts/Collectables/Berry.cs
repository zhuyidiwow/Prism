using UnityEngine;

public class Berry : Collectable {
    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player")) {
            Collect();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (Fox.Instance.Berry != null) return;
        if (GameManager.IsInteracting) return;

        if (other.CompareTag("Player")) {
            GameManager.Instance.ShowInteractHint(name);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (GameManager.IsInteracting) return;
        if (Fox.Instance.Berry != null) return;
        if (other.CompareTag("Player") && InputManager.GetInteractKeyDown()) {
            Collect();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (Fox.Instance.Berry != null) return;
        if (other.CompareTag("Player")) {
            GameManager.Instance.HideInteractHint();
        }
    }

    public override void Collect() {
        Fox fox = Fox.Instance;
        if (fox.Berry == null) {
            fox.Berry = gameObject;
            fox.PlayCollectItemSound();
            Transform slot = fox.GetAvailableItemSlot();

            transform.parent = slot;
            if (slot.name.Contains("1")) {
                transform.rotation = fox.MouthBerry.rotation;
                transform.position = fox.MouthBerry.position;
            } else {
                transform.rotation = slot.rotation;
                transform.position = slot.position;
            }

            foreach (Collider component in GetComponents<Collider>()) {
                component.enabled = false;
            }

            GameManager.Instance.HideInteractHint();
            GameManager.Instance.LogEvent("Picked Up Item", "Picked Up Berry", "Frequency", 1);
        }
    }
}