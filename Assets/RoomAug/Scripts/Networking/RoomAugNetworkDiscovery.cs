//Adopted from Hololens Example Project
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using UnityEngine;
using UnityEngine.Networking;

#if WINDOWS_UWP
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif

namespace RoomAug.Networking
{

    public class RoomAugNetworkDiscovery : NetworkDiscovery
    {

        public RoomAugNetworkController networkController;
        public RoomAugNetworkManager networkManager;



        public bool Connected
        {
            get
            {
                // we are connected if we are the server or if we aren't running discovery
                return (isServer || !running);
            }
        }


        private void Start()
        {
			Debug.Log(name + " J#  Network Starting as a " + networkController.overrideNetworkType );
            /*networkController = FindObjectOfType<RoomAugNetworkController>();
            if (!networkController)
                Debug.LogError("Unable to find the NetworkController");
            networkManager = FindObjectOfType<RoomAugNetworkManager>();
            if (!networkManager)
                Debug.LogError("Unable to find the NetworkManager");

            while (networkController == null)
            {
                networkController = FindObjectOfType<RoomAugNetworkController>();
                if (!networkController)
                    Debug.LogError("Unable to find the NetworkController");
            }*/

#if WINDOWS_UWP
            foreach (Windows.Networking.HostName hostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (hostName.DisplayName.Split(".".ToCharArray()).Length == 4)
                {
                    networkController.LocalIp = hostName.DisplayName;
                    break;
                }
            }
#else
            networkController.LocalIp = "editor"+UnityEngine.Random.Range(0, 999999).ToString();;
#endif
            Debug.Log("Set localIp: " + networkController.LocalIp);


            // Initializes NetworkDiscovery.
            Initialize();

            if (networkController.overrideNetworkType == RoomAugNetworkController.NetworkType.Server)
                Svr_StartHosting("Hello Sailor");
            else
                Cnt_StartListening();


        }

        private string GetLocalComputerName()
        {
#if WINDOWS_UWP
            foreach (Windows.Networking.HostName hostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (hostName.Type == HostNameType.DomainName)
                {

                    return hostName.DisplayName;
                }
            }
            Debug.LogWarning("Unable to get Computer Name");
            return "wazzmyname";
#else
            return System.Environment.ExpandEnvironmentVariables("%ComputerName%");
#endif
        }

        /// <summary>
        /// Called by UnityEngine when a broadcast is received. 
        /// </summary>
        /// <param name="fromAddress">When the broadcast came from</param>
        /// <param name="data">The data in the broad cast. Not currently used, but could
        /// be used for differntiating rooms or similar.</param>
        public override void OnReceivedBroadcast(string fromAddress, string data)
        {
			Debug.Log (name + " J# OnReceivedBroadcast from " + fromAddress + " of " + data);
            networkController.ServerIp = fromAddress.Substring(fromAddress.LastIndexOf(':') + 1);

            //Don't listen for any more games being broadcast
            if (running)
                Cnt_StopListening();

            networkManager.networkAddress = fromAddress;
            networkManager.StartClient();

            //Moving client type dependant stuff to NetworkManager Spawning system.
            /*if (RoomAugNetworkController.Instance.myNetworkType == Util.NetworkType.ARTKAgent)
                Cnt_StartAsARTKAgent();
            else
                Cnt_StartAsPlayer();*/
        }




        /// <summary>
        /// Call to stop listening for sessions.
        /// </summary>
        public void Cnt_StopListening()
        {
            Debug.Log(name + " Stopping Listening");
            StopBroadcast();
        }

        /// <summary>
        /// Call to start listening for sessions.
        /// </summary>
        public void Cnt_StartListening()
        {
			Debug.Log (name + "J# Listening for games");

            if (running)
                Cnt_StopListening();

            StartAsClient();
        }


        public void Svr_StartHosting(string SessionName)
        {
			Debug.Log (name + "J# Starting as Host");
#if WINDOWS_UWP
            //This is on device only, not in editor playmode
            string LocalName = SessionName;

            foreach (Windows.Networking.HostName hostName in Windows.Networking.Connectivity.NetworkInformation.GetHostNames())
            {
                if (string.IsNullOrEmpty(LocalName))
                {
                    LocalName = hostName.DisplayName;
                }
                if (hostName.DisplayName.Split(".".ToCharArray()).Length == 4)
                {
                    Debug.Log(hostName.DisplayName);
                    NetworkManager.singleton.serverBindToIP = true;
                    NetworkManager.singleton.serverBindAddress = hostName.DisplayName;
                }
                // NetworkManager.singleton.serverBindToIP = hostName.DisplayName;
            }
#endif
            // broadcastData = LocalName;
            // Starting as a 'host' makes us both a client and a server.
            // There are nuances to this in UNet's sync system, so do make sure
            // to test behavior of your networked objects on both a host and a client 
            // device.
            networkManager.StartServer();
            // Start broadcasting for other clients.
            StartAsServer();
        }


        private void Cnt_StartAsARTKAgent()
        {
			Debug.Log (name + " J# Starting as ARTKAgent");
#if (UNITY_EDITOR || UNITY_STANDALONE)

            //Test
            //udpConnection = new GenericUDPConnection();
            //udpConnection.Connect(serverIp, Util.portARToolkitAgentBase + ARToolkit_CamID);
#endif
        }

        private void Cnt_StartAsPlayer()
        {
			Debug.Log (name + " J# Starting as Player");
            // We heard about a game, now join the networked experience as a client.
            networkManager.StartClient();
        }

    }
}