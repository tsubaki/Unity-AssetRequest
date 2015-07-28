using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class FighterMove : MonoBehaviour {

	// cache
	[HideInInspector, SerializeField] 	Rigidbody2D rigidbody2d = null;
	[HideInInspector, SerializeField] 	Animator animator = null;

	// reference
	[SerializeField] 					BulletMove bullet = null;
	[SerializeField, HideInInspector] 	Transform pos1 = null, pos2 = null;

	// parameter
	[SerializeField, Range(1, 10)]		float speed = 10;

	// firld
	private Vector2 boost = Vector2.zero;

#region UnityCallback

	void Update () 
	{
		// Input
		var directional = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")).normalized;
		if (Input.GetButtonDown ("Fire1")) 	{ Shot(); }
		if (Input.GetButtonDown ("Jump")) 	{ boost = directional * speed;	}

		// Move
		boost = Vector2.Lerp (boost, Vector2.zero, 0.1f);
		rigidbody2d.MovePosition (((Vector2)transform.position + (directional + boost) * Time.deltaTime ));

		// Lookat
		Lookat ();
	}

#endregion

	void Shot()
	{
		var collider = GetComponent<Collider2D> ();
		var col1 = ((BulletMove)GameObject.Instantiate(bullet, pos1.position, transform.rotation)).GetComponent<Collider2D>();
		var col2 = ((BulletMove)GameObject.Instantiate(bullet, pos2.position, transform.rotation)).GetComponent<Collider2D>();
		Physics2D.IgnoreCollision (col1, collider);
		Physics2D.IgnoreCollision (col2, collider);
	}

	void Lookat()
	{
		var targetPos = Camera.main.ScreenToWorldPoint( Input.mousePosition + Camera.main.transform.forward * 10 );
		var diff = (targetPos - transform.position ).normalized;
		transform.rotation = Quaternion.FromToRotation( Vector3.up,  diff);
	}

	
#if UNITY_EDITOR
	[ContextMenu("Init")]
	void Reset()
	{
		rigidbody2d = GetComponent<Rigidbody2D> ();
		animator = GetComponent<Animator> ();
		
		bullet = GameObject.Find ("Resources/Bullet").GetComponent<BulletMove>();

		foreach( var name in new string[]{"Pos1", "Pos2"}){
			if (GameObject.Find ( gameObject.name + "/" + name) == null) {
				var obj = new GameObject (name);
				obj.transform.SetParent (transform);
			}
		}
		pos1 = GameObject.Find ( gameObject.name + "/Pos1").transform;
		pos2 = GameObject.Find ( gameObject.name + "/Pos2").transform;
	}
#endif
}
