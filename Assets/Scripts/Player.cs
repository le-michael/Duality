using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Player : NetworkBehaviour
{
    public CharacterController controller;
    public Transform groundCheck;
    public float speed = 14f;
    public float gravity = -9.81f;
    public float groundDistance = 0.4f;
    public float jumpHeight = 3;
    public LayerMask groundMask;
    Vector3 velocity;
    public float mouseSensitivity = 200f;
    float xRotation = 0f;

    public GameObject camera;
    public NetworkVariable<Vector3> networkPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<Quaternion> networkQuaternion = new NetworkVariable<Quaternion>();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        camera.SetActive(IsOwner);
        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frames
    void Update()
    {
        if (IsOwner)
        {
            walk();
            jump();
            fall();
            lookAround();
            if (NetworkManager.Singleton.IsServer)
            {
                networkPosition.Value = transform.position;
                networkQuaternion.Value = transform.rotation;
            }
            else
            {
                submitPositionServerRpc(transform.position, transform.rotation);
            }
        }
        else
        {
            transform.position = networkPosition.Value;
            transform.rotation = networkQuaternion.Value;
        }
    }

    [ServerRpc]
    void submitPositionServerRpc(Vector3 position, Quaternion rotation)
    {
        networkPosition.Value = position;
        networkQuaternion.Value = rotation;
    }

    private void walk()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);
    }

    private void jump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded())
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void fall()
    {

        if (isGrounded() && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

    }

    private void lookAround()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        transform.Rotate(Vector3.up * mouseX);

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        camera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private bool isGrounded()
    {
        return Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }
}
