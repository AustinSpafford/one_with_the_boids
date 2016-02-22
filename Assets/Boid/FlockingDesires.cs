using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FlockingDesires : MonoBehaviour
{
	public void Start ()
	{
		// Our creation has invalidated the global flocker's list.
		cachedAllFlockers = null;
	}

	public void Update ()
	{
		// This is an entirely reactive-component.
	}

	public void OnDestroy ()
	{
		// Our destruction has invalidated the global flocker's list.
		cachedAllFlockers = null;
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

	private static FlockingDesires[] cachedAllFlockers = null;

	private int cacheConditionFrameIndex = -1;
	private Quaternion cachedDesiredOrientation = Quaternion.identity;
	private float cachedDesiredSpeed = 0.0f;

	private void UpdateCache ()
	{
		// Regenerate the global-list of flockers if someone invalidated it.
		if (cachedAllFlockers == null)
		{
			cachedAllFlockers = Object.FindObjectsOfType<FlockingDesires>();
		}

		// If we haven't generated vectors for the current frame.
		if (cacheConditionFrameIndex != Time.frameCount)
		{
			// TODO Generate flocking desires!
			cachedDesiredOrientation = transform.rotation;
			cachedDesiredSpeed = 2.0f;

			cacheConditionFrameIndex = Time.frameCount;
		}
	}

	private void UpdateNeighbors ()
	{
		// TODO
	}
}


