using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#sync-example")]
public class SyncTransform_SyncExample : MonoBehaviour
{
    Vector3 initialPosition;

    public float moveSpeed = 3f;
    public float rotateSpeed = 20f;
    public float scaleSpeed = 2f;

    void Start()
    {
        initialPosition = transform.position;
    }

    void Update()
    {
        if (Lzwp.sync.isMaster)
        {
            transform.position = initialPosition + Vector3.up * Mathf.Sin(Time.time * moveSpeed) * 0.5f;
            transform.Rotate(Vector3.up, Time.deltaTime * rotateSpeed);
            transform.localScale = Vector3.one * (Mathf.Sin(Time.time * scaleSpeed) * 0.5f + 1.5f);
        }
    }
}
