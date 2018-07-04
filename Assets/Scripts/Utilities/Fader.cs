using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class Fader : MonoBehaviour {
    public float FadeDuration;

    private Coroutine fadeCoroutine;

    List<TextMeshProUGUI> texts = new List<TextMeshProUGUI>();
    List<Image> images = new List<Image>();

    private void OnEnable() {
        texts.Clear();
        images.Clear();
        
        foreach (TextMeshProUGUI ugui in GetComponentsInChildren<TextMeshProUGUI>()) {
            texts.Add(ugui);
        }

        foreach (Image image in GetComponentsInChildren<Image>()) {
            images.Add(image);
        }
    }

    public void FadeIn(float shownAlpha = 1f) {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(true, shownAlpha));
    }

    public void FadeOut() {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCoroutine(false));
    }

    private IEnumerator FadeCoroutine(bool fadeIn, float shownAlpha = 1f) {
        float elapsedTime = 0f;
        float a = fadeIn ? shownAlpha : 0f;
        while (elapsedTime < FadeDuration) {
            foreach (var text in texts) {
                Color targetColor = text.color;
                targetColor.a = a;
                text.color = Color.Lerp(text.color, targetColor, elapsedTime / FadeDuration);
            }
            
            foreach (var image in images) {
                Color targetColor = image.color;
                targetColor.a = a;
                image.color = Color.Lerp(image.color, targetColor, elapsedTime / FadeDuration);
            }

            yield return null;
            elapsedTime += Time.deltaTime;
        }

        fadeCoroutine = null;
        gameObject.SetActive(fadeIn);
    }
}