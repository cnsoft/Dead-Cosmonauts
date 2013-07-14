using UnityEngine;
using System.Collections;

public class Wall : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag("Bullet"))
		{
			PREFAB.SpawnPrefab(PREFAB.HIT_IMPACT, other.transform.position-new Vector3(0,0,5), "1");
			PREFAB.DespawnPrefab(other.transform, "1");
		}
	}

	void OnTriggerStay (Collider other)
	{
		if (other.CompareTag("Bullet"))
		{
			PREFAB.SpawnPrefab(PREFAB.HIT_IMPACT, other.transform.position-new Vector3(0,0,5), "1");
			PREFAB.DespawnPrefab(other.transform, "1");
		}
	}
}
