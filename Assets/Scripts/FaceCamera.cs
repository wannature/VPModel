using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    public Vector3 rotationOffset;

    [Tooltip("If true, the object will face the camera while maintaining a world up (only rotates around y-axis).")]
    public bool forceToWorldUp = false;

    [Tooltip("If true, the object will face the camera while maintaining a camera up.")]
    public bool forceCameraUp = false;

    // this needs to happen after all positions have been updated
    protected virtual void LateUpdate()
    {
        if (Camera.main && (CapturePhotoManager.Instance.GetCurrentStatus() != CurrentStatus.CaptureVideo
            || (CapturePhotoManager.Instance.GetCurrentStatus() == CurrentStatus.CaptureVideo && GazeManager.Instance.HitObject == null)))
        {
            FaceDirection(transform.position - Camera.main.transform.position);
        }
    }

    public void FaceDirection(Vector3 forwardDirection)
    {
        if (forceCameraUp)
        {
            transform.rotation = Quaternion.LookRotation(forwardDirection.normalized, Camera.main.transform.up) * Quaternion.Euler(rotationOffset);
        }
        else if (forceToWorldUp)
        {
            transform.rotation = Quaternion.LookRotation(forwardDirection.normalized, Vector3.up) * Quaternion.Euler(rotationOffset);
            transform.rotation = Quaternion.Euler(0.0f, transform.rotation.eulerAngles.y, 0.0f);
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(forwardDirection.normalized) * Quaternion.Euler(rotationOffset);
        }
    }
}
