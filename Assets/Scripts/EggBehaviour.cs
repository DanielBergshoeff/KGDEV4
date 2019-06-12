using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EggBehaviour : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.EggHit, other.gameObject == GameManager.Instance.triggerPlayerOne), ServerBehaviour.GetConnectionByPlayerNr(0));
            ServerBehaviour.SendInfo(ServerBehaviour.WriteInfo(SendType.EggHit, !other.gameObject == GameManager.Instance.triggerPlayerOne), ServerBehaviour.GetConnectionByPlayerNr(1));
        }
    }
}
