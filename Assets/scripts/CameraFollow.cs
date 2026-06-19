using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    public Transform playerTarget;   // Drag your player object here
    public Transform pivotTarget;    // Drag a child GameObject of the player (e.g., "CameraPivot")

    [Header("Offset & Settings")]
    public Vector3 defaultOffset = new Vector3(0f, 2f, -5f); // Base offset relative to player
    public float positionSmoothTime = 0.15f;
    public float rotationSlewSpeed = 5.0f; // Lower = lazy/slower slew, Higher = snappier rotation

    [Header("Collision Detection")]
    public LayerMask collisionLayers; // Select layers the camera should collide with (e.g., Default, Environment)
    public float cameraRadius = 0.25f; // How wide the camera collision bubble is

    private Vector3 currentVelocity;
    private float targetDistance;

    void Start()
    {
        if (pivotTarget == null && playerTarget != null)
        {
            pivotTarget = playerTarget;
        }
        
        targetDistance = defaultOffset.magnitude;
    }

    void LateUpdate()
    {
        if (playerTarget == null || pivotTarget == null) return;

        // 1. Calculate Target Position based on Player's current rotation and default offset
        Vector3 targetDirection = playerTarget.transform.TransformDirection(defaultOffset.normalized);
        Vector3 desiredCameraPos = pivotTarget.position + (targetDirection * targetDistance);

        // 2. Handle Collision Detection (SphereCast from Pivot to Desired Camera Position)
        Vector3 castDirection = desiredCameraPos - pivotTarget.position;
        float castDistance = castDirection.magnitude;

        if (Physics.SphereCast(pivotTarget.position, cameraRadius, castDirection.normalized, out RaycastHit hit, castDistance, collisionLayers))
        {
            // If an obstacle is hit, snap the desired position closer to the pivot
            desiredCameraPos = pivotTarget.position + (castDirection.normalized * (hit.distance - 0.05f));
        }

        // 3. Apply Slew (Smoothing) to Position
        transform.position = Vector3.SmoothDamp(transform.position, desiredCameraPos, ref currentVelocity, positionSmoothTime);

        // 4. Calculate and Smoothly Slew the Rotation to look at the Pivot
        Vector3 lookDirection = pivotTarget.position - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            
            // Slew the rotation using Spherical Linear Interpolation
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSlewSpeed * Time.deltaTime);
        }
    }
}
