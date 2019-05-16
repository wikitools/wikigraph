using UnityEngine;

#pragma warning disable 618  // disable 'member is obsolete' warnings caused by using old networking system
[HelpURL(Lzwp.LzwpLibManualUrl + "#sync-example")]
public class InstantiatedCube_SyncExample : MonoBehaviour
{
    public void InitCube(Vector3 pos, Quaternion rot, Color col)
    {
        GetComponent<NetworkView>().RPC("InitCubeRPC", RPCMode.AllBuffered, pos, rot, col.r, col.g, col.b);
    }

    [RPC]
    void InitCubeRPC(Vector3 pos, Quaternion rot, float r, float g, float b)
    {
        gameObject.name = "cubeObject";
        transform.SetParent(Instantiation_LzwpExample.Instance.cubesWrapper);
        transform.position = pos;
        transform.rotation = rot;
        GetComponent<Renderer>().material.color = new Color(r, g, b, 1f);
    }
}
#pragma warning restore 618
