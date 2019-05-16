using UnityEngine;
using UnityEngine.Events;

[HelpURL(Lzwp.LzwpLibManualUrl + "#TrackedObject")]
public class TrackedObject : MonoBehaviour
{
    public enum TrackedObjectType
    {
        Pose,
        Flystick,
        Glasses
    }

    public TrackedObjectType trackedType = TrackedObjectType.Flystick;

    public int idx = 0;

    public UnityEvent OnTrackingAcquired;
    public UnityEvent OnTrackingLost;

    LzwpPose pose;

    private void Start()
    {
        Lzwp.AddAfterInitializedAction(InitTracker);
    }

    private void OnDisable()
    {
        try
        {
            if (Lzwp.initialized)
            {
                Lzwp.input.PosesUpdated -= UpdatePose;
                pose.OnTrackingStateChanged -= TrackingStateChanged;
            }
        }
        catch (System.Exception) { }
    }

    void InitTracker()
    {
        pose = GetPose();

        Lzwp.debug.Log("GameObject '{0}' is tracking pose {1}", gameObject.name, pose.name);

        TrackingStateChanged(pose.tracked);

        Lzwp.input.PosesUpdated += UpdatePose;
        pose.OnTrackingStateChanged += TrackingStateChanged;
    }

    void TrackingStateChanged(bool tracked)
    {
        if (tracked)
            OnTrackingAcquired.Invoke();
        else
            OnTrackingLost.Invoke();
    }

    private LzwpPose GetPose()
    {
        if (trackedType == TrackedObjectType.Pose)
        {
            if (idx < Lzwp.input.poses.Count)
                return Lzwp.input.poses[idx];
        }
        else if (trackedType == TrackedObjectType.Flystick)
        {
            if (idx < Lzwp.input.flysticks.Count)
                return Lzwp.input.flysticks[idx].pose;
        }
        else if (trackedType == TrackedObjectType.Glasses)
        {
            if (idx < Lzwp.input.glasses.Count)
                return Lzwp.input.glasses[idx];
        }

        return new LzwpPose();
    }

    void LateUpdate()
    {
        if (Lzwp.initialized)
            UpdatePose(false);
    }

    void UpdatePose(bool hasNewData)
    {
        transform.position = pose.position;
        transform.rotation = pose.rotation;
    }
}
