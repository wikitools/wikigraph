using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#switch-scene-example")]
public class RotateObject_LzwpExample : MonoBehaviour {

    public Vector3 rotateAxis = Vector3.up;
    public float rotateSpeed = 20f;

	void Update () {
        transform.Rotate(rotateAxis * Time.deltaTime * rotateSpeed, Space.World);
	}
}
