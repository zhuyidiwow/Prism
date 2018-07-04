using UnityEngine;
using System.Collections;

public class LightManager : MonoBehaviour {
    public static LightManager Instance;

    public Material skybox;
    [Header("Day")]
    public float DayLightIntensity;
    public Color DayLightColor;
    public Color DaySkyBoxColor;
    public float DaySkyBoxExposure;
    public Color DaySkyColor;
    public Color DayEquatorColor;
    public Color DayGroundColor;
    public Color DayFogColor;
    public float DayFogIntensity;
    
    [Header("Night")]
    public float MoonLightIntensity;
    public Color MoonLightColor;
    public Color MoonSkyBoxColor;
    public float MoonSkyBoxExposure;
    public Color MoonSkyColor;
    public Color MoonEquatorColor;
    public Color MoonGroundColor;
    public Color MoonFogColor;
    public float MoonFogIntensity;
    
    private Light mainLight;

    private void Awake() {
        if (Instance == null) Instance = this;
    }

    private void Start() {
        mainLight = GetComponent<Light>();
        ChangeToNightLight(1f);
    }

    public void ChangeToDayLight(float duration = 3f) {
        GameManager.IsDay = true;
        GameManager.Instance.ShowDayScenarios();
        StartCoroutine(ChangeLightCoroutine(true, duration));
    }

    public void ChangeToNightLight(float duration = 3f) {
        GameManager.IsDay = false;
        GameManager.Instance.ShowNightScenarios();
        StartCoroutine(ChangeLightCoroutine(false, duration));
    }

    private IEnumerator ChangeLightCoroutine(bool changeToDay, float duration) {
        float startIntensity = mainLight.intensity;
        Color startColor = mainLight.color;
        Color startSkyBoxColor = skybox.GetColor("_Tint");
        float startSkyBoxExposure = skybox.GetFloat("_Exposure");
        Color startSkyColor = RenderSettings.ambientSkyColor;
        Color startEquatorColor = RenderSettings.ambientEquatorColor;
        Color StartGroundColor = RenderSettings.ambientGroundColor;
        Color startFogColor = RenderSettings.fogColor;
        float startFogIntensity = RenderSettings.fogDensity;

        float targetIntensity, targetSkyBoxExposure, targetFogIntensity;
        Color targetColor, targetSkyBoxColor, targetSkyColor, targetEquatorColor, targetGroundColor, targetFogColor;

        if (changeToDay) {
            targetIntensity = DayLightIntensity;
            targetColor = DayLightColor;
            targetSkyBoxColor = DaySkyBoxColor;
            targetSkyBoxExposure = DaySkyBoxExposure;
            targetSkyColor = DaySkyColor;
            targetEquatorColor = DayEquatorColor;
            targetGroundColor = DayGroundColor;
            targetFogIntensity = DayFogIntensity;
            targetFogColor = DayFogColor;
        } else {
            targetIntensity = MoonLightIntensity;
            targetColor = MoonLightColor;
            targetSkyBoxColor = MoonSkyBoxColor;
            targetSkyBoxExposure = MoonSkyBoxExposure;
            targetSkyColor = MoonSkyColor;
            targetEquatorColor = MoonEquatorColor;
            targetGroundColor = MoonGroundColor;
            targetFogIntensity = MoonFogIntensity;
            targetFogColor = MoonFogColor;
        }
        
        float elapsedTime = 0;
        while (elapsedTime < duration) {
            float percentage = elapsedTime / duration;
            mainLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, percentage);
            mainLight.color = Color.Lerp(startColor, targetColor, percentage);
            skybox.SetColor("_Tint", Color.Lerp(startSkyBoxColor, targetSkyBoxColor, percentage));
            skybox.SetFloat("_Exposure", Mathf.Lerp(startSkyBoxExposure, targetSkyBoxExposure, percentage));
            RenderSettings.ambientSkyColor = Color.Lerp(startSkyColor, targetSkyColor, percentage);
            RenderSettings.ambientEquatorColor = Color.Lerp(startEquatorColor, targetEquatorColor, percentage);
            RenderSettings.ambientGroundColor = Color.Lerp(StartGroundColor, targetGroundColor, percentage);
            RenderSettings.fogColor = Color.Lerp(startFogColor, targetFogColor, percentage);
            RenderSettings.fogDensity = Mathf.Lerp(startFogIntensity, targetFogIntensity, percentage);
            yield return null;
            elapsedTime += Time.deltaTime;
        }
//
//        while (true) {
//            if (changeToDay) {
//                targetIntensity = DayLightIntensity;
//                targetColor = DayLightColor;
//                targetSkyBoxColor = DaySkyBoxColor;
//                targetSkyBoxExposure = DaySkyBoxExposure;
//                targetSkyColor = DaySkyColor;
//                targetEquatorColor = DayEquatorColor;
//                targetGroundColor = DayGroundColor;
//                targetFogIntensity = DayFogIntensity;
//                targetFogColor = DayFogColor;
//            } else {
//                targetIntensity = MoonLightIntensity;
//                targetColor = MoonLightColor;
//                targetSkyBoxColor = MoonSkyBoxColor;
//                targetSkyBoxExposure = MoonSkyBoxExposure;
//                targetSkyColor = MoonSkyColor;
//                targetEquatorColor = MoonEquatorColor;
//                targetGroundColor = MoonGroundColor;
//                targetFogIntensity = MoonFogIntensity;
//                targetFogColor = MoonFogColor;
//            }
//            
//            float p = 1f;
//            mainLight.intensity = Mathf.Lerp(startIntensity, targetIntensity, p);
//            mainLight.color = Color.Lerp(startColor, targetColor, p);
//            skybox.SetColor("_Tint", Color.Lerp(startSkyBoxColor, targetSkyBoxColor, p));
//            skybox.SetFloat("_Exposure", Mathf.Lerp(startSkyBoxExposure, targetSkyBoxExposure, p));
//            RenderSettings.ambientSkyColor = Color.Lerp(startSkyColor, targetSkyColor, p);
//            RenderSettings.ambientEquatorColor = Color.Lerp(startEquatorColor, targetEquatorColor, p);
//            RenderSettings.ambientGroundColor = Color.Lerp(StartGroundColor, targetGroundColor, p);
//            RenderSettings.fogColor = Color.Lerp(startFogColor, targetFogColor, p);
//            RenderSettings.fogDensity = Mathf.Lerp(startFogIntensity, targetFogIntensity, p);
//            Debug.Log("hi");
//            yield return new WaitForSeconds(Time.deltaTime);
//        }
        
    }
}