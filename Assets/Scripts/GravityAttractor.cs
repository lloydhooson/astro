using UnityEngine;
using System.Collections;

public class GravityAttractor : MonoBehaviour {

    public int num, size;
	public float gravity = -9.8f;
	
	public void Attract(Rigidbody body) {
        int m = (num * size) / 2;
        Vector3 center = new Vector3(m, m, m);

		Vector3 gravityUp = (body.position - center).normalized;
		Vector3 localUp = body.transform.up;
		
		// Apply downwards gravity to body
		body.AddForce(gravityUp * gravity);
		// Allign bodies up axis with the centre of planet
		body.rotation = Quaternion.FromToRotation(localUp,gravityUp) * body.rotation;
	}  
}
