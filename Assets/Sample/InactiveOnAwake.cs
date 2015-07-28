using UnityEngine;
using System.Collections;

public class InactiveOnAwake : MonoBehaviour {

	void Awake () 
	{
		gameObject.SetActive (false);
	}
}
