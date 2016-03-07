using UnityEngine;
using System.Collections;

public class BoidSpawner : MonoBehaviour
{
	public GameObject BoidPrefab = null;

	public int InitialSpawnCount = 50;
	public float InitialSpawnBoidsPerSecond = 10.0f;

	[Range(0.0f, 180.0f)]
	public float SpreadHalfYawDegrees = 0.0f;
	
	[Range(0.0f, 90.0f)]
	public float SpreadHalfPitchDegrees = 0.0f;

	[Range(0.0f, 180.0f)]
	public float SpreadHalfRollDegrees = 0.0f;

	public float PlacementDistanceMin = 0.0f;
	public float PlacementDistanceMax = 0.0f;

	public void Start ()
	{
		remainingInitialBoidSpawnCount = InitialSpawnCount;
	}
	
	public void Update ()
	{
		remainingSecondsUntilNextBoidSpawn -= Time.deltaTime;

		while ((remainingInitialBoidSpawnCount > 0) &&
			(remainingSecondsUntilNextBoidSpawn <= 0.0f))
		{
			SpawnBoid();

			remainingInitialBoidSpawnCount--;
			remainingSecondsUntilNextBoidSpawn += (1.0f / InitialSpawnBoidsPerSecond);
		}

		// If we're no longer needed, answer the call of the void.
		if (remainingInitialBoidSpawnCount == 0)
		{
			DestroyObject(this.gameObject);
		}
	}

	private int remainingInitialBoidSpawnCount = 0;
	private float remainingSecondsUntilNextBoidSpawn = 0.0f;

	private void SpawnBoid ()
	{
		Quaternion newBoidOrientation = (transform.rotation * BuildRandomLocalSpawnOrientation());
		
		float newBoidDistance = Random.Range(PlacementDistanceMin, PlacementDistanceMax);
		Vector3 newBoidPosition = (transform.position + (newBoidDistance * (newBoidOrientation * Vector3.forward)));

		var newBoidRoot = Instantiate(BoidPrefab, newBoidPosition, newBoidOrientation) as GameObject;
		newBoidRoot.transform.parent = transform.parent;
	}

	private Quaternion BuildRandomLocalSpawnOrientation ()
	{
		float randomYaw = Random.Range(-SpreadHalfYawDegrees, SpreadHalfYawDegrees);

		// From http://mathworld.wolfram.com/SpherePointPicking.html, we need curve the generated 
		// pitches towards the equator to avoid clustering the generated points near the poles.
		float randomPitch;
		{
			float randomHeight = Random.Range(-(SpreadHalfPitchDegrees / 90), (SpreadHalfPitchDegrees / 90));

			randomPitch = (Mathf.Asin(Mathf.Clamp(randomHeight, -1.0f, 1.0f)) * Mathf.Rad2Deg);
		}

		float randomRoll = Random.Range(-SpreadHalfRollDegrees, SpreadHalfRollDegrees);

		Quaternion randomOrientation = Quaternion.Euler(randomPitch, randomYaw, randomRoll);

		return randomOrientation;

	}
}

