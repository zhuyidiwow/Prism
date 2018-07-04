using UnityEngine;

public class Highlighter : MonoBehaviour {
    public float LerpFactor;
    
    [SerializeField]private Vector3 targetPosition;

    private RectTransform rectTransform;

    private void Start() {
        rectTransform = GetComponent<RectTransform>();
    }

    void Update() {
        rectTransform.position = Vector3.Lerp(rectTransform.position, targetPosition, LerpFactor * Time.deltaTime);
    }

    public void SetPosition(Vector3 newPos) {
        targetPosition = newPos;
    }
}