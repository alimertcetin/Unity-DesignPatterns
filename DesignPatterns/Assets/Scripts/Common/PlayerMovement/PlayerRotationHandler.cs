using UnityEngine;

namespace XIV.DesignPatterns.Common.PlayerMovement
{
    public class PlayerRotationHandler : MonoBehaviour
    {
	    [SerializeField] Transform cinemachineTarget;
	    [SerializeField] float sensitivity = 2.5f;
	    [SerializeField] bool invertY;
	    [SerializeField] bool lockMouse;

	    Vector2 mouseDelta
	    {
		    get
		    {
			    if (lockMouse)
			    {
				    return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
			    }
			    var currentPosition = (Vector2)Input.mousePosition;
			    var diff = currentPosition - previousPosition;
			    previousPosition = currentPosition;
			    return diff;
		    }
	    }
	    
	    Vector2 previousPosition;
	    float cinemachineTargetPitch;
	    float rotationVelocity;

	    void Awake()
	    {
		    previousPosition = Input.mousePosition;
		    Cursor.lockState = lockMouse ? CursorLockMode.Locked : CursorLockMode.None;
	    }

	    void Update()
	    {
		    HandleRotation();
	    }

	    void HandleRotation()
	    {
		    var mouseDeltaCached = mouseDelta;
			cinemachineTargetPitch += mouseDeltaCached.y * sensitivity * (invertY ? 1 : -1);
			rotationVelocity = mouseDeltaCached.x * sensitivity;
			cinemachineTargetPitch = ClampAngle(cinemachineTargetPitch, -90f, 90f);
			cinemachineTarget.localRotation = Quaternion.Euler(cinemachineTargetPitch, 0.0f, 0.0f);
			transform.Rotate(Vector3.up * rotationVelocity);
		}

		static float ClampAngle(float angle, float min, float max)
		{
			if (angle < -360f) angle += 360f;
			if (angle > 360f) angle -= 360f;
			return Mathf.Clamp(angle, min, max);
		}
    }
}
