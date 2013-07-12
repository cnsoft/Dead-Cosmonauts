using UnityEngine;
using System.Collections;

public class GameManager : MonoSingleton<GameManager>, NetworkManager.INetworkEventHandler
{
    public NetworkManager networkManager;
	// Use this for initialization
    protected override void Awake()
    {
        base.Awake();
        if (networkManager == null)
        {
            networkManager = gameObject.AddComponent<NetworkManager>();
            networkManager.networkEventHandler = this;
        }
    }
	void Start () {
	    
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnThisPlayerConnected()
    {
        throw new System.NotImplementedException();
    }

    public void OnOtherPlayerConnected(NetworkPlayer player)
    {
        throw new System.NotImplementedException();
    }

    public NetworkManager.IPlayer CreateGameObjectForPlayer(NetworkPlayer player)
    {
        throw new System.NotImplementedException();
    }
}
