using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameLoop : NetworkBehaviour
{

    public static bool isMyTurn;

    public void Start()
    {
        isMyTurn = false;
    }

    private void Update()
    {
        //somehow start turn with 1 player
        if (Input.GetKeyDown(KeyCode.T))
        {
            nextPlayerServerRpc(true, new ServerRpcParams());
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void nextPlayerServerRpc(bool isFirstRound, ServerRpcParams serverRpcParams)
    {
        Debug.Log("hello, my id is: " + serverRpcParams.Receive.SenderClientId);
        int senderId = (int)serverRpcParams.Receive.SenderClientId;
        int nextId = senderId + 1;
        if (NetworkManager.Singleton.ConnectedClients.Count - 1 == senderId || isFirstRound)
        {
            nextId = 0;
        }
        nextPlayerClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { (ulong)nextId } } });
    }


    [ClientRpc]
    public void nextPlayerClientRpc(ClientRpcParams clientRpcParams)
    {
        isMyTurn = true;
        Debug.Log("yo bitch its my turn");
        //action 1
        //action 2
        //action 3
        isMyTurn = false;
        nextPlayerServerRpc(false, new ServerRpcParams());
    }
}
