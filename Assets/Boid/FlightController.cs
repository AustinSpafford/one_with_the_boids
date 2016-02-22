using UnityEngine;
using System.Collections;

public class FlightController : MonoBehaviour
{
	public float RotationMotionSeconds = 1.0f;

	public void Start ()
	{
	}
	
	public void Update ()
	{
		Quaternion desiredOrientation;
		float desiredSpeed;
		GetDesires(out desiredOrientation, out desiredSpeed);

		// Rotate towards the desire.
		{
			float oldDegreesFromTarget = Quaternion.Angle(transform.rotation, desiredOrientation);

			float newDegreesFromTarget = 
				Mathf.SmoothDamp(
					oldDegreesFromTarget, // current
					0.0f, // target
					ref rotationSmoothingCurrentVelocity, // velocity
					RotationMotionSeconds); // smoothTime

			float maxRotationStepDegrees = (oldDegreesFromTarget - newDegreesFromTarget);

			var oldRotation = transform.rotation;

			transform.rotation = 
				Quaternion.RotateTowards(
					transform.rotation, 
					desiredOrientation, 
					maxRotationStepDegrees);
			
			// Let any children locally counteract this rotation (eg. to avoid motion-sickness).
			foreach (var counterRotator in GetComponentsInChildren<CounterRotator>())
			{
				counterRotator.OnCounteractableRotation(oldRotation, transform.rotation);
			}
		}

		// Translate forward.
		{
			transform.position += (transform.forward * (desiredSpeed * Time.deltaTime));
		}
	}

	private float rotationSmoothingCurrentVelocity = 0.0f;

	private void GetDesires (
		out Quaternion desiredOrientation,
		out float desiredSpeed)
	{
		desiredOrientation = Quaternion.identity;
		desiredSpeed = 0.0f;

		var flockingBehavior = GetComponentInChildren<FlockingDesires>();

		if (flockingBehavior != null)
		{
			desiredOrientation = flockingBehavior.DesiredOrientation;
			desiredSpeed = flockingBehavior.DesiredSpeed;

			var playerInfluence = GetComponentInChildren<PlayerInfluence>();

			if (playerInfluence != null)
			{
				var playerInfluenceFraction = playerInfluence.GetInfluenceFraction();
				
				if (playerInfluenceFraction > 0.0f)
				{
					// Apply the player's influence.
					desiredOrientation = Quaternion.Slerp(desiredOrientation, playerInfluence.BuildDesiredOrientation(), playerInfluenceFraction);
					desiredSpeed = Mathf.Lerp(desiredSpeed, playerInfluence.GetDesiredSpeed(), playerInfluenceFraction);
				}
			}
		}
	}
}
