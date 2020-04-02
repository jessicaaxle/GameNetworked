using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mimic : MonoBehaviour
{
    public GameObject player2;
    public NetworkingManager NetworkingManager;
    // Start is called before the first frame update
    void Start()
    {
        NetworkingManager.StartClient();

    }

    // Update is called once per frame
    void Update()
    {

    }
}
