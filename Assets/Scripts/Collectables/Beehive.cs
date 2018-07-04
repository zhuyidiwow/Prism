using UnityEngine;
using Utilities;

public class Beehive : Collectable {
    [SerializeField] private AudioClip clipBee;
    [SerializeField] private GameObject particle;
    private AudioSource audioSource;

    private void Start() {
        audioSource = GetComponent<AudioSource>();
        Audio.PlayAudio(audioSource, clipBee, 1f, true);
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.CompareTag("Player")) {
            Collect();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (Fox.Instance.Honey != null) return;
        if (GameManager.IsInteracting) return;
        if (other.CompareTag("Player")) {
            GameManager.Instance.ShowInteractHint(name);
        }
    }

    private void OnTriggerStay(Collider other) {
        if (Fox.Instance.Honey != null) return;
        if (GameManager.IsInteracting) return;
        if (other.CompareTag("Player") && InputManager.GetInteractKeyDown()) {
            Collect();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (Fox.Instance.Honey != null) return;
        if (other.CompareTag("Player")) {
            GameManager.Instance.HideInteractHint();
        }
    }

    public override void Collect() {
        if (Fox.Instance.Honey == null) {
            Fox.Instance.Honey = gameObject;

            Fox.Instance.PlayCollectItemSound();

            Transform slot = Fox.Instance.GetAvailableItemSlot();

            transform.parent = slot;
            if (slot.name.Contains("1")) {
                transform.rotation = Fox.Instance.MouthHoney.rotation;
                transform.position = Fox.Instance.MouthHoney.position;
            } else {
                transform.rotation = slot.rotation;
                transform.position = slot.position;
            }

            Audio.StopIfPlaying(audioSource);
            Destroy(particle);
            foreach (Collider component in GetComponents<Collider>()) {
                component.enabled = false;
            }

            GameManager.Instance.HideInteractHint();
            GameManager.Instance.LogEvent("Picked Up Item", "Picked up Beehive", "Triggered", 1);
        }
    }
}