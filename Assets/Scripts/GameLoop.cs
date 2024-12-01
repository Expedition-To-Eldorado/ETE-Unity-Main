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

    public static GameLoop Instance { get; private set; }
    [SerializeField] public static bool isMyTurn;
    [SerializeField] public static Phase PlayerPhase;
    [SerializeField] private Button nextPhaseButton;
    [SerializeField] private Button discardCardsBtn;
    public static Action<int> drawFullHand;
    [SerializeField] private GameObject YouWonTxt;
    public bool CanStartGame;
    [SerializeField] private GameObject PhaseTxt;
    public static TMP_Text PhaseTxtComponent;
    public DeckManager deckManager;

    public void Start()
    {
        Instance = this;
        isMyTurn = false;
        CanStartGame = false;
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
        if (PlayerPhase == Phase.DISCARD_PHASE) {
            discardCardsBtn.gameObject.SetActive(true); 
        }
        else {
            discardCardsBtn.gameObject.SetActive(false);
        }

        deckManager.cancelCardExecution();
        updateText();
    }

    public static void updateText()
    {

        if (!isMyTurn)
        {
            PhaseTxtComponent.text = "NOT YOUR TURN";
        }
        else
        {
            string message = "none";
            switch (PlayerPhase)
            {
                case Phase.MOVEMENT_PHASE:
                    message = "MOVEMENT PHASE";
                    break;
                case Phase.BUYING_PHASE:
                    message = "BUYING PHASE";
                    break;
                case Phase.DISCARD_PHASE:
                    message = "DISCARD PHASE";
                    break;
                case Phase.GAME_ENDED:
                    message = "GAME ENDED";
                    break;
                case Phase.GAME_WON:
                    message = "GAME WON";
                    break;
                default:
                    Debug.Log("something went wrong");
                    message = "error";
                    break;
            }
            PhaseTxtComponent.text =  message;
        }

        if (PlayerPhase == Phase.GAME_ENDED)
        {
            PhaseTxtComponent.text = "GAME ENDED";
        }
    }

    private void Update()
    {
        if(PlayerPhase == Phase.GAME_WON)
        {
            YouWonTxt.SetActive(true);
        }

        if(PlayerPhase == Phase.GAME_ENDED)
        {
            isMyTurn = false;
        }
        else if (isMyTurn && PlayerPhase == Phase.GAME_WON)
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
        if (CanStartGame)
        {
            nextPlayerServerRpc(true, new ServerRpcParams());
            updateText();
            CanStartGame = false;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void nextPlayerServerRpc(bool isFirstRound, ServerRpcParams serverRpcParams)
    {
        if(PlayerPhase == Phase.GAME_ENDED)
        {
            return;
        }

        Debug.Log("hello, my id is: " + serverRpcParams.Receive.SenderClientId);
        int senderId = (int)serverRpcParams.Receive.SenderClientId;
        int nextId = senderId + 1;
        if (NetworkManager.Singleton.ConnectedClients.Count - 1 == senderId || isFirstRound)
        {
            nextId = 0;
        }
        nextPlayerClientRpc(new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { (ulong)nextId } } });
        ShowActivePlayerClientRpc(nextId);
    }


    [ClientRpc]
    public void nextPlayerClientRpc(ClientRpcParams clientRpcParams)
    {
        isMyTurn = true;
        if (PlayerPhase != Phase.GAME_WON)
        {
            PlayerPhase = Phase.MOVEMENT_PHASE;
        }
        updateText();
        Debug.Log("It is my turn");
    }

    [ClientRpc]
    public void ShowActivePlayerClientRpc(int activePlayer)
    {
        PlayerBoardUI.Instance.ChangeActivePlayer(activePlayer);
    }
}
