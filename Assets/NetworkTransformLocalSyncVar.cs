using UnityEngine.Networking;
using UnityEngine;

//NetworkTransform and NetworkTransformChild doesn't work for Scale
//This is an alternative.
public class NetworkTransformLocalSyncVar : NetworkBehaviour {

    [SyncVar(hook = "OnSyncLocalPosition")]
    public Vector3 localPosition;

    [SyncVar(hook = "OnSyncLocalRotation")]
    public Quaternion localRotation;

    [SyncVar(hook = "OnSyncLocalScale")]
    public Vector3 localScale;

    public override int GetNetworkChannel()
    {
        return Channels.DefaultUnreliable;
    }

    void Start()
    {
        Debug.Log(name + " has netId: " + netId + "  assetId: " + GetComponent<NetworkIdentity>().assetId );
    }

    void Update()
    {

        //We don't need to check if they changed or not, as SyncVars only transmit when there is a change
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
        localScale = transform.localScale;
    }



    //Server to CLient syncing calls these on the client
    void OnSyncLocalPosition(Vector3 p)
    {
        transform.localPosition = p;
        //Debug.Log("Set Local Position to: " + p);
    }
    void OnSyncLocalRotation(Quaternion r)
    {
        transform.localRotation = r;
        //Debug.Log("Set Local Rotation to: " + r);
    }
    void OnSyncLocalScale(Vector3 s)
    {
        transform.localScale = s;
        //Debug.Log("Set Local Scale to: " + s);
    }
}
