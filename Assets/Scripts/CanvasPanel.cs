using HoloToolkit.Unity.InputModule;
using UnityEngine;

public class CanvasPanel : MonoBehaviour
{
    public Vector3 showViewOffset;
    public float rotationDeadZoneAngle = 5.0f;
    public float epsilon = 0.001f;
    public float speed = 5.0f;
    public float showTopAngle = 5.0f;
    public float showBottomAngle = 46.0f;
    public float outOfViewRecenterTime = 1.0f;

    private float lastRotation = -360.0f; // uninitialized value
    private Vector3 lastRotationVector;
    private float outOfViewTimer = 0.0f;

    private void Awake()
    {
    }

    private void OnEnable()
    {
        RecenterTools();
    }

    private void Update()
    {
        if (Camera.main != null && (CapturePhotoManager.Instance.GetCurrentStatus() != CurrentStatus.CaptureVideo
            || (CapturePhotoManager.Instance.GetCurrentStatus() == CurrentStatus.CaptureVideo && GazeManager.Instance.HitObject == null)))
        {
            Vector3 desiredRotationVector = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.y, Vector3.up) * Vector3.forward;
            Vector3 verticalLook = Quaternion.AngleAxis(Camera.main.transform.rotation.eulerAngles.x, Camera.main.transform.right) * Camera.main.transform.forward;

            float angle = Vector3.Angle(desiredRotationVector, verticalLook);

            // detect if the tool panel is in the user's view, if it isn't, start a timer to
            //  recenter it so it is directly in front of them when they look back at it
            if ((angle < showTopAngle || angle > showBottomAngle) || verticalLook.y > 0)
            {
                outOfViewTimer += Time.deltaTime;

                if (outOfViewTimer >= outOfViewRecenterTime)
                {
                    RecenterTools();
                    outOfViewTimer = outOfViewRecenterTime;
                }
            }
            else
            {
                // if the user is looking at the toolbar, reset the timer
                outOfViewTimer = 0.0f;
            }

            if (lastRotation == -360.0f) // uninitialized value
            {
                RecenterTools();
            }
            else
            {
                float angleDifference = SignedAngle(lastRotationVector, desiredRotationVector, Vector3.up);
                float unsignedAngleDifference = Mathf.Abs(angleDifference);

                if (unsignedAngleDifference > rotationDeadZoneAngle)
                {
                    lastRotation += Mathf.Sign(angleDifference) * (unsignedAngleDifference - rotationDeadZoneAngle);
                    lastRotationVector = Quaternion.AngleAxis(lastRotation, Vector3.up) * Vector3.forward;
                }
            }

            Quaternion panelRotation = Quaternion.Euler(0.0f, lastRotation, 0.0f);
            UpdatePosition(panelRotation);
        }
    }

    private void RecenterTools()
    {
        float desiredRotation = Camera.main.transform.rotation.eulerAngles.y;
        lastRotation = desiredRotation;
        lastRotationVector = Quaternion.AngleAxis(desiredRotation, Vector3.up) * Vector3.forward;
    }

    private void UpdatePosition(Quaternion rotation)
    {
        Vector3 targetPos = Camera.main.transform.position + (rotation * showViewOffset);

        if (Vector3.Distance(targetPos, transform.position) > epsilon)
        {
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        }
    }

    private float SignedAngle(Vector3 vector1, Vector3 vector2, Vector3 positiveNormal)
    {
        Vector3 vectorCrossProduct = Vector3.Cross(vector1, vector2);
        float sign = Mathf.Sign(Vector3.Dot(vectorCrossProduct, positiveNormal));
        return sign * Vector3.Angle(vector1, vector2);
    }
}
