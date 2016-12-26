using UnityEngine;

[RequireComponent (typeof (Rigidbody))]
public class GravityBody : MonoBehaviour {
	
	public Planet planet;

	private Rigidbody rigidbody;
	
	void Start()
    {
		rigidbody = GetComponent<Rigidbody> ();

		rigidbody.useGravity = false;
		rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
	}
	
	void FixedUpdate()
    {
		planet.Attract(rigidbody);
	}
}