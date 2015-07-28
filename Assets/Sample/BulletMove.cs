using UnityEngine;
using System.Collections;

public class BulletMove : MonoBehaviour {

	[SerializeField, HideInInspector]
	Rigidbody2D rigidbody2d;

	void Reset()
	{
		rigidbody2d = GetComponent<Rigidbody2D> ();
	}

	void Start () {
		rigidbody2d.velocity = transform.up.normalized * 8;
		Destroy (gameObject, 3);
	}
}
