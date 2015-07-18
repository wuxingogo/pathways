var speed = 6.0;
var gravity = 20.0;
var circularMovementRadius = 3.0;

var move = true;
private var moveDirection = Vector3.zero;
private var circularMoveN = 0.0;

function IsMoving()
{
	return moveDirection.x != 0 || moveDirection.z != 0;
}

function FixedUpdate()
{
	var controller : CharacterController = GetComponent(CharacterController);
	if (controller.isGrounded)
	{
		// We are grounded, so recalculate
		// move direction directly from axes
		if (circularMoveN <= 2*Mathf.PI)
		{
			circularMoveN += 0.1;
		}
		else
		{
			circularMoveN -= 2*Mathf.PI;
		}
		if (move)
		{
			moveDirection = Vector3(Mathf.Cos(circularMoveN) * circularMovementRadius, 0, Mathf.Sin(circularMoveN) * circularMovementRadius);
			moveDirection = transform.TransformDirection(moveDirection);
			moveDirection *= speed;
		}
	}

	// Apply gravity
	moveDirection.y -= gravity * Time.deltaTime;

	// Move the controller
	controller.Move(moveDirection * Time.deltaTime);
}
