using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    //Camera Stuff
    public Transform playerTransform;

    private Vector3 camOffset;

    [Range(0.01f, 1.0f)]
    public float smoothFactor = 0.5f;

    public bool lookAtPlayer = false;

    public bool rotateAroundPlayer = true;

    public float rotationSpeed = 5.0f;

    // Start is called before the first frame update
    void Start()
    {
        camOffset = transform.position - playerTransform.position;
    }

    void LateUpdate()
    {
        if (rotateAroundPlayer)
        {
            Quaternion camTurnAngleX = Quaternion.AngleAxis(Input.GetAxis("Mouse X") * rotationSpeed, Vector3.up);
            Quaternion camTurnAngleY = Quaternion.AngleAxis(Input.GetAxis("Mouse Y") * rotationSpeed, Vector3.right);

            camOffset = camTurnAngleX * camTurnAngleY * camOffset;
        }

        Vector3 newPos = playerTransform.position + camOffset;

        transform.position = Vector3.Slerp(transform.position, newPos, smoothFactor);

        if (lookAtPlayer || rotateAroundPlayer)
        {
            transform.LookAt(playerTransform);
        }
    }
}
