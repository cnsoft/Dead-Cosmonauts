using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(uLinkSimpleServer))]
public class ServerManager : uLink.MonoBehaviour {
    public uLinkSimpleServer simpleServer;
	public List<Player> players = new List<Player> ();

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

        }
    }

    void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
    {
        
    }

    void UpdatePlayers() {

    }
}
