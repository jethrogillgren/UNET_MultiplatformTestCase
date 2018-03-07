using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Networking;

#if (UNITY_EDITOR || UNITY_STANDALONE)
using System.Net;
using System.Net.Sockets;
#endif

namespace RoomAug.Networking
{
    //Any non-UNET specific Network goes here.
    //All Networking config lives here by default.
    public class RoomAugNetworkController : MonoBehaviour {


        public RoomAugNetworkDiscovery networkDiscovery;
        public RoomAugNetworkManager networkManager;

        //public GenericUDPConnection udpConnection;

        public enum NetworkType
        {
            Auto = -1,
            Server = 0,
            TangoPlayer = 1,
            HololensPlayer = 2,
            ARTKAgent = 3,
            DesktopHololensPlayer = 4
        }
        public static bool IsAPlayer(NetworkType networkType)
        {
            return (networkType == NetworkType.TangoPlayer || networkType == NetworkType.HololensPlayer || networkType == NetworkType.DesktopHololensPlayer);
        }
        public NetworkType overrideNetworkType;

        public int ARToolkit_CamID = 0; //Only applicable for ARToolkitAgents

        public string ServerIp;
        public string LocalIp;

        public uint maxPlayers = 5;
        public Dictionary<int, NetworkConnection> connectedPlayers = new Dictionary<int, NetworkConnection>();

        public bool Connected
        {
            get
            {
                //TODO different logic for different network types, eg ARToolkitAgent or Server..
                return networkDiscovery.Connected;
            }
        }


        void Awake()
        {
            //networkDiscovery = FindObjectOfType<RoomAugNetworkDiscovery>();
            //networkManager = FindObjectOfType<RoomAugNetworkManager>();

            if (overrideNetworkType == NetworkType.Auto)
            {
				if (Application.platform == RuntimePlatform.Android)
					overrideNetworkType = NetworkType.TangoPlayer;
				else if (Application.platform == RuntimePlatform.WSAPlayerX86)
					overrideNetworkType = NetworkType.HololensPlayer;
				else if (Application.platform == RuntimePlatform.OSXEditor
				        || Application.platform == RuntimePlatform.WindowsEditor
				        || Application.platform == RuntimePlatform.OSXPlayer
				        || Application.platform == RuntimePlatform.WindowsPlayer
						|| Application.isEditor)
					overrideNetworkType = NetworkType.Server;
				else
					Debug.LogError ("NetworkType.Auto not supported on this platform: " + Application.platform );
            }

        }


		//TODO strip out cleanly
        public void ResetIMUPosTracking()
        {
#if (UNITY_EDITOR || UNITY_STANDALONE) && !UNITY_ANDROID
			//if(svr_arduinoComm)
            //	svr_arduinoComm.resetPos = true;
#endif
        }
    }
}
