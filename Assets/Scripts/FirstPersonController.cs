using UnityEngine;
using System.Collections;

[RequireComponent (typeof (GravityBody))]
public class FirstPersonController : MonoBehaviour
{
	//--Main Stuff--
	public float mouseSensitivityX;
	public float mouseSensitivityY;
	public float walkSpeed;
	public float jumpForce;
	public LayerMask groundedMask;
	
	//--Private Stuff--
	private bool grounded;
    private Vector3 moveAmount;
    private Vector3 smoothMoveVelocity;
    private float verticalLookRotation;
    private Transform cameraTransform;
    private Rigidbody rigidbody;
	
	void Start()
    {
		cameraTransform = Camera.main.transform;
		rigidbody = GetComponent<Rigidbody> ();
	}
	
	void Update()
    {
        if(Input.GetKeyDown(KeyCode.L))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

		transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * mouseSensitivityX);
		verticalLookRotation += Input.GetAxis("Mouse Y") * mouseSensitivityY;
		verticalLookRotation = Mathf.Clamp(verticalLookRotation,-90,90);
		cameraTransform.localEulerAngles = Vector3.left * verticalLookRotation;
		
		float inputX = Input.GetAxisRaw("Horizontal");
		float inputY = Input.GetAxisRaw("Vertical");
		
		Vector3 moveDir = new Vector3(inputX,0, inputY).normalized;
		Vector3 targetMoveAmount = moveDir * walkSpeed;
		moveAmount = Vector3.SmoothDamp(moveAmount,targetMoveAmount,ref smoothMoveVelocity,.15f);
		
		if (Input.GetButtonDown("Jump")) {
			if (grounded) {
				rigidbody.AddForce(transform.up * jumpForce);
			}
		}

		Ray ray = new Ray(transform.position, -transform.up);
		RaycastHit hit;
		
		if (Physics.Raycast(ray, out hit, 1 + .1f, groundedMask)) {
			grounded = true;
		}
		else {
			grounded = false;
		}
		
	}
	
	void FixedUpdate()
    {
		Vector3 localMove = transform.TransformDirection(moveAmount) * Time.fixedDeltaTime;
		rigidbody.MovePosition(rigidbody.position + localMove);
	}
}
