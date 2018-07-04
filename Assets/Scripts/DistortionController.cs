using UnityEngine;

public class DistortionController : MonoBehaviour {

    public float LerpFactor;
    public Vector2 DistortionRange = new Vector2(0.5f, 0.99f);
    private AudioDistortionFilter distortionFilter;
    private float targetLevel = 0f;

    public void Initialize() {
        distortionFilter = gameObject.AddComponent<AudioDistortionFilter>();
        distortionFilter.distortionLevel = DistortionRange.x;
    }

    private void FixedUpdate() {
        if (Mathf.Abs(distortionFilter.distortionLevel - targetLevel) > 0.01f) {

            if (distortionFilter.distortionLevel > targetLevel) {
                distortionFilter.distortionLevel = targetLevel;
            } else {
                distortionFilter.distortionLevel = Mathf.Lerp(distortionFilter.distortionLevel, targetLevel, Time.fixedDeltaTime * LerpFactor);
            }
        }
    }

    public void ChangeDistortionLevel(float percentage) {
        targetLevel = Mathf.Lerp(DistortionRange.x, DistortionRange.y, percentage);
    }
    
    public void SetDistortionLevel(float level) {
        if (level < 0) return;
        targetLevel = level;
    }
}