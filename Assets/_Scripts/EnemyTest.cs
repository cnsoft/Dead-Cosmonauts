using UnityEngine;
using System.Collections;

[RequireComponent(typeof(uLinkNetworkView))]
public class EnemyTest : uLink.MonoBehaviour {
    public string id;
	public int health = 5;

    public void Start() {
        if (networkView == null) {
            gameObject.AddComponent<uLinkNetworkView> ();
        }

        if (networkView.observed == null) {
            networkView.observed = this;
        }
    }

    void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
        if (!info.networkView.initialData.TryRead<string> (out id)) {
            Debug.LogError ("No initial id for barricade.");
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
                if (bullet.mine) {
                    StartCoroutine (DeferredDestroy ());
                }
			}

			PREFAB.DespawnPrefab(other.transform, "1");
		}
	}

    IEnumerator DeferredDestroy() {
        yield return new WaitForEndOfFrame ();

        if (!string.IsNullOrEmpty(id)) {
            WWW www = new WWW (string.Format("{0}/powerups/pickup/{1}",Constants.METEOR_ROOT,id));
            yield return www;
        }

        uLink.Network.Destroy (this.gameObject);
    }
}
