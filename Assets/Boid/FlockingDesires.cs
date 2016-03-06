using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingDesires : MonoBehaviour
{
	public float AvoidanceBaseDistance = 3.0f;

	[Tooltip("At the base-distance, we'll avoid collisions with this speed.")]
	public float AvoidanceBaseSpeed = 3.0f;
	
	[Tooltip("Even during a total freak-out (eg. spawning on top of each other), we'll stay under this speed to help prevent insane maneuvers.")]
	public float AvoidanceMaxSpeed = 20.0f;

	public float CohesionMaxSpeed = 3.0f;

	[Tooltip("When our neighborhood's center-of-mass reaches this distance and then approaches our center, the cohesion behavior fades out since it's being satisfied.")]
	public float CohesionSatisfactionFalloffDistance = 2.0f;

	public Quaternion IdlingLocalOrientation = Quaternion.identity;
	public float IdlingSpeed = 2.0f;

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

	private Vector3 BuildAvoidanceBehaviorVelocity (
		FlockingNeighborDetector neighborDetector)
	{
		Vector3 naiveNeighborhoodAvoidanceVelocity = Vector3.zero;

		foreach (var neighbor in neighborDetector.Neighbors)
		{
			Vector3 selfToNeighborDelta = (neighbor.transform.position - transform.position);
			float selfToNeighborDistance =  selfToNeighborDelta.magnitude;
			Vector3 selfToNeighborDirection = (selfToNeighborDelta / Mathf.Max(selfToNeighborDistance, Mathf.Epsilon));

			float normalizedDistanceToNeighbor = (selfToNeighborDistance / AvoidanceBaseDistance);
			
			float naiveNeighborAvoidanceSpeed = (AvoidanceBaseSpeed / Mathf.Max(normalizedDistanceToNeighbor, Mathf.Epsilon));

			// Blend in our consideration of the neighbor so as to avoid twitching when they're first sighted.
			float neighborConsiderationFraction = neighborDetector.GetNeighborConsiderationFraction(neighbor);

			naiveNeighborhoodAvoidanceVelocity += (-1 * (neighborConsiderationFraction * naiveNeighborAvoidanceSpeed) * selfToNeighborDirection);
		}

		float naiveNeighborhoodAvoidanceSpeed = naiveNeighborhoodAvoidanceVelocity.magnitude;

		Vector3 neighborhoodAvoidanceDirection = (naiveNeighborhoodAvoidanceVelocity / Mathf.Max(naiveNeighborhoodAvoidanceSpeed, Mathf.Epsilon));
		
		Vector3 finalNeighborhoodAvoidanceVelocity = (Mathf.Min(naiveNeighborhoodAvoidanceSpeed, AvoidanceMaxSpeed) * neighborhoodAvoidanceDirection);
		
		return finalNeighborhoodAvoidanceVelocity;
	}

	private Vector3 BuildCohesionBehaviorVelocity (
		FlockingNeighborDetector neighborDetector)
	{
		Vector3 summedSelfToNeighborDeltas = Vector3.zero;
		float summedNeighborConsiderationFractions = 1.0f; // NOTE: We're including ourselves in the center-of-mass calculation.

		foreach (var neighbor in neighborDetector.Neighbors)
		{
			Vector3 selfToNeighborDelta = (neighbor.transform.position - transform.position);

			// Blend in our consideration of the neighbor so as to avoid twitching when they're first sighted.
			float neighborConsiderationFraction = neighborDetector.GetNeighborConsiderationFraction(neighbor);

			summedSelfToNeighborDeltas += (neighborConsiderationFraction * selfToNeighborDelta);
			summedNeighborConsiderationFractions += neighborConsiderationFraction;
		}

		Vector3 selfToCenterOfMass = (summedSelfToNeighborDeltas / summedNeighborConsiderationFractions);
		float selfToCenterOfMassDistance = selfToCenterOfMass.magnitude;

		float initialNeighborThrottleFraction = Mathf.Clamp01(summedNeighborConsiderationFractions);
		float peakSatisfactionThrottleFraction = Mathf.Clamp01(selfToCenterOfMassDistance / CohesionSatisfactionFalloffDistance);

		float throttleFraction = (initialNeighborThrottleFraction * peakSatisfactionThrottleFraction);
		float throttleSpeed = (CohesionMaxSpeed * throttleFraction);

		Vector3 cohesionDirection = (selfToCenterOfMass / Mathf.Max(selfToCenterOfMassDistance, Mathf.Epsilon));
		Vector3 cohesionVelocity = (throttleSpeed * cohesionDirection);

		return cohesionVelocity;
	}

	private Vector3 BuildDesiredVelocity (
		FlockingNeighborDetector neighborDetector)
	{
		Vector3 avoidanceVelocity = BuildAvoidanceBehaviorVelocity(neighborDetector);
		float avoidanceSpeed = avoidanceVelocity.magnitude;

		Vector3 cohesionVelocity = BuildCohesionBehaviorVelocity(neighborDetector);
		float cohesionSpeed = cohesionVelocity.magnitude;

		Vector3 idlingVelocity = BuildIdlingBehaviorVelocity();
		float idlingSpeed = idlingVelocity.magnitude;

		Vector3 orientationVelocity = BuildOrientationBehaviorVelocity(neighborDetector);
		float orientationSpeed = orientationVelocity.magnitude;
		
		float naiveTotalSpeed = (
			avoidanceSpeed +
			cohesionSpeed +
			idlingSpeed + 
			orientationSpeed);

		// To avoid having the behaviors increase our speed purely by merit of their existence, we weight each
		// behavior by its requested speed versus the others. A single behavior is still able to request an increased
		// overall speed, which will also wind up "drowning out" the behaviors making more minor responses.
		// Consideration: The behavior functions could return an explicit weight in addition to the velocity, which would permit requests to slow down.
		Vector3 desiredVelocity = (
			(avoidanceVelocity * (avoidanceSpeed / naiveTotalSpeed)) +
			(cohesionVelocity * (cohesionSpeed / naiveTotalSpeed)) +
			(idlingVelocity * (idlingSpeed / naiveTotalSpeed)) +
			(orientationVelocity * (orientationSpeed / naiveTotalSpeed)));
		
		/*
		Debug.DrawLine(transform.position, (transform.position + avoidanceVelocity), Color.red);
		Debug.DrawLine(transform.position, (transform.position + cohesionVelocity), Color.blue);
		Debug.DrawLine(transform.position, (transform.position + idlingVelocity), Color.grey);
		Debug.DrawLine(transform.position, (transform.position + orientationVelocity), Color.green);
		Debug.DrawLine(transform.position, (transform.position + desiredVelocity), Color.white);
		*/

		return desiredVelocity;
	}

	private Vector3 BuildIdlingBehaviorVelocity ()
	{
		return (IdlingSpeed * (IdlingLocalOrientation * transform.forward));
	}

	private Vector3 BuildOrientationBehaviorVelocity (
		FlockingNeighborDetector neighborDetector)
	{
		Vector3 summedNeighborVelocity = Vector3.zero;
		float summedNeighborConsiderationFractions = 0.0f;

		foreach (var neighbor in neighborDetector.Neighbors)
		{
			var neighborFlightController = neighbor.GetComponent<FlightController>();

			if (neighborFlightController != null)
			{
				float neighborCurrentSpeed = IdlingSpeed; // neighborFlightController.CurrentSpeed; // BUG! Boids were feeding back into each other and creating explosive velocities, so for now we can't read CurrentSpeed for this.

				// Blend in our consideration of the neighbor so as to avoid twitching when they're first sighted.
				float neighborConsiderationFraction = neighborDetector.GetNeighborConsiderationFraction(neighbor);

				Vector3 neighborActualVelocity = (neighborCurrentSpeed * neighbor.transform.forward);

				Vector3 neighborWeightedVelocity = (neighborConsiderationFraction * neighborActualVelocity);

				summedNeighborVelocity += neighborWeightedVelocity;
				summedNeighborConsiderationFractions += neighborConsiderationFraction;
			}
		}

		Vector3 meanNeighborVelocity = (summedNeighborVelocity / Mathf.Max(summedNeighborConsiderationFractions, 1.0f));

		return meanNeighborVelocity;
	}
}


