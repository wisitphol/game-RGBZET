using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        // Connect to Photon
        PhotonNetwork.ConnectUsingSettings();
    }

    // Callbacks for connection status
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Photon");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("Disconnected from Photon: " + cause.ToString());
    }
}
