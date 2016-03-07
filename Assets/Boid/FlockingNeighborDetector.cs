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
		cachedConsiderationFractionsAreValid = false;
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
			
			cachedConsiderationFractionsAreValid = false;
		}
	}

	public void OnTriggerExit(
		Collider other)
	{
		var otherFlockingDesires = other.GetComponent<FlockingDesires>();

		if (otherFlockingDesires != null)
		{
			neighborCollection.Remove(otherFlockingDesires);
			
			cachedConsiderationFractionsAreValid = false;
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
	
	private void UpdateCache ()
	{
		if (!cachedConsiderationFractionsAreValid)
		{
			foreach (var neighbor in neighborCollection.Values)
			{
				float distanceToNeighborCenter =
					Vector3.Distance(transform.position, neighbor.flockingDesires.transform.position);

				float distanceToNeighborSurface =
					Mathf.Max(0.0f, (distanceToNeighborCenter - neighbor.flockingDesires.GetComponent<SphereCollider>().radius));
		
				neighbor.currentConsiderationFraction = 
					Mathf.Clamp01(Mathf.InverseLerp(
						VisionRadius, 
						(NeighborFullVisibilityDistanceFraction * VisionRadius), 
						distanceToNeighborSurface));
			}

			cachedConsiderationFractionsAreValid = true;
		}
	}
}
