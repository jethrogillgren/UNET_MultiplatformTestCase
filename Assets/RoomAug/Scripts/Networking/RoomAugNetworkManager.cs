using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

using RoomAug.Player;
using System.Linq;

namespace RoomAug.Networking
{
    //This is a specialised NetworkManager.
    //NetworkManager wraps the actual Link connection between Server and Client.
    //My Own NetworkController handles non UNET connection stuff
    public class RoomAugNetworkManager : NetworkManager
    {

        public RoomAugNetworkController networkController;


        //Called on client when we connect, it is modofied to also pass the NetworkType when we ask the server to Add our player
        public override void OnClientConnect(NetworkConnection conn)
        {
			Debug.Log (name + " J# OnClientConnect");
            // Create message to set the player
            if ( !RoomAugNetworkController.IsAPlayer(networkController.overrideNetworkType ) )
            {
                Debug.LogError("J# OnCLientConnect called but I have NetWorkType: " + networkController.overrideNetworkType.ToString());
                return;
            }
            IntegerMessage msg = new IntegerMessage( (int) networkController.overrideNetworkType );


            // Call Add player and pass the message
            ClientScene.AddPlayer(conn, 0, msg);
        }

        //Control adding new Players on client connection.  Runs on the server.  Modified to respect NetworkType
        //playerControllerId is the client scoped player id, for cases where a client has more than one player (eg multiple gamepads)
        public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId
                ,NetworkReader extraMessageReader)
        {
			
            // Read client message and receive index
            if (extraMessageReader == null)
            {
                Debug.LogError("J# Client did not send its NetworkType over during Discovery");
            }
            var stream = extraMessageReader.ReadMessage<IntegerMessage>();
            RoomAugNetworkController.NetworkType curPlayer = (RoomAugNetworkController.NetworkType)RoomAugNetworkController.NetworkType.Parse(typeof(RoomAugNetworkController.NetworkType), stream.value.ToString() );

            if (curPlayer == RoomAugNetworkController.NetworkType.Auto
               || curPlayer == RoomAugNetworkController.NetworkType.Server)
            {
                Debug.LogError("J# OnServerAddPlayer called but Message was from NetWorkType: " + networkController.overrideNetworkType.ToString());
                return;
            }

            for( int i =0; i< networkController.maxPlayers; i++)
            {
                if( ! networkController.connectedPlayers.ContainsKey(i) )
                { //The first available player slot

                    GameObject player = (GameObject)Instantiate(spawnPrefabs[((int)stream.value)], Vector3.zero, Quaternion.identity);
                    RoomAugPlayerController p = player.GetComponent<RoomAugPlayerController>();
                    p.setName("Player " + i);
                    p.id = i;

                    Debug.Log(name + " J# Adding player with id " + p.id + " for connection as a " + curPlayer + " from " + conn.address );

                    if( NetworkServer.AddPlayerForConnection(conn, player, playerControllerId) )
                    {
                        networkController.connectedPlayers.Add(i, conn);
                        return;
                    } else
                    {
                        Debug.LogError(name + " was unable to AddPlayerForConnection " + conn + " " + player + " " +playerControllerId );
                        //We don't return, but let the loop continue to retry the AddPlayer code
                    }
                }
            }

            Debug.LogError(name + " There are no player slots left, maxPlayers=" + networkController.maxPlayers + "  Not adding new " + curPlayer.ToString() );
             
        }

        //Called when the app closes without asking to disconnecting first
        public override void OnServerDisconnect(NetworkConnection conn)
        {
            Debug.Log(name + " OnServerDisconnect Called" );
            base.OnServerDisconnect(conn);

            RemovePlayer(conn);
        }

        //Called when app specifically asks to disconnect.  Untested wether OnServerDIsconnect is also called.
        public override void OnServerRemovePlayer(NetworkConnection conn, PlayerController player)
        {
            Debug.Log(name + " OnServerRemovePlayer Called");
            base.OnServerRemovePlayer(conn, player);

            RemovePlayer(conn);
        }

        protected void RemovePlayer(NetworkConnection conn)
        {
            try
            {
                var item = networkController.connectedPlayers.First(kvp => kvp.Value == conn);
                Debug.Log(name + " Remiving connected player " + item.Key);
                networkController.connectedPlayers.Remove(item.Key);
            } catch (System.InvalidOperationException)
            {
                Debug.LogError(name + " asked to RemovePlayer but was unable to find an entry for NetworkConnection: " + conn.address );
            }

        }


        //	bool _isSyncTimeWithServer = false;
        //	public static double syncServerTime;
        //	public static double latencyMs = 0;
        //
        //	void Update()
        //	{
        //		if (_isSyncTimeWithServer)
        //		{
        //			syncServerTime += Time.deltaTime;
        //		}
        //	}

        //	public override void OnStartServer ()
        //	{
        //		base.OnStartServer();
        //		// we're the Server, dont need to sync with anyone :)
        //		_isSyncTimeWithServer = true;
        //		syncServerTime = Network.time;
        //	}
        //
        //	/// <summary>
        //	/// On server, be called when a client connected to Server
        //	/// </summary>
        //	public override void OnServerConnect (NetworkConnection conn)
        //	{
        //		base.OnServerConnect(conn);
        //		Debug.Log("---- Server send syncTime to client : " + conn.connectionId);
        //
        //		var syncTimeMessage = new SyncTimeMessage();
        //		syncTimeMessage.timeStamp = Network.time;
        //		NetworkServer.SendToClient(conn.connectionId, CustomMsgType.SyncTime, syncTimeMessage);
        //	}
        //
        //
        ////	//Add in Client Unique Scene assets for clients only.
        ////	public override void OnClientSceneChanged(NetworkConnection conn)
        ////	{
        ////		if (Application.platform == RuntimePlatform.Android) {
        ////
        ////			ClientScene.Ready (conn);
        ////			ClientScene.AddPlayer (conn, 0);
        ////
        ////		} else {
        ////			Debug.LogError("Tried to load the ClientScene from a Non-Android Device.");
        ////		}
        ////	}
        //
        //	public override void OnStartClient (NetworkClient client)
        //	{
        //		base.OnStartClient(client);
        //		client.RegisterHandler(CustomMsgType.SyncTime, OnReceiveSyncTime);
        //		latencyMs = client.GetRTT ();
        //	}
        //
        //
        //	void OnReceiveSyncTime(NetworkMessage msg)
        //	{
        //		var castMsg = msg.ReadMessage<SyncTimeMessage>();
        //		_isSyncTimeWithServer = true;
        //		syncServerTime = castMsg.timeStamp + latencyMs;
        //		Debug.Log("--------Client receive : " + syncServerTime);
        //	}
        //
        //
        //
        //
        //
        //	public class CustomMsgType
        //	{
        //		public const short SyncTime = MsgType.Highest + 1;
        //	}
        //
        //
        //	public class SyncTimeMessage : MessageBase
        //	{
        //		public double timeStamp;
        //	}
    }
}