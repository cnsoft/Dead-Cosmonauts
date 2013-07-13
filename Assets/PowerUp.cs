using UnityEngine;
using System.Collections;

public class PowerUp : MonoBehaviour {

	private Player player;

	void Awake()
	{
		player = (Player)GameObject.FindObjectOfType(typeof(Player));
	}

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			player.ChangeWeapon(2);
			gameObject.SetActive(false);
		}
	}
}
