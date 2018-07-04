using System.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;


public class PostProcManager : MonoBehaviour {
    public static PostProcManager Instance;

    // only take values from them
    public PostProcessingProfile[] Profiles;

    private int currentProfileIndex = 0;

    private PostProcessingProfile oldProfile;
    private PostProcessingProfile profile;
    private Coroutine transitCoroutine;
    
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }
    }

    void Start() {
        profile = CameraManager.Instance.GetComponent<PostProcessingBehaviour>().profile;
        StopDistortion();
    }
    
    public void RespondToOverloadFactor(float factor) {
        Lerp(Profiles[0], Profiles[1], factor);
    }

    public void StartDistortion() {
        SmoothTransitTo(Profiles[1]);
        currentProfileIndex = 1;
    }

    public void StopDistortion() {
        SmoothTransitTo(Profiles[0]);
        currentProfileIndex = 0;
    }

    private void Lerp(PostProcessingProfile origin, PostProcessingProfile target, float percentage) {
        /* this was added because of a former problem where if the night comes when the fox is howling
         this Lerp() will fight with the transitCoroutine, causing the screen to blink
         */
        if (transitCoroutine != null || currentProfileIndex == 0) return;
        
        var depthOfFieldSettings = profile.depthOfField.settings;
        var bloomSettings = profile.bloom.settings;
        var colorGradingSettings = profile.colorGrading.settings;
        var chromaticSettings = profile.chromaticAberration.settings;
        var grainSettings = profile.grain.settings;
        var vignetteSettings = profile.vignette.settings;

        var depthOfFieldSettings_old = origin.depthOfField.settings;
        var bloomSettings_old = origin.bloom.settings;
        var colorGradingSettings_old = origin.colorGrading.settings;
        var chromaticSettings_old = origin.chromaticAberration.settings;
        var grainSettings_old = origin.grain.settings;
        var vignetteSettings_old = origin.vignette.settings;

        var depthOfFieldSettings_new = target.depthOfField.settings;
        var bloomSettings_new = target.bloom.settings;
        var colorGradingSettings_new = target.colorGrading.settings;
        var chromaticSettings_new = target.chromaticAberration.settings;
        var grainSettings_new = target.grain.settings;
        var vignetteSettings_new = target.vignette.settings;

        bloomSettings.lensDirt.texture = bloomSettings_new.lensDirt.texture;
        colorGradingSettings.channelMixer.currentEditingChannel = colorGradingSettings_new.channelMixer.currentEditingChannel;

        depthOfFieldSettings.focusDistance = Mathf.Lerp(depthOfFieldSettings_old.focusDistance, depthOfFieldSettings_new.focusDistance, percentage);
        depthOfFieldSettings.aperture = Mathf.Lerp(depthOfFieldSettings_old.aperture, depthOfFieldSettings_new.aperture, percentage);
        profile.depthOfField.settings = depthOfFieldSettings;

        bloomSettings.bloom.intensity = Mathf.Lerp(bloomSettings_old.bloom.intensity, bloomSettings_new.bloom.intensity, percentage);
        bloomSettings.bloom.threshold = Mathf.Lerp(bloomSettings_old.bloom.threshold, bloomSettings_new.bloom.threshold, percentage);
        bloomSettings.bloom.radius = Mathf.Lerp(bloomSettings_old.bloom.radius, bloomSettings_new.bloom.radius, percentage);
        bloomSettings.lensDirt.intensity = Mathf.Lerp(bloomSettings_old.lensDirt.intensity, bloomSettings_new.lensDirt.intensity, percentage);
        profile.bloom.settings = bloomSettings;

        colorGradingSettings.basic.temperature =
            Mathf.Lerp(colorGradingSettings_old.basic.temperature, colorGradingSettings_new.basic.temperature, percentage);
        colorGradingSettings.basic.hueShift =
            Mathf.Lerp(colorGradingSettings_old.basic.hueShift, colorGradingSettings_new.basic.hueShift, percentage);
        colorGradingSettings.basic.saturation =
            Mathf.Lerp(colorGradingSettings_old.basic.saturation, colorGradingSettings_new.basic.saturation, percentage);
        colorGradingSettings.basic.contrast =
            Mathf.Lerp(colorGradingSettings_old.basic.contrast, colorGradingSettings_new.basic.contrast, percentage);
        colorGradingSettings.channelMixer.red =
            Vector3.Lerp(colorGradingSettings_old.channelMixer.red, colorGradingSettings_new.channelMixer.red, percentage);
        colorGradingSettings.channelMixer.green =
            Vector3.Lerp(colorGradingSettings_old.channelMixer.green, colorGradingSettings_new.channelMixer.green, percentage);
        colorGradingSettings.channelMixer.blue =
            Vector3.Lerp(colorGradingSettings_old.channelMixer.blue, colorGradingSettings_new.channelMixer.blue, percentage);
        profile.colorGrading.settings = colorGradingSettings;

        chromaticSettings.intensity = Mathf.Lerp(chromaticSettings_old.intensity, chromaticSettings_new.intensity, percentage);
        profile.chromaticAberration.settings = chromaticSettings;

        grainSettings.intensity = Mathf.Lerp(grainSettings_old.intensity, grainSettings_new.intensity, percentage);
        grainSettings.luminanceContribution =
            Mathf.Lerp(grainSettings_old.luminanceContribution, grainSettings_new.luminanceContribution, percentage);
        grainSettings.size = Mathf.Lerp(grainSettings_old.size, grainSettings_new.size, percentage);
        profile.grain.settings = grainSettings;

        vignetteSettings.intensity = Mathf.Lerp(vignetteSettings_old.intensity, vignetteSettings_new.intensity, percentage);
        vignetteSettings.smoothness = Mathf.Lerp(vignetteSettings_old.smoothness, vignetteSettings_new.smoothness, percentage);
        vignetteSettings.roundness = Mathf.Lerp(vignetteSettings_old.roundness, vignetteSettings_new.roundness, percentage);
        profile.vignette.settings = vignetteSettings;
    }

    #region smooth transit between profiles
    
    private void SmoothTransitTo(PostProcessingProfile targetProfile, float duration = 3f, float stepTime = 0.03f) {
        if (transitCoroutine != null) StopCoroutine(transitCoroutine);
        transitCoroutine = StartCoroutine(TransitProfileCoroutine(targetProfile, duration, stepTime));
    }

    private IEnumerator TransitProfileCoroutine(PostProcessingProfile newProfile, float duration, float stepTime) {
        oldProfile = profile;
        DebugUtility.Instance.Log("[Overload] Transit to " + newProfile.name + " initiated");

        var depthOfFieldSettings = profile.depthOfField.settings;
        var bloomSettings = profile.bloom.settings;
        var colorGradingSettings = profile.colorGrading.settings;
        var chromaticSettings = profile.chromaticAberration.settings;
        var grainSettings = profile.grain.settings;
        var vignetteSettings = profile.vignette.settings;
        
        var depthOfFieldSettings_old = oldProfile.depthOfField.settings;
        var bloomSettings_old = oldProfile.bloom.settings;
        var colorGradingSettings_old = oldProfile.colorGrading.settings;
        var chromaticSettings_old = oldProfile.chromaticAberration.settings;
        var grainSettings_old = oldProfile.grain.settings;
        var vignetteSettings_old = oldProfile.vignette.settings;
        
        var depthOfFieldSettings_new = newProfile.depthOfField.settings;
        var bloomSettings_new = newProfile.bloom.settings;
        var colorGradingSettings_new = newProfile.colorGrading.settings;
        var chromaticSettings_new = newProfile.chromaticAberration.settings;
        var grainSettings_new = newProfile.grain.settings;
        var vignetteSettings_new = newProfile.vignette.settings;

        bloomSettings.lensDirt.texture = bloomSettings_new.lensDirt.texture;
        colorGradingSettings.channelMixer.currentEditingChannel = colorGradingSettings_new.channelMixer.currentEditingChannel;

        if (newProfile.depthOfField.enabled) profile.depthOfField.enabled = true;
        if (newProfile.bloom.enabled) profile.bloom.enabled = true;
        if (newProfile.colorGrading.enabled) profile.colorGrading.enabled = true;
        if (newProfile.chromaticAberration.enabled) profile.chromaticAberration.enabled = true;
        if (newProfile.grain.enabled) profile.grain.enabled = true;
        if (newProfile.vignette.enabled) profile.grain.enabled = true;
        
        float startTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            elapsedTime = Time.time - startTime;
            float percentage = elapsedTime / duration;

            if (profile.depthOfField.enabled) {
                depthOfFieldSettings.focusDistance =
                    Mathf.Lerp(depthOfFieldSettings_old.focusDistance, depthOfFieldSettings_new.focusDistance, percentage);
                depthOfFieldSettings.aperture = Mathf.Lerp(depthOfFieldSettings_old.aperture, depthOfFieldSettings_new.aperture, percentage);
                profile.depthOfField.settings = depthOfFieldSettings;
            }

            if (profile.bloom.enabled) {
                bloomSettings.bloom.intensity = Mathf.Lerp(bloomSettings_old.bloom.intensity, bloomSettings_new.bloom.intensity, percentage);
                bloomSettings.bloom.threshold = Mathf.Lerp(bloomSettings_old.bloom.threshold, bloomSettings_new.bloom.threshold, percentage);
                bloomSettings.bloom.radius = Mathf.Lerp(bloomSettings_old.bloom.radius, bloomSettings_new.bloom.radius, percentage);
                bloomSettings.lensDirt.intensity = Mathf.Lerp(bloomSettings_old.lensDirt.intensity, bloomSettings_new.lensDirt.intensity, percentage);
                profile.bloom.settings = bloomSettings;
            }

            if (profile.colorGrading.enabled) {
                colorGradingSettings.basic.temperature = Mathf.Lerp(colorGradingSettings_old.basic.temperature,
                    colorGradingSettings_new.basic.temperature, percentage);
                colorGradingSettings.basic.hueShift =
                    Mathf.Lerp(colorGradingSettings_old.basic.hueShift, colorGradingSettings_new.basic.hueShift, percentage);
                colorGradingSettings.basic.saturation = Mathf.Lerp(colorGradingSettings_old.basic.saturation,
                    colorGradingSettings_new.basic.saturation, percentage);
                colorGradingSettings.basic.contrast =
                    Mathf.Lerp(colorGradingSettings_old.basic.contrast, colorGradingSettings_new.basic.contrast, percentage);
                colorGradingSettings.channelMixer.red = Vector3.Lerp(colorGradingSettings_old.channelMixer.red,
                    colorGradingSettings_new.channelMixer.red, percentage);
                colorGradingSettings.channelMixer.green = Vector3.Lerp(colorGradingSettings_old.channelMixer.green,
                    colorGradingSettings_new.channelMixer.green, percentage);
                colorGradingSettings.channelMixer.blue = Vector3.Lerp(colorGradingSettings_old.channelMixer.blue,
                    colorGradingSettings_new.channelMixer.blue, percentage);
                profile.colorGrading.settings = colorGradingSettings;
            }

            if (profile.chromaticAberration.enabled) {
                chromaticSettings.intensity = Mathf.Lerp(chromaticSettings_old.intensity, chromaticSettings_new.intensity, percentage);
                profile.chromaticAberration.settings = chromaticSettings;
            }

            if (profile.grain.enabled) {
                grainSettings.intensity = Mathf.Lerp(grainSettings_old.intensity, grainSettings_new.intensity, percentage);
                grainSettings.luminanceContribution =
                    Mathf.Lerp(grainSettings_old.luminanceContribution, grainSettings_new.luminanceContribution, percentage);
                grainSettings.size = Mathf.Lerp(grainSettings_old.size, grainSettings_new.size, percentage);
                profile.grain.settings = grainSettings;
            }

            if (profile.vignette.enabled) {
                vignetteSettings.intensity = Mathf.Lerp(vignetteSettings_old.intensity, vignetteSettings_new.intensity, percentage);
                vignetteSettings.smoothness = Mathf.Lerp(vignetteSettings_old.smoothness, vignetteSettings_new.smoothness, percentage);
                vignetteSettings.roundness = Mathf.Lerp(vignetteSettings_old.roundness, vignetteSettings_new.roundness, percentage);
                profile.vignette.settings = vignetteSettings;
            }

            yield return new WaitForSeconds(stepTime);
        }

        
        
        transitCoroutine = null;
        DebugUtility.Instance.Log("[Overload] Transited to profile " + newProfile.name);

        if (newProfile.name == "Main") {
            if (newProfile.depthOfField.enabled) profile.depthOfField.settings = depthOfFieldSettings_new;
            if (newProfile.bloom.enabled) profile.bloom.settings = bloomSettings_new;
            if (newProfile.colorGrading.enabled) profile.colorGrading.settings = colorGradingSettings_new;
            if (newProfile.chromaticAberration.enabled) profile.chromaticAberration.settings = chromaticSettings_new;
            if (newProfile.grain.enabled) profile.grain.settings = grainSettings_new;
            if (newProfile.vignette.enabled) profile.vignette.settings = vignetteSettings_new;
        }
    }
#endregion
    
}