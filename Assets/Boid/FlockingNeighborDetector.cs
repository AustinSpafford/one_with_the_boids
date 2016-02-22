using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingNeighborDetector : MonoBehaviour
{
	[Tooltip("When less than 1, neighbors on the fringe of our visibility radius will be smoothly blended into our behaviors as they reach the peak-visibility distance.")]
	public float NeighborFullVisibilityDistanceFraction = 0.8f;

	public List<FlockingDesires> Neighbors { get; private set; }

	public float VisionRadius
	{
		get { return triggerCollider.radius; }
	}

	public void Awake ()
	{
		Neighbors = new List<FlockingDesires>();

		triggerCollider = GetComponent<SphereCollider>();
	}

	public void Start ()
	{
	}
	
	public void Update ()
	{
	}

	public void OnTriggerEnter(
		Collider other)
	{
		var otherFlockingDesires = other.GetComponent<FlockingDesires>();

		if (otherFlockingDesires != null)
		{
			Neighbors.Add(otherFlockingDesires);
		}
	}

	public void OnTriggerExit(
		Collider other)
	{
		var otherFlockingDesires = other.GetComponent<FlockingDesires>();

		if (otherFlockingDesires != null)
		{
			Neighbors.Remove(otherFlockingDesires);
		}
	}

	public float GetNeighborConsiderationFraction (
		FlockingDesires neighbor)
	{
		float distanceToNeighborCenter = Vector3.Distance(transform.position, neighbor.transform.position);

		float distanceToNeighborSurface = Mathf.Max(0, (distanceToNeighborCenter - neighbor.GetComponent<SphereCollider>().radius));
		
		float neighborConsiderationFraction = 
			Mathf.Clamp01(Mathf.InverseLerp(
				VisionRadius, 
				(NeighborFullVisibilityDistanceFraction * VisionRadius), 
				distanceToNeighborSurface));

		return neighborConsiderationFraction;
	}

	private SphereCollider triggerCollider = null;
}
