using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	public float flySpeed = 5;
	public float lifetime = 2;

	void OnSpawned () {
		CancelInvoke();
		Invoke("Despawn", lifetime);
	}

	void Update () {
		transform.Translate(Vector3.up * flySpeed * Time.deltaTime);
	}

	void Despawn()
	{
		PREFAB.DespawnPrefab(this.transform, "1");
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Enemy"))
		{
			other.SendMessage("BulletDamage", SendMessageOptions.DontRequireReceiver);
			print (other.name);
			PREFAB.DespawnPrefab(this.transform, "1");
		}
	}
}
