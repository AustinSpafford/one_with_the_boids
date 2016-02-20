using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingDesires : MonoBehaviour
{
	public float VisibilityRadius = 10.0f;

	public void Start ()
	{
		UpdateNeighbors();
	}

	public void Update ()
	{
		// TODO Keep the neighbors list trimmed down to just those within our visibility radius.
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

	private List<FlockingDesires> neighbors = new List<FlockingDesires>();

	private void UpdateCache ()
	{
		if (cacheConditionFrameIndex != Time.frameCount)
		{
			// TODO
			cachedDesiredOrientation = transform.localRotation;
			cachedDesiredSpeed = 2.0f;

			cacheConditionFrameIndex = Time.frameCount;
		}
	}

	private void UpdateNeighbors ()
	{
		// TODO
	}
}


