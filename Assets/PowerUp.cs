using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour {

	private Player player;

	public int weaponId = 2;

	/*void Awake(uLink.NetworkMessageInfo info) {
	{
		player = (Player)GameObject.FindObjectOfType(typeof(Player));
	}*/

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			player = (Player)GameObject.FindObjectOfType(typeof(Player));
		
			player.ChangeWeapon(weaponId);
			TextPopup txtPop = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, other.transform.position-new Vector3(0,0,5), "1").GetComponent<TextPopup>();

			switch (weaponId)
			{
				case(1): txtPop.ChangeText("FAST MG", Color.yellow); break;
				case(2): txtPop.ChangeText("SHOTGUN", Color.yellow); break;
			}

			gameObject.SetActive(false);
		}
	}
}
