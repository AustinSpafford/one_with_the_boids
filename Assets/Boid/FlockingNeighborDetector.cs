using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingNeighborDetector : MonoBehaviour
{
	public class NeighborInformation
	{
		public FlockingDesires flockingDesires;

		public float currentConsiderationFraction;
	}

	[Tooltip("When less than 1, neighbors on the fringe of our visibility radius will be smoothly blended into our behaviors as they reach the peak-visibility distance.")]
	public float NeighborFullVisibilityDistanceFraction = 0.8f;

	public float CacheUpdatesPerSecondMin = 5.0f;
	public float CacheUpdatesPerSecondMax = 10.0f;

	public float VisionRadius
	{
		get { return triggerCollider.radius; }
	}

	public void Awake ()
	{
		neighborCollection = new Dictionary<FlockingDesires, NeighborInformation>();

		triggerCollider = GetComponent<SphereCollider>();
	}

	public void Start ()
	{
	}
	
	public void Update ()
	{
		remainingSecondsUntilScheduledCacheRebuild -= Time.deltaTime;

		if (remainingSecondsUntilScheduledCacheRebuild <= 0.0f)
		{
			cachedConsiderationFractionsAreValid = false;

			// Slightly randomize our update-schedule as a cheap/simple way to 
			// avoid spiking perf when many boids are spawned on the same initial frame.
			remainingSecondsUntilScheduledCacheRebuild = 
				(1.0f / Random.Range(CacheUpdatesPerSecondMin, CacheUpdatesPerSecondMax));
		}
	}

	public void OnTriggerEnter(
		Collider other)
	{
		var otherFlockingDesires = other.GetComponent<FlockingDesires>();

		if (otherFlockingDesires != null)
		{
			var newNeighbor = new NeighborInformation()
			{
				flockingDesires = otherFlockingDesires,
				currentConsiderationFraction = 0.0f,
			};

			neighborCollection.Add(newNeighbor.flockingDesires, newNeighbor);

			// NOTE: For total-correctness we'd immediately invalidate our cache here, but since
			// it's harmless for the additional neighbor to be temporarily present with a
			// rating of 0, we'll just wait until everyone gets updated.
		}
	}

	public void OnTriggerExit(
		Collider other)
	{
		var otherFlockingDesires = other.GetComponent<FlockingDesires>();

		if (otherFlockingDesires != null)
		{
			neighborCollection.Remove(otherFlockingDesires);
		}
	}
	
	public IEnumerable<NeighborInformation> GetNeighbors ()
	{
		UpdateCache();
			
		return neighborCollection.Values;
	}

	private Dictionary<FlockingDesires, NeighborInformation> neighborCollection = null;

	private SphereCollider triggerCollider = null;

	private bool cachedConsiderationFractionsAreValid = false;

	private float remainingSecondsUntilScheduledCacheRebuild = 0.0f;
	
	private void UpdateCache ()
	{
		if (!cachedConsiderationFractionsAreValid)
		{
			foreach (var neighbor in neighborCollection.Values)
			{
				float squaredDistanceToNeighborCenter =
					Vector3.SqrMagnitude(neighbor.flockingDesires.transform.position - transform.position);

				if (squaredDistanceToNeighborCenter < (NeighborFullVisibilityDistanceFraction * NeighborFullVisibilityDistanceFraction))
				{
					neighbor.currentConsiderationFraction = 1.0f;
				}
				else
				{
					float distanceToNeighborCenter = Mathf.Sqrt(squaredDistanceToNeighborCenter);

					float distanceToNeighborSurface =
						Mathf.Max(0.0f, (distanceToNeighborCenter - neighbor.flockingDesires.GetComponent<SphereCollider>().radius));
		
					neighbor.currentConsiderationFraction = 
						Mathf.Clamp01(Mathf.InverseLerp(
							VisionRadius, 
							(NeighborFullVisibilityDistanceFraction * VisionRadius), 
							distanceToNeighborSurface));
				}
			}

			cachedConsiderationFractionsAreValid = true;
		}
	}
}
