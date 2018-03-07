using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TestGameObject : NetworkBehaviour {

	// Use this for initialization
	public override void OnStartServer () {
        base.OnStartServer();
        InvokeRepeating("Call", 2.0f, 2.0f);
	}
	
	// Update is called once per frame
	void Call() {
        RpcDoStuff();
	}

    [ClientRpc]
    public void RpcDoStuff()
    {
        Debug.Log("Stuff");
    }
}
