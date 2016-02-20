using UnityEngine;
using System.Collections;

public class PlayerInfluence : MonoBehaviour
{
	public void Start ()
	{
	}
	
	public void Update ()
	{
		// TODO Relocate the camera-recenter logic to global host. Also make the inputs bindable.
		if (Input.GetButtonDown("Recenter HMD"))
		{
			UnityEngine.VR.InputTracking.Recenter();
		}
	}

	public float GetInfluenceFraction ()
	{
		float inputVerticalAxis = Input.GetAxis("Throttle");
		
		return Mathf.Clamp01(inputVerticalAxis);
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
		return 3.0f;
	}
}
