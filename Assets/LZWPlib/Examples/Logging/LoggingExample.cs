using System;
using UnityEngine;

[HelpURL(Lzwp.LzwpLibManualUrl + "#logging-example")]
public class LoggingExample : MonoBehaviour
{
    void Start()
    {
        Lzwp.debug.Log("Some string");
        Lzwp.debug.Log("Log with <b>rich</b> text.");
        Lzwp.debug.Log("PI = {0}  TAU = {1}", Mathf.PI, 2f * Mathf.PI);
        Lzwp.debug.Log(Vector3.right);
        Lzwp.debug.LogWithContext(this, "Log with context");

        Lzwp.debug.Warning("Some warning string");
        Lzwp.debug.Warning("Unknown type: {0}", typeof(LoggingExample));

        Lzwp.debug.Error("Some error!");


        try
        {
            throw new Exception("Some exception");
        }
        catch (Exception ex)
        {
            Lzwp.debug.Exception(ex);
            Lzwp.debug.ExceptionWithContext(ex, this);
        }
    }
}
