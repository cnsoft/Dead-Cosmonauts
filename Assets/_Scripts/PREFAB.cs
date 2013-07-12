using UnityEngine;
using System.Collections;

public class PREFAB : MonoBehaviour {

	public static GameObject		BULLET;
	public static GameObject		DAMAGE_TEXT;
	
	void Awake()
	{
		BULLET = (GameObject)Resources.Load("Bullet");
		DAMAGE_TEXT = (GameObject)Resources.Load("DamageText");
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
