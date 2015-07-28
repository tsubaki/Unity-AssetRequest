using UnityEngine;
using System.Collections;

public class HitAndDestroy : MonoBehaviour 
{
	[SerializeField]
	Animator explosion;

	void OnTriggerEnter2D(Collider2D other) 
	{
		GameObject.Instantiate (explosion, transform.position, transform.rotation);
		//Destroy (gameObject);
		gameObject.SetActive (false);
	}

#if UNITY_EDITOR

	void Reset()
	{
		explosion = GameObject.Find ("Resources/Explosion").GetComponent<Animator>();
	}

#endif
}
