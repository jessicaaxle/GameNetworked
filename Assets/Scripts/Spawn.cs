using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public Vector3 previousPos = new Vector3(0f, 0f, 0f);
    public Quaternion previousRot = Quaternion.identity;
    public Vector3 currentPos = new Vector3(0f, 0f, 0f);
    public Quaternion currentRot = Quaternion.identity;
    private Vector3 futurePos;
    private Vector3 velocity;
    public void UpdateTransform(float UpdateInterval)
    {
        velocity = currentPos - previousPos;
        futurePos = currentPos + (velocity * UpdateInterval);

        gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, futurePos, Time.deltaTime);
    }
}
