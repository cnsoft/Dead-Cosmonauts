using UnityEngine;
using System.Collections;

public class Despawn : MonoBehaviour {

	public string pool = "1";
	public float lifetime = 1;

	tk2dSpriteAnimator animator;

	// Use this for initialization
	void OnSpawned () {
		CancelInvoke();
		Invoke("TimedDespawn", lifetime);

		if (animator == null)
			animator = GetComponent<tk2dSpriteAnimator>();

		animator.Stop();
		animator.Play();
	}

	void TimedDespawn()
	{
		PREFAB.DespawnPrefab(this.transform, pool);
	}
}
