using System.Collections;
using UnityEngine;
using UnityEngine.PostProcessing;


public class PostProcManagerUtility : MonoBehaviour {

    // only take values from them
    public PostProcessingProfile[] Profiles;

    private int currentProfileIndex = 0;

    private PostProcessingProfile oldProfile;
    private PostProcessingProfile profile;
    private Coroutine transitCoroutine;

    void Start() {
        profile = CameraManager.Instance.GetComponent<PostProcessingBehaviour>().profile;
    }

    private void Lerp(PostProcessingProfile origin, PostProcessingProfile target, float percentage) {
        if (transitCoroutine != null) return;
        
        var aoSettings = profile.ambientOcclusion.settings;
        var depthOfFieldSettings = profile.depthOfField.settings;
        var motionBlurSettings = profile.motionBlur.settings;
        var bloomSettings = profile.bloom.settings;
        var colorGradingSettings = profile.colorGrading.settings;
        var chromaticSettings = profile.chromaticAberration.settings;
        var grainSettings = profile.grain.settings;
        var vignetteSettings = profile.vignette.settings;

        var aoSettings_old = origin.ambientOcclusion.settings;
        var depthOfFieldSettings_old = origin.depthOfField.settings;
        var motionBlurSettings_old = origin.motionBlur.settings;
        var bloomSettings_old = origin.bloom.settings;
        var colorGradingSettings_old = origin.colorGrading.settings;
        var chromaticSettings_old = origin.chromaticAberration.settings;
        var grainSettings_old = origin.grain.settings;
        var vignetteSettings_old = origin.vignette.settings;

        var aoSettings_new = target.ambientOcclusion.settings;
        var depthOfFieldSettings_new = target.depthOfField.settings;
        var motionBlurSettings_new = target.motionBlur.settings;
        var bloomSettings_new = target.bloom.settings;
        var colorGradingSettings_new = target.colorGrading.settings;
        var chromaticSettings_new = target.chromaticAberration.settings;
        var grainSettings_new = target.grain.settings;
        var vignetteSettings_new = target.vignette.settings;

        bloomSettings.lensDirt.texture = bloomSettings_new.lensDirt.texture;
        colorGradingSettings.channelMixer.currentEditingChannel = colorGradingSettings_new.channelMixer.currentEditingChannel;

        aoSettings.intensity = Mathf.Lerp(aoSettings_old.intensity, aoSettings_new.intensity, percentage);
        aoSettings.radius = Mathf.Lerp(aoSettings_old.radius, aoSettings_new.radius, percentage);
        profile.ambientOcclusion.settings = aoSettings;

        depthOfFieldSettings.focusDistance = Mathf.Lerp(depthOfFieldSettings_old.focusDistance, depthOfFieldSettings_new.focusDistance, percentage);
        depthOfFieldSettings.aperture = Mathf.Lerp(depthOfFieldSettings_old.aperture, depthOfFieldSettings_new.aperture, percentage);
        profile.depthOfField.settings = depthOfFieldSettings;

        motionBlurSettings.shutterAngle = Mathf.Lerp(motionBlurSettings_old.shutterAngle, motionBlurSettings_new.shutterAngle, percentage);
        motionBlurSettings.frameBlending = Mathf.Lerp(motionBlurSettings_old.frameBlending, motionBlurSettings_new.frameBlending, percentage);
        profile.motionBlur.settings = motionBlurSettings;

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

        var aoSettings = profile.ambientOcclusion.settings;
        var depthOfFieldSettings = profile.depthOfField.settings;
        var motionBlurSettings = profile.motionBlur.settings;
        var bloomSettings = profile.bloom.settings;
        var colorGradingSettings = profile.colorGrading.settings;
        var chromaticSettings = profile.chromaticAberration.settings;
        var grainSettings = profile.grain.settings;
        var vignetteSettings = profile.vignette.settings;
        
        var aoSettings_old = oldProfile.ambientOcclusion.settings;
        var depthOfFieldSettings_old = oldProfile.depthOfField.settings;
        var motionBlurSettings_old = oldProfile.motionBlur.settings;
        var bloomSettings_old = oldProfile.bloom.settings;
        var colorGradingSettings_old = oldProfile.colorGrading.settings;
        var chromaticSettings_old = oldProfile.chromaticAberration.settings;
        var grainSettings_old = oldProfile.grain.settings;
        var vignetteSettings_old = oldProfile.vignette.settings;
        
        var aoSettings_new = newProfile.ambientOcclusion.settings;
        var depthOfFieldSettings_new = newProfile.depthOfField.settings;
        var motionBlurSettings_new = newProfile.motionBlur.settings;
        var bloomSettings_new = newProfile.bloom.settings;
        var colorGradingSettings_new = newProfile.colorGrading.settings;
        var chromaticSettings_new = newProfile.chromaticAberration.settings;
        var grainSettings_new = newProfile.grain.settings;
        var vignetteSettings_new = newProfile.vignette.settings;

        bloomSettings.lensDirt.texture = bloomSettings_new.lensDirt.texture;
        colorGradingSettings.channelMixer.currentEditingChannel = colorGradingSettings_new.channelMixer.currentEditingChannel;

        float startTime = Time.time;
        float elapsedTime = 0f;
        while (elapsedTime < duration) {
            elapsedTime = Time.time - startTime;
            float percentage = elapsedTime / duration;
            
            aoSettings.intensity = Mathf.Lerp(aoSettings_old.intensity, aoSettings_new.intensity, percentage);
            aoSettings.radius = Mathf.Lerp(aoSettings_old.radius, aoSettings_new.radius, percentage);
            profile.ambientOcclusion.settings = aoSettings;

            depthOfFieldSettings.focusDistance = Mathf.Lerp(depthOfFieldSettings_old.focusDistance, depthOfFieldSettings_new.focusDistance, percentage);
            depthOfFieldSettings.aperture = Mathf.Lerp(depthOfFieldSettings_old.aperture, depthOfFieldSettings_new.aperture, percentage);
            profile.depthOfField.settings = depthOfFieldSettings;

            motionBlurSettings.shutterAngle = Mathf.Lerp(motionBlurSettings_old.shutterAngle, motionBlurSettings_new.shutterAngle, percentage);
            motionBlurSettings.frameBlending = Mathf.Lerp(motionBlurSettings_old.frameBlending, motionBlurSettings_new.frameBlending, percentage);
            profile.motionBlur.settings = motionBlurSettings;
            
            bloomSettings.bloom.intensity = Mathf.Lerp(bloomSettings_old.bloom.intensity, bloomSettings_new.bloom.intensity, percentage);
            bloomSettings.bloom.threshold = Mathf.Lerp(bloomSettings_old.bloom.threshold, bloomSettings_new.bloom.threshold, percentage);
            bloomSettings.bloom.radius = Mathf.Lerp(bloomSettings_old.bloom.radius, bloomSettings_new.bloom.radius, percentage);
            bloomSettings.lensDirt.intensity = Mathf.Lerp(bloomSettings_old.lensDirt.intensity, bloomSettings_new.lensDirt.intensity, percentage);
            profile.bloom.settings = bloomSettings;
            
            colorGradingSettings.basic.temperature = Mathf.Lerp(colorGradingSettings_old.basic.temperature, colorGradingSettings_new.basic.temperature, percentage);
            colorGradingSettings.basic.hueShift = Mathf.Lerp(colorGradingSettings_old.basic.hueShift, colorGradingSettings_new.basic.hueShift, percentage);
            colorGradingSettings.basic.saturation = Mathf.Lerp(colorGradingSettings_old.basic.saturation, colorGradingSettings_new.basic.saturation, percentage);
            colorGradingSettings.basic.contrast = Mathf.Lerp(colorGradingSettings_old.basic.contrast, colorGradingSettings_new.basic.contrast, percentage);
            colorGradingSettings.channelMixer.red = Vector3.Lerp(colorGradingSettings_old.channelMixer.red, colorGradingSettings_new.channelMixer.red, percentage);
            colorGradingSettings.channelMixer.green = Vector3.Lerp(colorGradingSettings_old.channelMixer.green, colorGradingSettings_new.channelMixer.green, percentage);
            colorGradingSettings.channelMixer.blue = Vector3.Lerp(colorGradingSettings_old.channelMixer.blue, colorGradingSettings_new.channelMixer.blue, percentage);
            profile.colorGrading.settings = colorGradingSettings;

            chromaticSettings.intensity = Mathf.Lerp(chromaticSettings_old.intensity, chromaticSettings_new.intensity, percentage);
            profile.chromaticAberration.settings = chromaticSettings;

            grainSettings.intensity = Mathf.Lerp(grainSettings_old.intensity, grainSettings_new.intensity, percentage);
            grainSettings.luminanceContribution = Mathf.Lerp(grainSettings_old.luminanceContribution, grainSettings_new.luminanceContribution, percentage);
            grainSettings.size = Mathf.Lerp(grainSettings_old.size, grainSettings_new.size, percentage);
            profile.grain.settings = grainSettings;

            vignetteSettings.intensity = Mathf.Lerp(vignetteSettings_old.intensity, vignetteSettings_new.intensity, percentage);
            vignetteSettings.smoothness = Mathf.Lerp(vignetteSettings_old.smoothness, vignetteSettings_new.smoothness, percentage);
            vignetteSettings.roundness = Mathf.Lerp(vignetteSettings_old.roundness, vignetteSettings_new.roundness, percentage);
            profile.vignette.settings = vignetteSettings;
            
            yield return new WaitForSeconds(stepTime);
        }

        profile.depthOfField.settings = depthOfFieldSettings_new;
        profile.bloom.settings = bloomSettings_new;
        profile.colorGrading.settings = colorGradingSettings_new;
        profile.chromaticAberration.settings = chromaticSettings_new;
        profile.grain.settings = grainSettings_new;
        profile.vignette.settings = vignetteSettings_new;
        
        transitCoroutine = null;
    }
#endregion
    
}