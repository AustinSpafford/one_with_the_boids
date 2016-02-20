using UnityEngine;
using System.Collections;

public class FlightController : MonoBehaviour
{
	public float MaxDegreesPerSecond = 60.0f;

	public void Start ()
	{
	
	}
	
	public void Update ()
	{
		var boidActor = transform.Find("Boid Actor");

		if (boidActor != null)
		{
			Quaternion desiredOrientation;
			float desiredSpeed;
			GetDesires(out desiredOrientation, out desiredSpeed);

			// Immediately rotate the boid actor, and set that as a rotation-goal for the camera.
			{
				Vector3? cameraPositionInActorSpace = null;
				var cameraGimbal = GetComponentInChildren<RecenteringGimbal>();

				// Before applying a rotation, preserve the relative positions of any sibling cameras.
				if (cameraGimbal != null)
				{
					cameraPositionInActorSpace = boidActor.transform.InverseTransformPoint(cameraGimbal.transform.position);
				}

				// TODO Use Mathf.SmoothDamp().
				float actorRotationStepFraction = (3.0f * Time.deltaTime);

				boidActor.transform.rotation = 
					Quaternion.Slerp(
						boidActor.transform.rotation, 
						desiredOrientation, 
						actorRotationStepFraction);
				
				// Restore the camera's position, and (as movement allows) point it in the same direction as the actor.
				if (cameraGimbal != null)
				{
					cameraGimbal.transform.position = boidActor.transform.TransformPoint(cameraPositionInActorSpace.Value);

					cameraGimbal.GimbalGoalLocalOrientation = boidActor.transform.localRotation;
				}
			}

			// Translate the entire root, since that will keep the actor and camera in sync with each other.
			transform.position += (boidActor.transform.forward * Time.deltaTime * desiredSpeed);
		}
	}

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

			var playerInfluence = GetComponent<PlayerInfluence>();

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
