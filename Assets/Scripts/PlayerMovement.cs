using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject player;
    //public static NetworkingManager NetworkingManager;

    public static string msg;

    //Shooting Stuff
    public GameObject ball;
    public Transform spawn;
    public float thrust;

    // Start is called before the first frame update
  

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            player.transform.Translate(0.1f, 0.0f, 0.0f);
            //msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
           // NetworkingManager.SendCurrPos(msg);
        }
        if (Input.GetKey(KeyCode.S))
        {
            player.transform.Translate(-0.1f, 0.0f, 0.0f);
           // msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
           // NetworkingManager.SendCurrPos(msg);
        }
        if (Input.GetKey(KeyCode.A))
        {
            player.transform.Translate(0.0f, 0.0f, -0.1f);
            //msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
            //NetworkingManager.SendCurrPos(msg);
        }
        if (Input.GetKey(KeyCode.D))
        {
            player.transform.Translate(0.0f, 0.0f, 0.1f);
            //msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
            //NetworkingManager.SendCurrPos(msg);
        }

        //Left click
        if (Input.GetMouseButtonDown(0))
        {
            GameObject clone;

            clone = (GameObject)Instantiate(ball, spawn.position, Quaternion.Euler(0f, 90f, 0f));

            clone.GetComponent<Rigidbody>().AddForce(transform.forward * thrust);
        }
    }
}
