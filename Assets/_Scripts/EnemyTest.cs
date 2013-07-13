using UnityEngine;
using System.Collections;

public class EnemyTest : MonoBehaviour {

	public int health = 5;

	void BulletDamage()
	{
		PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, transform.position+new Vector3(Random.Range(-0.75f, 0.75f), Random.Range(0f, 0.5f),-2), "1");

		health -= 1;

		if (health > 0){
			StartCoroutine(Blink());
		}
		else
		{
			PREFAB.SpawnPrefab(PREFAB.EXPLOSION, transform.position, "1");
			AudioSource.PlayClipAtPoint(PREFAB.audio.explosionSound, transform.position);
			this.gameObject.SetActive(false);
		}
	}

	IEnumerator Blink()
	{
		renderer.material.color = Color.red;
		yield return new WaitForSeconds(0.05f);
		renderer.material.color = Color.white;
	}

	void OnTriggerEnter (Collider other)
	{
		if (other.CompareTag("Bullet"))
		{
			Bullet bullet = other.GetComponent<Bullet>();
			TextPopup txtPop = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, transform.position+new Vector3(Random.Range(-0.80f, 0.8f), Random.Range(-0.30f, 0.50f),-5), "1").GetComponent<TextPopup>();
			int damage = bullet.damage;
			txtPop.ChangeText(damage.ToString("f0"));

			health -= damage;

			PREFAB.SpawnPrefab(PREFAB.HIT_IMPACT, other.transform.position - new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f),5), "1");

			if (health > 0){
				StartCoroutine(Blink());
				AudioSource.PlayClipAtPoint(PREFAB.audio.hitSound, transform.position);
			}
			else
			{
				PREFAB.SpawnPrefab(PREFAB.EXPLOSION, transform.position, "1");
				AudioSource.PlayClipAtPoint(PREFAB.audio.explosionSound, transform.position);
				this.gameObject.SetActive(false);
			}

			PREFAB.DespawnPrefab(other.transform, "1");

		}
	}
}
