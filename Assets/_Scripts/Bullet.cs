using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float flySpeed = 5;
	public float lifetime = 2;

	public int damage = 1;

	public bool launched;

	void OnSpawned () {
		//renderer.material.color = Color.blue;

		tag = "Untagged";

		CancelInvoke();
		Invoke("Despawn", lifetime);
		Invoke("TagObject", 0.04f);
	}

	void Update () {
		if (launched)
			transform.Translate(Vector3.up * flySpeed * Time.deltaTime);
	}

	void Despawn()
	{
		PREFAB.DespawnPrefab(this.transform, "1");
	}

	void TagObject()
	{
		gameObject.tag = "Bullet";
		//renderer.material.color = Color.red;
	}

	/*void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Enemy"))
		{
			other.SendMessage("BulletDamage", SendMessageOptions.DontRequireReceiver);
			PREFAB.SpawnPrefab(PREFAB.HIT_IMPACT, transform.position - new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),2), "1");
			AudioSource.PlayClipAtPoint(PREFAB.audio.hitSound, transform.position);
			PREFAB.DespawnPrefab(this.transform, "1");
		}
	}*/
	
}
