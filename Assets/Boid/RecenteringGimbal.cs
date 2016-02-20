using UnityEngine;
using System.Collections;

public class RecenteringGimbal : MonoBehaviour
{
	public float AngleSamplingWindowSeconds = 0.05f;

	public float ActualHeadMinAngularVelocity = 1.0f;
	public float ActualHeadMaxAngularVelocity = 5.0f;

	public float RecenteringEffectMinAngularVelocity = 10.0f;
	public float RecenteringEffectMaxAngularVelocity = 30.0f;

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

			if (averageActualHeadAngularVelocity > ActualHeadMinAngularVelocity)
			{
				float currentRecenteringEffectFraction = 
					Mathf.Clamp01(Mathf.InverseLerp(
						ActualHeadMinAngularVelocity, 
						ActualHeadMaxAngularVelocity, 
						averageActualHeadAngularVelocity));
				
				float currentRecenteringEffectAngularVelocity=
					Mathf.Lerp(
						RecenteringEffectMinAngularVelocity,
						RecenteringEffectMaxAngularVelocity,
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

