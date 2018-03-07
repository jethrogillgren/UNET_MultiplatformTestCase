using UnityEngine;
using UnityEngine.Networking;



namespace RoomAug.Player
{
    public class RoomAugPlayerController : NetworkBehaviour
    {
        [SyncVar]
        public new string name = "auto"; //Should be set during construction
        public void setName(string name) //Server sets this
        {
            this.name = name;
        }

        [SyncVar(hook = "OnSYncVar")]
        public string test;

        public void OnSYncVar(string val)
        {
            Debug.Log("Syncvar Hook");
            test = val;
        }

        /// <summary>
        /// Zero based global ID for this player.  Reconnection will give a new one.
        /// </summary>
        [SyncVar]
        public int id = -1;

		public GameObject svr_playerMarkerPrefab;
		public GameObject cnt_localPlayerOnlyPrefab;

        
		void Awake()
		{
			Debug.Log ( name + " RoomAugPlayerController J# awake" );
		}

        public void Start()
        {
			Debug.Log ( name + " RoomAugPlayerController J# start" );


        }
        
        //This is invoked for NetworkBehaviour objects when they become active on the server.
        public override void OnStartServer()
        {
			Debug.Log (name+" J# OnStartServer");


		}

        public override void OnStartLocalPlayer()
        {
			if (!localPlayerAuthority)
			{
				Debug.Log ("Another player (" + name + ") joined, and was disabled for on this client" );
				gameObject.SetActive ( false );
				return;
			}


			Debug.Log (name+" J# OnStartLocalPlayer");

        }
        


        [Command]
        public void CmdWatchEarth()
        {
            Debug.Log("Cmd");
        }
			
        

        [ClientRpc]
        public void RpcPortalTeleport()
        {
            Debug.Log(name + " RPC " );
        }

    }
}