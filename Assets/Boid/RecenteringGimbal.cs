using UnityEngine;
using System.Collections;

public class RecenteringGimbal : MonoBehaviour
{
	[Tooltip("When enabled, the gimbal will always stay upright and pointing at the horizon.\nThis is generally desired, but should be disabled in space-games.")]
	public bool LockGimbalToHorizon = true;

	public Quaternion ParentSpaceGimbalGoalOrientation = Quaternion.identity;

	public float AngleSamplingWindowSeconds = 0.02f;

	public float ActualHeadAngularVelocityMin = 1.0f;
	public float ActualHeadAngularVelocityMax = 5.0f;

	public float RecenteringEffectAngularVelocityMin = 10.0f;
	public float RecenteringEffectAngularVelocityMax = 30.0f;

	public static float GimbalLockVerticalDeadzoneDegrees = 30.0f; // Within this cone, we consider ourselves to be gimbal-locked, making it difficult to make horizon-related decisions.

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
				
				var unrestrictedWorldSpaceGimbalGoalOrientation = (transform.parent.rotation * ParentSpaceGimbalGoalOrientation);

				if (LockGimbalToHorizon)
				{
					Vector3 horizontalGimbalGoalHeading =
						Vector3.ProjectOnPlane(
							(unrestrictedWorldSpaceGimbalGoalOrientation * Vector3.forward),
							Vector3.up);

					if (horizontalGimbalGoalHeading.sqrMagnitude >= Mathf.Pow(Mathf.Sin(GimbalLockVerticalDeadzoneDegrees * Mathf.Deg2Rad), 2))
					{
						var horizonLockedGimbalGoalOrientation = Quaternion.LookRotation(horizontalGimbalGoalHeading, Vector3.up);

						RotateGimbalTowardsGoal(horizonLockedGimbalGoalOrientation, currentRecenteringEffectAngularVelocity);
					}
					else
					{
						// We're currently gimbal-locked (too close to vertical), so we'll avoid making decisions until we've returned to the the horizon.
					}
				}
				else
				{
					RotateGimbalTowardsGoal(unrestrictedWorldSpaceGimbalGoalOrientation, currentRecenteringEffectAngularVelocity);
				}
			}

			// NOTE: We update the last known orientation as the last step so as to ensure that we're only tracking the hooman's movements.
			lastKnownHeadOrientation = attachedCamera.transform.localRotation;
		}
	}

	private float averageActualHeadAngularVelocity = 0.0f;
	private Quaternion lastKnownHeadOrientation = Quaternion.identity;

	private void RotateGimbalTowardsGoal(
		Quaternion worldSpaceGimbalGoalOrientation,
		float angularVelocity)
	{
		transform.rotation =
			Quaternion.RotateTowards(
				transform.rotation,
				worldSpaceGimbalGoalOrientation,
				(angularVelocity * Time.deltaTime));
	}
}

