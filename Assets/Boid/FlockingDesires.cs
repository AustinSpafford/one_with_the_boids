using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingDesires : MonoBehaviour
{
	public Quaternion IdlingLocalOrientation = Quaternion.identity;
	public float IdlingSpeed = 2.0f;

	public float OrientationMatchingPeakDistanceRatio = 0.75f;

	public void Start ()
	{
	}

	public void Update ()
	{
	}

	public Quaternion DesiredOrientation
	{
		get
		{
			UpdateCache();

			return cachedDesiredOrientation;
		}
	}

	public float DesiredSpeed
	{
		get
		{
			UpdateCache();

			return cachedDesiredSpeed;
		}
	}

	private int cacheConditionFrameIndex = -1;
	private Quaternion cachedDesiredOrientation = Quaternion.identity;
	private float cachedDesiredSpeed = 0.0f;

	private void UpdateCache ()
	{
		// If we haven't generated vectors for the current frame.
		if (cacheConditionFrameIndex != Time.frameCount)
		{
			var neighborDetector = GetComponentInChildren<FlockingNeighborDetector>();

			if (neighborDetector != null)
			{
				Vector3 desiredVelocity = BuildDesiredVelocity(neighborDetector);
				
				// TODO Additionally factor in the neighbor's average roll about the desired velocity axis.
				cachedDesiredOrientation = Quaternion.LookRotation(desiredVelocity, Vector3.up);
				cachedDesiredSpeed = desiredVelocity.magnitude;
				
				cacheConditionFrameIndex = Time.frameCount;
			}
		}
	}

	private Vector3 BuildDesiredVelocity (FlockingNeighborDetector neighborDetector)
	{
		Vector3 idlingVelocity = BuildIdlingBehaviorVelocity();
		float idlingSpeed = idlingVelocity.magnitude;

		Vector3 orientationVelocity = BuildOrientationBehaviorVelocity(neighborDetector);
		float orientationSpeed = orientationVelocity.magnitude;
		
		float naiveTotalSpeed = (
			idlingSpeed + 
			orientationSpeed);

		// To avoid having the behaviors increase our speed purely by merit of their existence, weight each
		// behavior by its requested speed versus the others. A single behavior is still able to request an increased
		// overall speed, and will also wind up "drowning out" the behaviors making more minor responses.
		// Consideration: The behavior functions could return an explicit weight in addition to the velocity, which would permit requests to slow down.
		Vector3 weightedVelocitySum =
			(idlingVelocity * (idlingSpeed / naiveTotalSpeed)) +
			(orientationVelocity * (orientationSpeed / naiveTotalSpeed));

		return weightedVelocitySum;
	}

	private Vector3 BuildIdlingBehaviorVelocity ()
	{
		return (IdlingSpeed * (IdlingLocalOrientation * transform.forward));
	}

	private Vector3 BuildOrientationBehaviorVelocity (FlockingNeighborDetector neighborDetector)
	{
		Vector3 summedNeighborVelocity = Vector3.zero;

		foreach (var neighbor in neighborDetector.Neighbors)
		{
			var neighborFlightController = neighbor.GetComponent<FlightController>();

			if (neighborFlightController != null)
			{
				float neighborCurrentSpeed = neighborFlightController.CurrentSpeed;

				float neighborDistance = Vector3.Distance(transform.position, neighbor.transform.position);

				// Blend in our consideration of the neighbor so as to avoid twitching when they're first sighted.
				float neighborConsiderationFraction = 
					Mathf.Clamp01(Mathf.InverseLerp(
						neighborDetector.VisionRadius, 
						(OrientationMatchingPeakDistanceRatio * neighborDetector.VisionRadius), 
						neighborDistance));

				Vector3 neighborActualVelocity = (neighborCurrentSpeed * neighbor.transform.forward);

				Vector3 neighborWeightedVelocity = (neighborConsiderationFraction * neighborActualVelocity);

				summedNeighborVelocity += neighborWeightedVelocity;
			}
		}

		Vector3 meanNeighborVelocity = (summedNeighborVelocity / (Mathf.Max(neighborDetector.Neighbors.Count, 1)));

		return meanNeighborVelocity;
	}
}


