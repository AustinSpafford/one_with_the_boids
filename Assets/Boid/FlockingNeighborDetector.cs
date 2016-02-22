using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingNeighborDetector : MonoBehaviour
{
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

	private SphereCollider triggerCollider = null;
}
