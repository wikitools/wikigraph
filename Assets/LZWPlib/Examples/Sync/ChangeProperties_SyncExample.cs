using UnityEngine;

#pragma warning disable 618  // disable 'member is obsolete' warnings caused by using old networking system
[HelpURL(Lzwp.LzwpLibManualUrl + "#sync-example")]
public class ChangeProperties_SyncExample : MonoBehaviour
{
    Material mat;
    NetworkView nv;

    void Start()
    {
        nv = GetComponent<NetworkView>();
        mat = GetComponent<Renderer>().material;

        if (Lzwp.sync.isMaster)
        {
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnPress += ChangeSize;
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnPress += ChangeColor;
        }
    }

    void Update()
    {
        if (Lzwp.sync.isMaster)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
                ChangeSize();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
                ChangeColor();
        }
    }



    void ChangeSize()
    {
        transform.localScale = Vector3.one * Random.Range(0.6f, 2f);
        nv.RPC("ChangeSizeRPC", RPCMode.OthersBuffered, transform.localScale.x);
    }

    [RPC]
    void ChangeSizeRPC(float scale)
    {
        transform.localScale = Vector3.one * scale;
    }



    void ChangeColor()
    {
        Color c = Random.ColorHSV(0, 1f, 0.8f, 1f, 0.8f, 1f, 1f, 1f);
        nv.RPC("ChangeColorRPC", RPCMode.AllBuffered, c.r, c.g, c.b, c.a);
    }

    [RPC]
    void ChangeColorRPC(float r, float g, float b, float a)
    {
        mat.color = new Color(r, g, b, a);
    }
}
#pragma warning restore 618
