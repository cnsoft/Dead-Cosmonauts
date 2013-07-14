using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(uLinkSimpleServer))]
public class ServerManager : uLink.MonoBehaviour {
    public uLinkSimpleServer simpleServer;
	public List<Player> players = new List<Player> ();
    public string meteorRoot = "http://cosmo.meteor.com";

    public float updateFrequency = 0.1f;

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

    void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
    {
        
    }

    void UpdatePlayers() {

    }
}
