using UnityEngine;

public class Scalar : MonoBehaviour {

	[SerializeField] private AnimationCurve curve;

	private float elapsedTime;

	private void OnEnable() {
		elapsedTime = 0f;
	}

	private void Update() {
		elapsedTime += Time.deltaTime;
		if (elapsedTime > curve.keys[curve.keys.Length - 1].time) {
			elapsedTime = 0f;
		}

		transform.localScale = Vector3.one * curve.Evaluate(elapsedTime);
	}
}
