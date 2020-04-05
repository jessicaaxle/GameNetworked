using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dumb : MonoBehaviour
{
    public int material;
    public Vector3 prevPos = new Vector3(0.0f, 0.0f, 0.0f);
    public Vector3 currPos = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 futurePos;
    private Vector3 velocity;
    float UPDATE_INTERVAL = 0.2f;

    public void DeadReckonPosition()
    {
        velocity = currPos - prevPos;
        futurePos = currPos + (velocity * UPDATE_INTERVAL);

        gameObject.transform.localPosition = Vector3.Lerp(gameObject.transform.localPosition, futurePos, Time.deltaTime);
    }
}
