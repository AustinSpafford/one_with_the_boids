using UnityEngine;
using System.Collections;

public class CounterRotator : MonoBehaviour
{
	public void Start ()
	{
	}
	
	public void Update ()
	{
	}

	public void OnCounteractableRotation (
		Quaternion oldAncestorRotation,
		Quaternion newAncestorRotation)
	{
		if (this.enabled)
		{
			Quaternion selfRotationInAncestorSpace = (Quaternion.Inverse(newAncestorRotation) * transform.rotation);

			Quaternion oldSelfRotationInWorldSpace = (oldAncestorRotation * selfRotationInAncestorSpace);
			
			// Immediately counteract the rotation.
			transform.rotation = oldSelfRotationInWorldSpace;
		}
	}
}
