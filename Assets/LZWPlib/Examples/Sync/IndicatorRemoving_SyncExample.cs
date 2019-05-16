using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#sync-example")]
public class IndicatorRemoving_SyncExample : MonoBehaviour {

    public Instantiation_LzwpExample gameManager;

    void OnTriggerEnter(Collider other)
    {
        if (Lzwp.sync.isMaster && other.name == "cubeObject")
            gameManager.AddElementToDestroyList(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (Lzwp.sync.isMaster && other.name == "cubeObject")
            gameManager.RemoveElementFromDestroyList(other.gameObject);
    }
}
