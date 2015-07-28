using UnityEngine;
using System.Collections;

public class FighterMove : MonoBehaviour {

	[HideInInspector, SerializeField]
	Rigidbody2D rigidbody2d;

	[SerializeField]
	BulletMove bullet;

	[SerializeField, HideInInspector]
	Transform pos1, pos2;

	[SerializeField, Range(1, 10)]
	float speed = 10;

	private Vector2 boost = Vector2.zero;

	void Reset()
	{
		rigidbody2d = GetComponent<Rigidbody2D> ();

		bullet = GameObject.Find ("Resources/Bullet").GetComponent<BulletMove>();

		foreach( var name in new string[]{"Pos1", "Pos2"}){
			if (GameObject.Find ("Spaceship/" + name) == null) {
				var obj = new GameObject (name);
				obj.transform.SetParent (transform);
			}
		}
		pos1 = GameObject.Find ( gameObject.name + "/Pos1").transform;
		pos2 = GameObject.Find ( gameObject.name + "/Pos2").transform;
	}

	void Update () 
	{
		var directional = new Vector2 (Input.GetAxis ("Horizontal"), Input.GetAxis ("Vertical")).normalized;
		
		if (Input.GetButtonDown ("Fire1")) {
			Shot();
		}
		
		if (Input.GetButtonDown ("Jump")) {
			boost = directional * speed;
		}
		boost = Vector2.Lerp (boost, Vector2.zero, 0.1f);
		rigidbody2d.MovePosition (((Vector2)transform.position + (directional + boost) * Time.deltaTime ));
		Lookat ();
	}

	void Shot()
	{
		GameObject.Instantiate(bullet, pos1.position, transform.rotation);
		GameObject.Instantiate(bullet, pos2.position, transform.rotation);
	}

	void Lookat()
	{
		var targetPos = Camera.main.ScreenToWorldPoint( Input.mousePosition + Camera.main.transform.forward * 10 );
		var diff = (targetPos - transform.position ).normalized;
		transform.rotation = Quaternion.FromToRotation( Vector3.up,  diff);
	}
}
