using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingNeighborDetector : MonoBehaviour
{
	public List<FlockingDesires> Neighbors { get; private set; }

	public void Awake ()
	{
		Neighbors = new List<FlockingDesires>();
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

			Debug.LogFormat("object:{0} added object:{1} to the neighbor-list (count:{2}).", transform.parent.name, otherFlockingDesires.transform.name, Neighbors.Count);
		}
	}

	public void OnTriggerExit(
		Collider other)
	{
		var otherFlockingDesires = other.GetComponent<FlockingDesires>();

		if (otherFlockingDesires != null)
		{
			Neighbors.Remove(otherFlockingDesires);

			Debug.LogFormat("object:{0} removed object:{1} from the neighbor-list (count:{2}).", transform.parent.name, otherFlockingDesires.transform.name, Neighbors.Count);
		}
	}
}
