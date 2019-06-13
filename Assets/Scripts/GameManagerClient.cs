using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManagerClient : MonoBehaviour
{
    public GameObject Car;

    public bool localIsServer;
    public UnityEngine.UI.Text GameTimerText;

    private CarBehaviour carBehaviour;
    public bool gameStarted = false;
    private float gameTimer = 0.0f;
    private float blindTime = 3.0f;
    public GameObject blindText;

    public GameObject WinCanvas;
    public GameObject LossCanvas;
    public GameObject BlindPanel;

    public GameObject triggerPlayerOne;
    public GameObject triggerPlayerTwo;
    public bool sentSessionId;

    public GameObject PreGameCamera;
    private bool driveTurn = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
