using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (GameManager.Instance is GameManagerServer) {
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.EggHit, other.gameObject == ((GameManagerServer)GameManager.Instance).TriggerPlayerOne), ServerBehaviour.GetConnectionByPlayerNr(0));
                ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.EggHit, !(other.gameObject == ((GameManagerServer)GameManager.Instance).TriggerPlayerOne)), ServerBehaviour.GetConnectionByPlayerNr(1));
            }
        }
    }
}
