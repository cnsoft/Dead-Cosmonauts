using UnityEngine;
using System.Collections;

[RequireComponent(typeof(uLinkNetworkView))]
[RequireComponent(typeof(BoxCollider))]
public class PowerUp : uLink.MonoBehaviour {

	private Player player;

	public int weaponId = 2;

	/*void Awake(uLink.NetworkMessageInfo info) {
	{
		player = (Player)GameObject.FindObjectOfType(typeof(Player));
	}*/

    void Start() {
        if (networkView == null) {
            gameObject.AddComponent<uLinkNetworkView> ();
        }
        if (networkView.observed == null) {
            networkView.observed = this;
        }
    }

	void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			player = other.gameObject.GetComponent<Player>();

            if (player.networkView.isOwner) {
                player.ChangeWeapon(weaponId);
                TextPopup txtPop = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, other.transform.position-new Vector3(0,0,5), "1").GetComponent<TextPopup>();

                switch (weaponId)
                {
                    case(1): txtPop.ChangeText("FAST MG", Color.yellow); break;
                    case(2): txtPop.ChangeText("SHOTGUN", Color.yellow); break;
                }

                networkView.RPC ("Take", uLink.RPCMode.All);
            }
		}
	}

    [RPC]
    public void Take() {
        gameObject.SetActive (false);
    }
}
