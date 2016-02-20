using UnityEngine;
using System.Collections;

public class RecenteringGimbal : MonoBehaviour
{
	public float AngleSamplingWindowSeconds = 0.02f;

	public float ActualHeadAngularVelocityMin = 1.0f;
	public float ActualHeadAngularVelocityMax = 5.0f;

	public float RecenteringEffectAngularVelocityMin = 10.0f;
	public float RecenteringEffectAngularVelocityMax = 30.0f;

	public Quaternion GimbalGoalLocalOrientation = Quaternion.identity;

	public void Start ()
	{
		var attachedCamera = GetComponentInChildren<Camera>();

		if (attachedCamera != null)
		{
			lastKnownHeadOrientation = attachedCamera.transform.localRotation;
		}
	}
	
	public void Update ()
	{
		var attachedCamera = GetComponentInChildren<Camera>();

		if (attachedCamera != null)
		{
			float degreesFromLastKnownHeadOrientation = Quaternion.Angle(attachedCamera.transform.localRotation, lastKnownHeadOrientation);

			averageActualHeadAngularVelocity = 
				Mathf.Lerp(
					averageActualHeadAngularVelocity, 
					degreesFromLastKnownHeadOrientation, 
					(Time.deltaTime / AngleSamplingWindowSeconds));

			if (Input.GetButton("Debug Head Whip"))
			{
				averageActualHeadAngularVelocity = ActualHeadAngularVelocityMax;
			}

			if (averageActualHeadAngularVelocity > ActualHeadAngularVelocityMin)
			{
				float currentRecenteringEffectFraction = 
					Mathf.Clamp01(Mathf.InverseLerp(
						ActualHeadAngularVelocityMin, 
						ActualHeadAngularVelocityMax, 
						averageActualHeadAngularVelocity));
				
				float currentRecenteringEffectAngularVelocity=
					Mathf.Lerp(
						RecenteringEffectAngularVelocityMin,
						RecenteringEffectAngularVelocityMax,
						currentRecenteringEffectFraction);

				// Rotate the gimbal towards wherever our parent has indicated "forward" resides.
				transform.localRotation =
					Quaternion.RotateTowards(
						transform.localRotation,
						GimbalGoalLocalOrientation,
						(currentRecenteringEffectAngularVelocity * Time.deltaTime));
			}

			// NOTE: We update the last known orientation as the last step so as to ensure that we're only tracking the hooman's movements.
			lastKnownHeadOrientation = attachedCamera.transform.localRotation;
		}
	}

	private float averageActualHeadAngularVelocity = 0.0f;
	private Quaternion lastKnownHeadOrientation = Quaternion.identity;
}

