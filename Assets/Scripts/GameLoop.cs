using GeneralEnumerations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GameLoop : NetworkBehaviour
{

    public static bool isMyTurn;
    [SerializeField] public static Phase PlayerPhase;
    [SerializeField] private Button nextPhaseButton;
    public static Action<int> drawFullHand;
    [SerializeField] private GameObject YouWonTxt;
    [SerializeField] private GameObject PhaseTxt;
    private TMP_Text PhaseTxtComponent;

    public void Start()
    {
        isMyTurn = false;
    }

    public void OnEnable()
    {
        MouseController.instance.NextPhase += nextPhase;
    }

    public void OnDisable()
    {
        MouseController.instance.NextPhase -= nextPhase;
    }

    public void Awake()
    {
        PhaseTxtComponent = PhaseTxt.GetComponent<TMP_Text>();
        nextPhaseButton.onClick.AddListener(() =>
        {
            if (!isMyTurn)
            {
                return;
            }
            nextPhase();
        });
    }

    private void nextPhase()
    {
        PlayerPhase++;
        Debug.Log("Current phase: " + PlayerPhase);
        if (PlayerPhase >= Phase.FINAL_ELEMENT)
        {
            PlayerPhase = Phase.MOVEMENT_PHASE;
            isMyTurn = false;
            drawFullHand?.Invoke(0);
            nextPlayerServerRpc(false, new ServerRpcParams());
        }
        updateText();
    }

    private void updateText()
    {
        if (!isMyTurn)
        {
            PhaseTxtComponent.text = "Not your turn";
        }
        else
        {
            string message = "none";
            switch (PlayerPhase)
            {
                case Phase.MOVEMENT_PHASE:
                    message = "Movement Phase";
                    break;
                case Phase.BUYING_PHASE:
                    message = "Buying Phase";
                    break;
                case Phase.REDRAW_PHASE:
                    message = "Redraw Phase";
                    break;
                case Phase.FINAL_ELEMENT:
                    Debug.Log("something went wrong");
                    break;
            }
            PhaseTxtComponent.text = "Current phase is: " + message;
        }

        
    }

    private void Update()
    {
        if(PlayerPhase == Phase.GAME_WON)
        {
            YouWonTxt.SetActive(true);
        }

        if (isMyTurn && PlayerPhase == Phase.GAME_WON)
        {
            isMyTurn = false;
            nextPlayerServerRpc(false, new ServerRpcParams());
        }
        //somehow start turn with 1 player
        if (Input.GetKeyDown(KeyCode.T))
        {
            nextPlayerServerRpc(true, new ServerRpcParams());
            updateText();
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
        if(PlayerPhase != Phase.GAME_WON)
        {
            PlayerPhase = Phase.MOVEMENT_PHASE;
        }
        updateText();
        Debug.Log("It is my turn");
    }
}
