using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Extensions;

[RequireComponent(typeof(uLinkSimpleServer))]
public class ServerManager : uLink.MonoBehaviour {
    public uLinkSimpleServer simpleServer;
	public List<Player> players = new List<Player> ();
    public Transform powerupPrefab;
    public string meteorRoot = "http://cosmo.meteor.com";

    public float updateFrequency = 0.1f;
    private float updateTimer = 0f;

    void Awake() {
        if (simpleServer == null) {
            simpleServer = gameObject.GetComponent<uLinkSimpleServer> ();
        }
    }

    // Use this for initialization
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
        updateTimer -= Time.deltaTime;
        if (updateTimer<0f) {
            StartCoroutine (PowerupsUpdate ());

            updateTimer = updateFrequency;
        }
	}

    void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player)
    {
        if (simpleServer.cleanupAfterPlayers)
        {
            StartCoroutine (RemoveMeFromMeteor (player));
        }
    }

    IEnumerator RemoveMeFromMeteor(uLink.NetworkPlayer player) {
        WWW www = new WWW (string.Format ("{0}/delete/{1}", meteorRoot, player.id));
        yield return www;
    }

    IEnumerator PowerupsUpdate() {
        WWW www = new WWW (string.Format ("{0}/powerups", meteorRoot));
        yield return www;

        if (!string.IsNullOrEmpty(www.error)) {
            Debug.LogError (www.error);
            yield return null;
        }
        Debug.Log (www.text);
        PowerupsResponse response = www.text.Deserialize<PowerupsResponse> ();

        for (int i = 0; i < response.data.Count; i++) {
            PowerupResponseDto powerup = response.data [i];

            Transform instance = uLink.Network.Instantiate (powerupPrefab, new Vector3 (powerup.x, powerup.y, 0f), Quaternion.identity, 0);
            instance.GetComponent<PowerUp> ().id = powerup._id;
            Debug.Log ("powerup instantiated");
        }
    }

    void UpdatePlayers() {

    }
}
