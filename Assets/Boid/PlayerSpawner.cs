using UnityEngine;
using System.Collections;

public class PlayerSpawner : MonoBehaviour
{
	public GameObject VanillaBoidPrefab = null;
	public GameObject PlayerParasitePrefab = null;

	public void Start ()
	{
		// Replace ourselves with the new player-controlled boid.

		var newBoidRoot = Instantiate(VanillaBoidPrefab, transform.position, transform.rotation) as GameObject;
		newBoidRoot.transform.parent = transform.parent;

		var newplayerParasite = Instantiate(PlayerParasitePrefab, transform.position, transform.rotation) as GameObject;
		newplayerParasite.transform.parent = newBoidRoot.transform;

		DestroyObject(this.gameObject);
	}
	
	public void Update ()
	{
	}
}
