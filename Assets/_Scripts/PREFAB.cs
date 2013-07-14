using UnityEngine;
using System.Collections;

public class PREFAB : MonoBehaviour {

	public static GameObject		BULLET;
	public static GameObject		BULLET_SHOTGUN;
	public static GameObject		DAMAGE_TEXT;
	public static GameObject		EXPLOSION;
	public static GameObject		EXPLOSION2;
	public static GameObject		HIT_IMPACT;

	public static AudioClip			AUDIO_HIT;

	public static AUDIO audio;
	
	void Awake()
	{
		BULLET = (GameObject)Resources.Load("Bullet");
		DAMAGE_TEXT = (GameObject)Resources.Load("DamageText");
		EXPLOSION = (GameObject)Resources.Load("Explosion");
		EXPLOSION2 = (GameObject)Resources.Load("Explosion2");
		HIT_IMPACT = (GameObject)Resources.Load("HitImpact");
		BULLET_SHOTGUN = (GameObject)Resources.Load("Bullet_Shotgun");

		audio = GetComponent<AUDIO>();
	}
	
	public static Transform SpawnPrefab (GameObject obj, Vector3 pos, string pool)
	{
		Transform instance = PoolManager.Pools[pool].Spawn(obj.transform, pos, Quaternion.identity);
		instance.eulerAngles = Vector3.zero;
		
		return instance;
	}

	public static Transform SpawnPrefab (GameObject obj, Vector3 pos, Vector3 rot, string pool)
	{
		Transform instance = PoolManager.Pools[pool].Spawn(obj.transform, pos, Quaternion.identity);
		instance.eulerAngles = rot;
		
		return instance;
	}
	
	public static void DespawnPrefab (Transform trans, string pool)
	{
		PoolManager.Pools[pool].Despawn(trans);
	}
}
