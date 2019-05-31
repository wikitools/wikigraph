using System.Collections.Generic;
using UnityEngine;

#pragma warning disable 618  // disable 'member is obsolete' warnings caused by using old networking system
[HelpURL(Lzwp.LzwpLibManualUrl + "#sync-example")]
public class Instantiation_LzwpExample : LZWPlib.UnitySingleton<Instantiation_LzwpExample>
{
    protected Instantiation_LzwpExample() { }

    public Transform cubesWrapper;

    public GameObject cubePrefab;

    public GameObject indicatorAdding;
    public GameObject indicatorRemoving;

    Material mat;

    bool adding = true;

    List<GameObject> elementsToDestroy = new List<GameObject>();

    NetworkView nv;



    void Start()
    {
        nv = GetComponent<NetworkView>();
        mat = indicatorAdding.GetComponent<Renderer>().material;

        if (Lzwp.sync.isMaster)
        {
            SetRandomIndicatorColor();
            UpdateIndicators();

            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button1).OnPress += SetMode1;
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Button2).OnPress += SetMode2;
            Lzwp.input.flysticks[0].GetButton(LzwpInput.Flystick.ButtonID.Fire).OnPress += ExecuteAction;
        }
    }
    
    void Update()
    {
        if (Lzwp.sync.isMaster)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
                SetMode1();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
                SetMode2();

            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
                ExecuteAction();
        }
    }


    void SetMode1()
    {
        adding = true;
        UpdateIndicators();
        elementsToDestroy.Clear();
    }

    void SetMode2()
    {
        adding = false;
        UpdateIndicators();
    }

    void UpdateIndicators()
    {
        nv.RPC("UpdateIndicatorsRPC", RPCMode.AllBuffered, adding ? 1 : 0);
    }
    
    [RPC]
    void UpdateIndicatorsRPC(int addingMode)
    {
        indicatorAdding.SetActive(addingMode == 1);
        indicatorRemoving.SetActive(addingMode != 1);
    }


    public void AddElementToDestroyList(GameObject go)
    {
        if (!adding && !elementsToDestroy.Contains(go))
            elementsToDestroy.Add(go);
    }

    public void RemoveElementFromDestroyList(GameObject go)
    {
        if (!adding && elementsToDestroy.Contains(go))
            elementsToDestroy.Remove(go);
    }

    void ExecuteAction()
    {
        if (adding)
        {
            GameObject cube = Network.Instantiate(cubePrefab, indicatorAdding.transform.position, indicatorAdding.transform.rotation, 0) as GameObject;

            cube.GetComponent<InstantiatedCube_SyncExample>().InitCube(indicatorAdding.transform.position, indicatorAdding.transform.rotation, mat.color);

            SetRandomIndicatorColor();
        }
        else
        {
            foreach (GameObject go in elementsToDestroy)
                Network.Destroy(go);
            elementsToDestroy.Clear();
        }
    }

    void SetRandomIndicatorColor()
    {
        Color c = Random.ColorHSV(0, 1f, 0.8f, 1f, 0.8f, 1f, 1f, 0.2f);
        nv.RPC("ChangeColorRPC", RPCMode.AllBuffered, c.r, c.g, c.b, c.a);
    }

    [RPC]
    void ChangeColorRPC(float r, float g, float b, float a)
    {
        mat.color = new Color(r, g, b, a);
    }
}
#pragma warning restore 618
