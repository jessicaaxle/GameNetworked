using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public GameObject playerOne;
    public NetworkingManager NetworkingManager;

    string msg;

    // Start is called before the first frame update
    void Start()
    {
        NetworkingManager.StartClient();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W))
        {
            playerOne.transform.Translate(0.1f, 0.0f, 0.0f);
            msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
            NetworkingManager.sendCurrPos(msg);
        }
        if (Input.GetKey(KeyCode.S))
        {
            playerOne.transform.Translate(-0.1f, 0.0f, 0.0f);
            msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
            NetworkingManager.sendCurrPos(msg);
        }
        if (Input.GetKey(KeyCode.A))
        {
            playerOne.transform.Translate(0.0f, 0.0f, -0.1f);
            msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
            NetworkingManager.sendCurrPos(msg);
        }
        if (Input.GetKey(KeyCode.D))
        {
            playerOne.transform.Translate(0.0f, 0.0f, 0.1f);
            msg = ("v;" + playerOne.transform.position.x.ToString() + ";" + playerOne.transform.position.z.ToString());
            NetworkingManager.sendCurrPos(msg);
        }
    }
}
