using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#audio-example")]
public class Wasp_AudioExample : MonoBehaviour
{
    public float speed = 4f;
    public float radius = 2.5f;

    void Update()
    {
        transform.position = new Vector3(
            Mathf.Sin(Time.time) * radius,
            1.8f,
            Mathf.Cos(Time.time) * radius
        );
    }
}
