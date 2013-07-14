using UnityEngine;
using System.Collections;

[RequireComponent(typeof(uLinkNetworkView))]
[RequireComponent(typeof(BoxCollider))]
public class PowerUp : uLink.MonoBehaviour {
    public const string meteorRoot = "http://cosmo.meteor.com";
    public string id;
	private Player player;
	private Light2D light;


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
                TextPopup txtPop = PREFAB.SpawnPrefab(PREFAB.DAMAGE_TEXT, other.transform.position-new Vector3(0,0,5), "1").GetComponent<TextPopup>();

                // is this a healthpack?
                if (weaponId == 4) {
                    player.Healthpack ();

                    txtPop.ChangeText ("HEALTH", Color.red);
                } else {
                    player.ChangeWeapon(weaponId);

                    switch (weaponId)
                    {
                        case(1): txtPop.ChangeText("FAST MG", Color.yellow); break;
                        case(2): txtPop.ChangeText("SHOTGUN", Color.yellow); break;
                    }
                }

                StartCoroutine (DeferredDestroy ());

				AudioSource.PlayClipAtPoint( PREFAB.audio.collectSound, transform.position);
            }
		}
	}

    void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info) {
        if (!info.networkView.initialData.TryRead<string>(out id)) {
            Debug.LogError ("No initial id for powerup.");
        }
        if (!info.networkView.initialData.TryRead<int>(out weaponId)) {
            Debug.LogError ("No initial id for powerup.");
        }
    }

    IEnumerator DeferredDestroy() {
        yield return new WaitForEndOfFrame ();

        if (!string.IsNullOrEmpty(id)) {
            WWW www = new WWW (string.Format("{0}/powerups/pickup/{1}",meteorRoot,id));
            yield return www;
        }

        uLink.Network.Destroy (this.gameObject);
    }
}
