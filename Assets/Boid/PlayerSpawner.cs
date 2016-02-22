using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
	public GameObject vanillaBoidPrefab = null;
	public GameObject playerParasitePrefab = null;

	public void Start ()
	{
		if (vanillaBoidPrefab == null)
		{
			Debug.LogError("'vanillaBoidPrefab' is null.");
		}
		else if (playerParasitePrefab == null)
		{
			Debug.LogError("'playerParasitePrefab' is null.");
		}
		else
		{
			// Replace ourselves with the new player-controlled boid.

			var newBoidRoot = Instantiate(vanillaBoidPrefab, transform.position, transform.rotation) as GameObject;
			newBoidRoot.transform.parent = transform.parent;

			var newplayerParasite = Instantiate(playerParasitePrefab, transform.position, transform.rotation) as GameObject;
			newplayerParasite.transform.parent = newBoidRoot.transform;

			DestroyObject(this.gameObject);
		}
	}
	
	public void Update ()
	{
	}
}
