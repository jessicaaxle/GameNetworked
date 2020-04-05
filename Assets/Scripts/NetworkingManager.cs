using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public struct CS_to_Plugin_Functions
{
    public IntPtr MsgReceivedPtr;

    // The functions don't need to be the same though
    // Init isn't in C++
    public bool Init()
    {
        MsgReceivedPtr = Marshal.GetFunctionPointerForDelegate(new Action<IntPtr>(NetworkingManager.MsgReceived));

        return true;
    }
}

public class ClientConnectionData
{
    public string name;
    public string status;
    public int id;
    public UserElement go;
    public bool inGame = false;
    public Dumb playerObj;
    public int material;
    public Vector3 lastUpdatePos = new Vector3(0f, 0f, 0f);
}

public class MsgToPopulate
{
    public string msg;
    public int id;
}

public class NetworkingManager : MonoBehaviour
{
    // Same old DLL init stuff
    private const string path = "/Plugins/NetworkingTutorialDLL.dll";

    private IntPtr Plugin_Handle;
    private CS_to_Plugin_Functions Plugin_Functions;

    public delegate void InitDLLDelegate(CS_to_Plugin_Functions funcs);
    public InitDLLDelegate InitDLL;

    public delegate void InitServerDelegate(string IP, int port);
    public InitServerDelegate InitServer;

    public delegate void InitClientDelegate(string IP, int port, string name);
    public InitClientDelegate InitClient;

    public delegate void SendPacketToServerDelegate(string msg);
    public SendPacketToServerDelegate SendPacketToServer;

    public delegate void CleanupDelegate();
    public CleanupDelegate Cleanup;

    //MOVEMENT STUFF
    public delegate void SendPacketToServerDelegateMove(string msg);
    public SendPacketToServerDelegateMove SendPacketToServerMove;


    //Shooting Stuff
    //public GameObject ball;
    //public Transform spawn;
    //public float thrust;


    // MUST be called before you call any of the DLL functions
    private void InitDLLFunctions()
    {
        InitDLL = ManualPluginImporter.GetDelegate<InitDLLDelegate>(Plugin_Handle, "InitDLL");
        InitServer = ManualPluginImporter.GetDelegate<InitServerDelegate>(Plugin_Handle, "InitServer");
        InitClient = ManualPluginImporter.GetDelegate<InitClientDelegate>(Plugin_Handle, "InitClient");
        SendPacketToServer = ManualPluginImporter.GetDelegate<SendPacketToServerDelegate>(Plugin_Handle, "SendPacketToServer");
        Cleanup = ManualPluginImporter.GetDelegate<CleanupDelegate>(Plugin_Handle, "Cleanup");
    }

    // Fields we need later
    public GameObject textboxPrefab;
    public GameObject textboxParent;
    public GameObject userPrefab;
    public GameObject userParent;
    public InputField textinput;
    public InputField nameInput;

    public GameObject player;
    public GameObject player2;

   

    public static int sceneID = 1;
    public static float x = 0;
    public static float z =0;
    public static string msg;
    //
    //public GameObject playerPrefab;
    //public GameObject pawnPrefab;
    //public Transform pawnParent;

    private static bool mutex = false;
    static List<MsgToPopulate> msgs = new List<MsgToPopulate>();
    static List<ClientConnectionData> clients = new List<ClientConnectionData>();
    static ClientConnectionData user = new ClientConnectionData();

    private float mutexCounter = 0;
    private float activityCounter = 0;
    private static NetworkingManager Instance;

