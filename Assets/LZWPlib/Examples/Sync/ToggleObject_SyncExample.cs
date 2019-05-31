using UnityEngine;

#pragma warning disable 618  // disable 'member is obsolete' warnings caused by using old networking system
[HelpURL(Lzwp.LzwpLibManualUrl + "#sync-example")]
public class ToggleObject_SyncExample : MonoBehaviour
{

    public GameObject objToToggle;

    NetworkView nv;

    void Start()
    {
        nv = GetComponent<NetworkView>();

        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button3).OnPress += ToggleVisibility;
        Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button4).OnPress += ChangeOrientation;
    }

    void Update()
    {
        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
            ToggleVisibility();

        if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
            ChangeOrientation();
    }

    void ToggleVisibility()
    {
        if (!Lzwp.sync.isMaster)
            return;

        objToToggle.SetActive(!objToToggle.activeSelf);
        nv.RPC("SetVisibilityRPC", RPCMode.OthersBuffered, objToToggle.activeSelf ? 1 : 0);
    }

    [RPC]
    void SetVisibilityRPC(int visible)
    {
        objToToggle.SetActive(visible == 1);
    }





    void ChangeOrientation()
    {
        if (!Lzwp.sync.isMaster)
            return;

        Quaternion rot = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));
        nv.RPC("ChangeOrientationRPC", RPCMode.AllBuffered, rot);
    }

    [RPC]
    void ChangeOrientationRPC(Quaternion rot)
    {
        objToToggle.transform.rotation = rot;
    }
}
#pragma warning restore 618
