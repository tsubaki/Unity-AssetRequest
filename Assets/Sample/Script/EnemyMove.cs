using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyMove : MonoBehaviour {

	[SerializeField, HideInInspector]	Rigidbody2D rigidbody2d = null;
	[SerializeField]					Transform target = null;
	[SerializeField]					AnimationCurve curve = null;
	[SerializeField, Range(1, 5)]		float power = 3;
	[SerializeField, Range(1, 3)]		float interval = 1;

	void Update () 
	{
		var dx = transform.right * curve.Evaluate( (Time.timeSinceLevelLoad /interval) % 1 ) * power;
		rigidbody2d.MovePosition (transform.position + (transform.up + dx * power) * Time.deltaTime * -1);
	}

#if UNITY_EDITOR

	void Reset()
	{
		target = GameObject.FindWithTag ("Player").transform;
		rigidbody2d = GetComponent<Rigidbody2D> ();
	}

#endif
}
