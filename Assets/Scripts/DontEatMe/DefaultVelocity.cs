using UnityEngine;
using System.Collections;

public class DefaultVelocity : MonoBehaviour {

	// The Velocity (can be set from Inspector)
	public Vector2 velocity;

	void FixedUpdate() {
		rigidbody2D.velocity = velocity;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
