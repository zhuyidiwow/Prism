using UnityEngine;
using Utilities;

public class NonInteractiveAnimal : AAnimal {
    private void OnEnable() {
        if (sourceRun == null) sourceRun = GetComponent<AudioSource>();
        Initialize();
    }

    public void PlayRunSound() {
        Audio.PlayAudio(sourceRun, clipRun, 0.6f, true);
    }

    public void StopRunSound() {
        Audio.StopIfPlaying(sourceRun);
    }
    
    protected override void OnTriggerEnter(Collider other) {
        return;
    }

    protected override void OnTriggerStay(Collider other) {
        return;
    }

    protected override void OnTriggerExit(Collider other) {
        return;
    }

    protected override void Interact() {
        return;
    }
    
    
}