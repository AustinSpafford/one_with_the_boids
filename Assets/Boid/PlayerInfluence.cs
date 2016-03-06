using UnityEngine;
using System.Collections;

public class PlayerInfluence : MonoBehaviour
{
	public float ForwardThrottleDesiredSpeed = 5.0f;
	public float ReverseThrottleDesiredSpeed = 0.0f;

	public void Start ()
	{
	}
	
	public void Update ()
	{
		// TODO Move the camera-recenter logic into a global script.
		if (Input.GetButtonDown("Recenter HMD"))
		{
			UnityEngine.VR.InputTracking.Recenter();
		}
	}

	public float GetInfluenceFraction ()
	{
		float inputThrottle = Input.GetAxis("Throttle");
		
		return Mathf.Clamp01(Mathf.Abs(inputThrottle));
	}

	public Quaternion BuildDesiredOrientation ()
	{
		Quaternion desiredOrientation = transform.rotation;

		var attachedCamera = GetComponentInChildren<Camera>();

		if (attachedCamera != null)
		{
			desiredOrientation = attachedCamera.transform.rotation;
		}

		return desiredOrientation;
	}

	public float GetDesiredSpeed ()
	{
		float desiredSpeed = 0.0f;
		
		if (Input.GetAxis("Throttle") >= 0)
		{
			desiredSpeed = ForwardThrottleDesiredSpeed;
		}
		else
		{
			desiredSpeed = ReverseThrottleDesiredSpeed;
		}

		return desiredSpeed;
	}
}
