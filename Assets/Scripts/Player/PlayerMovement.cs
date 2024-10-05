// Description: This script controls the player's movement
// Can be adjusted in the training stage with experimental mode on
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController controller;
    [SerializeField] private float forceOfGravity = /* -9.81f */ -19.62f;
    [SerializeField] private float speed = 8f;

    private Vector3 velocity;
    private bool canMove = false;

    public float Speed
    {
        get { return speed; }
        set { speed = value; }
    }

    private bool isMoving;
    public bool IsMoving
    {
        get { return isMoving; }
        set { isMoving = value; }
    }

    void Start()
    {
        speed = PersistentDataManager.Instance.MovementSpeed;
    }

    void Update()
    {
        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        if (canMove)
        {
            Vector3 move = transform.right * xDirection + transform.forward * zDirection;

            // Time.deltaTime prevents movement speed from changing based on framerate
            Vector3 movementSpeed = move * speed * Time.deltaTime;
            controller.Move(movementSpeed);
        }

        ApplyGravity();

        // Move the player
        controller.Move(velocity * Time.deltaTime);

        // Check if the player is moving
        isMoving = (xDirection != 0 || zDirection != 0);
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += forceOfGravity * Time.deltaTime;
    }

    public void EnableMovement()
    {
        canMove = true;
    }

    public void DisableMovement()
    {
        canMove = false;
    }

    public void AdjustMovementSpeed(bool increase)
    {
        speed += increase ? 0.5f : -0.5f;
        PersistentDataManager.Instance.MovementSpeed = speed;
    }
}