    private void Start()
    {
        
    }
    // Init the DLL
    private void Awake()
    {
        if(Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Plugin_Handle = ManualPluginImporter.OpenLibrary(Application.dataPath + path);
        Plugin_Functions.Init();

        InitDLLFunctions();

        InitDLL(Plugin_Functions);

    }

    private void Update()
    {
        if (sceneID == 1)
        {
            //Debug.Log("CHAT");
            mutexCounter += Time.fixedDeltaTime;
            activityCounter += Time.fixedDeltaTime;
       
            if (mutexCounter >= 0.5f)
            {
                mutex = true;
       
                UpdateData();
       
                mutex = false;
            }
       
            if (activityCounter >= 10.0f)
            {
                user.status = "IDLE";
                SendPacketToServer("s;" + user.id.ToString() + ";IDLE");
                activityCounter = 0;
            }
       
            if (Input.GetKeyDown(KeyCode.Return))
            {
                SendCurrentMessage();
            }
        }
       
        if (sceneID == 2)
        {
            UpdateMove();
           // keyboard(player);
            SendCurrPos();
        }
    }

    // Update client and message data
    private void UpdateData()
    {
        if (msgs.Count > 0)
        {
            for (int i = msgs.Count - 1; i >= 0; i--)
            {
                GameObject go = Instantiate(textboxPrefab, textboxParent.transform);
                for (int j = 0; j < clients.Count; j++)
                {
                    if (clients[j].id == msgs[i].id)
                    {
                        go.GetComponent<TextElement>().UpdateText(clients[j].name, msgs[i].msg);

                        break;
                    }
                }
                msgs.Remove(msgs[i]);
            }
        }

        if (clients.Count > 0)
        {
            for (int i = 0; i < clients.Count; i++)
            {
                if (clients[i].go == null)
                {
                    clients[i].go = Instantiate(userPrefab, userParent.transform).GetComponent<UserElement>();
                }
                if(i ==0)
                {
                    clients[i].go.UpdateUser(user.name, user.status);
                }
                else
                {
                    clients[i].go.UpdateUser(clients[i].name, clients[i].status);
                }
            }
        }
    }
    public void UpdateMove()
    {
        //Debug.Log("HI"); 
        for (int i = 0; i < clients.Count; i++)
        {
           if (clients[i].playerObj == null)
           {
               if(i==0)
               {
                   clients[0].playerObj = Instantiate(player).GetComponent<Dumb>();

               }
               else
               {
                   Debug.Log("i am player2");
                   //if(clients[i].material == 1)
                   if(i!=0)
                    {
                        clients[i].playerObj = Instantiate(player2).GetComponent<Dumb>();

                    }
               }

           }
           else
           {
               clients[i].playerObj.DeadReckonPosition();
           }
        }

       // velocity = currentPos - prevPos;
       // futurePos = currentPos + (velocity * UPDATE_INTERVAL);
       //
       // p.transform.position = Vector3.Lerp(gameObject.transform.position, futurePos,Time.deltaTime);

    }
    // Init the server
    public void StartServer()
    {
        InitServer("127.0.0.1", 54000);
    }

    // Init the client
    public void StartClient()
    {
        InitClient("127.0.0.1", 54000, nameInput.text);
        user.name = nameInput.text;
    }
    public void startGame()
    {
        SceneManager.LoadScene("Game");
        sceneID++;
        clients[0].inGame = true;
        
    }

    // Where we'll process incoming messages
    public static void MsgReceived(IntPtr p_in)
    {
        string p = Marshal.PtrToStringAnsi(p_in);
        Debug.Log("RECEIVED: " + p);

        while (mutex)
        { } // wait 
        // Look up mutex, semaphores

        switch (p[0])
        {
            // Got an ID packet
            case 'i':
                {
                    string[] ar = p.Split(';');
                    user.id = int.Parse(ar[1]);

                    ClientConnectionData temp = new ClientConnectionData();

                    temp.name = user.name;
                    temp.status = user.status;
                    temp.id = user.id;

                    clients.Add(temp);
                    // may want to use TryParse

                    break;
                }
            case 'c': // c;NAME;STATUS;00
                {
                    ClientConnectionData temp = new ClientConnectionData();
                    string[] ar = p.Split(';');

                    temp.name = ar[1];
                    temp.status = ar[2];
                    temp.id = int.Parse(ar[3]);

                    clients.Add(temp);

                    break;
                }
            case 's': // s;00;STATUS
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);

                    for (int i = 0; i < clients.Count; i++)
                    {
                        if (clients[i].id == id)
                        {
                            clients[i].status = ar[2];

                            return;
                        }
                    }

                    break;
                }
            case 'm':
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    string msg = ar[2];

                    MsgToPopulate msgp = new MsgToPopulate();
                    msgp.msg = msg;
                    msgp.id = id;
                    msgs.Add(msgp);

                    break;
                }
            case 'q':
                {
                    string[] ar = p.Split(';');
                    int id = int.Parse(ar[1]);
                    x = float.Parse(ar[2]);
                    z = float.Parse(ar[3]);


                   //UpdateMove()
                   for (int i =0; i<clients.Count; i++)
                   {
                       if(clients[i].id == id)
                       {
                            if (!clients[i].inGame)
                            {
                                clients[i].inGame = true;
                            }
                            else
                            {
                                if(clients[i].playerObj != null)
                                {
                                   clients[i].playerObj.prevPos = clients[i].playerObj.currPos;
                                   clients[i].playerObj.currPos = new Vector3(x, 0.0f, z);

                                    
                                }
                            }
                       }
                   }
                    break;
                }
           //case 'x':
           //    {
           //        string[] ar = p.Split(';');
           //        int id = int.Parse(ar[1]);
           //        int matID = int.Parse(ar[2]);
           //        for (int i = 0; i < clients.Count; i++)
           //        {
           //            if (clients[i].id == id)
           //            {
           //                clients[i].material = matID; 
           //            }
           //        }
           //        break;
            //  }
        }

    }

  public void SendCurrPos()
  {
        // string msg;
        // msg = player.transform.position.x.ToString() + "@" + player.transform.position.z.ToString();
        //Debug.Log("SEND MOVE");
        //SendPacketToServerMove("v;" + player.transform.position.x.ToString() + ";" + player.transform.position.z.ToString()) ;
        msg = "q;" + user.id.ToString()+ ";" + clients[0].playerObj.transform.position.x.ToString() + ";" + clients[0].playerObj.transform.position.z.ToString();
       // Debug.Log(msg);
        SendPacketToServer(msg);
  }
    public void sendPlayerMat()
    {
        //SendPacketToServer("x;" + user.id.ToString() + ";" + );
    }
    public void SendCurrentMessage()
    {
        MsgToPopulate msgp = new MsgToPopulate();
        msgp.msg = textinput.text;
        msgp.id = user.id;
        msgs.Add(msgp);

        user.status = "CHATTING";
        SendPacketToServer("m;" + user.id.ToString() + ";" + textinput.text);
        SendPacketToServer("s;" + user.id.ToString() + ";CHATTING");
        textinput.text = "";

        activityCounter = 0;
    }

    private void OnApplicationQuit()
    {
        SendPacketToServer("s;" + user.id.ToString() + ";OFFLINE");
        Cleanup();
        ManualPluginImporter.CloseLibrary(Plugin_Handle);
    }
}
