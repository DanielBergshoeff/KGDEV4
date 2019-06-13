using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.Networking;


public class GameManager : MonoBehaviour
{
    //Both
    public static GameManager Instance;
    public static int myId;
    public GameObject EggPrefab;
    
    public GameObject Car;
    public UnityEngine.UI.Text GameTimerText;

    protected CarBehaviour carBehaviour;
    public bool gameStarted = false;
    protected float gameTimer = 0.0f;
    protected float blindTime = 3.0f;
    

    // Start is called before the first frame update
    protected void Start()
    {
        Instance = this;
        Communication.receivedObject = new UnityObjectEvent();
        Communication.receivedObject.AddListener(Receive);

        Communication.receivedObjects = new UnityObjectsEvent();
        Communication.receivedObjects.AddListener(Receive);

        carBehaviour = Car.GetComponent<CarBehaviour>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameTimer < 0.0f || gameStarted) {
            gameTimer += Time.deltaTime;
            GameTimerText.text = gameTimer.ToString("F2");
        }
    }
    
    public void StartGame() {
        gameStarted = true;
    }

    protected void BackToMenu() {
        MenuBehaviour.BackToMenu();
    }    

    protected virtual void Receive(SendType sendType, object o, NetworkConnection connection) { }
    protected virtual void Receive(SendType sendType, object[] values, NetworkConnection connection) { }
}


