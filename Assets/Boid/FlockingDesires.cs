using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingDesires : MonoBehaviour
{
	public void Start ()
	{
	}

	public void Update ()
	{
		// This is an entirely reactive-component.
	}

	public void OnDestroy ()
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
			// TODO Generate flocking desires!
			cachedDesiredOrientation = transform.rotation;
			cachedDesiredSpeed = 2.0f;

			cacheConditionFrameIndex = Time.frameCount;
		}
	}
}


