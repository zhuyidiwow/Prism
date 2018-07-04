using UnityEngine;
using Random = UnityEngine.Random;

public class Planter : MonoBehaviour {

	    
	public enum PlantType {
		BIG_TREE, MID_TREE, SPRUCE, BIG_ROCK, FLOWER
	}

	public PlantType Type;
	public GameObject[] BigTrees;
	public GameObject[] MidTrees;
	public GameObject[] Spruce;
	public GameObject[] BigRocks;
	public GameObject[] Flowers;
	
	private Camera mainCamera;

	private void Start() {
		mainCamera = Camera.main;
	}

	private void Update() {
		if (Input.GetMouseButtonDown(0)) {
			PlantOne();	
		}
	}

	void PlantOne() {
		RaycastHit hit;
		Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 10000f, LayerMask.GetMask("Terrain"));
		GameObject o;

		switch (Type) {
			case PlantType.BIG_ROCK:
				o = Instantiate(BigRocks[Random.Range(0, BigRocks.Length)], hit.point, Quaternion.identity, GameObject.Find("Rocks").transform);
				break;
			case PlantType.BIG_TREE:
				o = Instantiate(BigTrees[Random.Range(0, BigTrees.Length)], hit.point, Quaternion.identity, GameObject.Find("Trees").transform);
				break;
			case PlantType.SPRUCE:
				o = Instantiate(Spruce[Random.Range(0, Spruce.Length)], hit.point, Quaternion.identity, GameObject.Find("Trees").transform);
				break;
			case PlantType.MID_TREE:
				o = Instantiate(MidTrees[Random.Range(0, MidTrees.Length)], hit.point, Quaternion.identity, GameObject.Find("Trees").transform);
				break;
			case PlantType.FLOWER:
				o = Instantiate(Flowers[Random.Range(0, Flowers.Length)], hit.point, Quaternion.identity, GameObject.Find("Flowers").transform);
				break;
			default:
				o = Instantiate(MidTrees[Random.Range(0, MidTrees.Length)], hit.point, Quaternion.identity, GameObject.Find("Trees").transform);
				break;
		}

		o.transform.localScale *= Random.Range(0.8f, 1.1f);
		o.transform.Rotate(Vector3.up, Random.Range(0f, 360f));
	}
}
