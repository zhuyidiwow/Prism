using UnityEngine;

public class MaterialContainer : MonoBehaviour {
	public static MaterialContainer Instance;

	private void Awake() {
		if (Instance == null) Instance = this;
	}

	public PhysicMaterial MaxFriction;
	public PhysicMaterial NoFriction;
}
