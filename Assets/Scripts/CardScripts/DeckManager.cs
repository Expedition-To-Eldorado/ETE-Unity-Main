using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using static HexGridMeshGenerator;
using System;
using Unity.VisualScripting;
using UnityEngine.UI;
using UnityEditor;
using System.Reflection;
using TMPro;

public class DeckManager : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField] public List<GameObject> cardsInDeck;
    [SerializeField] public List<GameObject> cardsOnHand;
    [SerializeField] public List<GameObject> usedCards;
    [SerializeField] float speedOfCard = 5;
    [SerializeField] double spaceBetweenCard = 5;
    public GameObject Deck;
    public GameObject[] starterCardPack = new GameObject[3];
    int viewNumber = (int)ViewTypes.CardsOnly;
    int cursor = -1;
    int selectedCursor = -1;
    [SerializeField] float selectedCursorOffsetValue = 0;
    [SerializeField] List<GameObject> multipleChosenCards;
    [SerializeField] private Button redrawCardsBtn;
    public GameObject InformationTxt;

    //cards to burn from special effect
    public int cardsToBurn;

    public static Action<RaycastHit> ExecuteSpecialEffect;

    public void Awake()
    {
        redrawCardsBtn.onClick.AddListener(() =>
        {
            if (GameLoop.PlayerPhase == Phase.REDRAW_PHASE)
            {
                clearMultipleChosenCards();
            }
        });
    }
    public int getNumberOfChosenCards()
    {
        return multipleChosenCards.Count;
    }

    public void clearMultipleChosenCards()
    {
        for (int i = 0; i < multipleChosenCards.Count; i++)
        {
            GameObject card = multipleChosenCards[i];
            card.tag = "Card_Used";
            card.SetActive(false);
            usedCards.Add(card);
            int index = findIndexOfCard(card);
            cardsOnHand.RemoveAt(index);
        }
        multipleChosenCards.Clear();
    }

    public int getSumOfCoins()
    {
        float sum = 0;
        foreach(var card in multipleChosenCards)
        {
            CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
            float cardCoins = cardBehaviour.Power;
            float coins = 0;
            if (cardBehaviour.Typ == "Village" || cardBehaviour.Typ == "All")
            {
                coins = cardCoins;
            }
            else
            {
                coins = 0.5f;
            }
            sum += coins;
        }

        return (int) sum;
    }

    public GameObject getSelectedCard()
    {
        if ( selectedCursor !=  -1 )
        {
            return cardsOnHand[selectedCursor];
        }
        return null;
    }

    // Start is called before the first frame update
    void Start()
    {
        createStarterDeck();
        drawFullHand(0);
    }

    private void OnEnable()
    {
        MouseController.instance.SetCursor += SetCursor;
        CameraBehaviour.changeView += changeView;
        ShopBehaviour.AddCardToDeck += AddCardToDeck;
        MouseController.instance.SetSelectedCursor += SetSelectedCursor;
        MouseController.instance.SetMultipleCursor += SetMultipleCursor;
        GameLoop.drawFullHand += drawFullHand;
        PlayerNetwork.clearMultipleChosenCards += clearMultipleChosenCards;
        PlayerNetwork.burnMultipleCards += burnMultipleCards;
        PlayerNetwork.UseCard += UseCard;
        CardBehaviour.drawCard += drawCard;
        CardBehaviour.useCard += UseCard;
        CardBehaviour.specialEffectBurn += specialEffectBurn;
        CardBehaviour.burnCard += burnCard;
    }

    private void OnDisable()
    {
        MouseController.instance.SetCursor -= SetCursor;
        CameraBehaviour.changeView -= changeView;
        ShopBehaviour.AddCardToDeck -= AddCardToDeck;
        MouseController.instance.SetSelectedCursor -= SetSelectedCursor;
        MouseController.instance.SetMultipleCursor -= SetMultipleCursor;
        GameLoop.drawFullHand -= drawFullHand;
        PlayerNetwork.clearMultipleChosenCards -= clearMultipleChosenCards;
        PlayerNetwork.burnMultipleCards -= burnMultipleCards;
        PlayerNetwork.UseCard -= UseCard;
        CardBehaviour.drawCard -= drawCard;
        CardBehaviour.useCard -= UseCard;
        CardBehaviour.specialEffectBurn -= specialEffectBurn;
        CardBehaviour.burnCard -= burnCard;
    }


    // Update is called once per frame
    void Update()
    {
        handlePlayerInput();
        calculateCardPositions();
    }

    public void calculateCardPositions()
    {
        //choose view modifier according to the view
        int viewModifierY = 0;
        int viewModifierZ = 0;
        switch (viewNumber)
        {
            case (int)ViewTypes.CardsOnly:
                viewModifierY = -18;
                viewModifierZ = 10;
                break;
            case (int)ViewTypes.BoardCards:
                viewModifierY = -18;
                viewModifierZ = 0;
                break;
            case (int)ViewTypes.BoardOnly:
            case (int)ViewTypes.Shop:
                viewModifierY = -40;
                viewModifierZ = -100;
                break;
        }

        Vector3[] cardPosition = new Vector3[cardsOnHand.Count];
        double range = spaceBetweenCard * cardsOnHand.Count;
        double spaces = range / (cardsOnHand.Count - 1);
        if (cardsOnHand.Count == 1)
        {
            spaces = 0;
        }

        float selectedOffset = 0;
        for (int i = 0; i < cardsOnHand.Count; i++)
        {
            if(GameLoop.PlayerPhase == Phase.MOVEMENT_PHASE && multipleChosenCards.Count == 0)
            {
                if (selectedCursor != -1 && selectedCursor != i)
                {
                    selectedOffset = selectedCursorOffsetValue;
                }
                else
                {
                    selectedOffset = 0;
                }
            }
            else if (multipleChosenCards.Count != 0)
            {
                bool isFound = false;
                foreach(var card in multipleChosenCards)
                {
                    if( card == cardsOnHand[i] )
                    {
                        isFound = true;
                        selectedOffset = 0;
                        break;
                    }
                }
                if (!isFound)
                {
                    selectedOffset = selectedCursorOffsetValue;
                }
            }

            cardPosition[i].x = (float)(mainCamera.transform.position.x + (i * spaces) - range / 2);
            cardPosition[i].y = mainCamera.transform.position.y + viewModifierY;
            cardPosition[i].z = mainCamera.transform.position.z + viewModifierZ + selectedOffset;

            if (cursor == i + 1)
            {
                cardPosition[i].z += 4;
            }


        }

        //zastosowac ta interpolacje (daje ten efekt ruchu) 
        for (int i = 0; i < cardsOnHand.Count; i++)
        {
            cardsOnHand[i].transform.position = Vector3.Lerp(cardsOnHand[i].transform.position, cardPosition[i], speedOfCard * Time.deltaTime);
        }
    }

    public void changeView(ViewTypes type)
    {
        this.viewNumber = (int)type;
    }

    private void SetCursor(RaycastHit hit, bool isSelected)
    {
        if (!isSelected)
        {
            cursor = -1;
            return;
        }

        int index = findIndexOfCard(hit.collider.gameObject);
        cursor = index + 1;
    }

    //is only invoked in movement phase, in movement phase the special efect of cards can be executed
    private void SetSelectedCursor(RaycastHit hit)
    {
        int index = findIndexOfCard(hit.collider.gameObject);
        selectedCursor = index;

        //if special effect includes burning cards
        if (cardsToBurn > 0)
        {
            burnCard();
            cardsToBurn--;
            InformationTxt.GetComponent<TextMeshProUGUI>().text = "Choose " + cardsToBurn + " cards to burn";
        }
        
        if(cardsToBurn == 0)
        {
            InformationTxt.SetActive(false);
        }

        if (hit.collider.gameObject.GetComponent<CardBehaviour>().Typ == "Special")
        {
            ExecuteSpecialEffect?.Invoke(hit);
        }
    }

    private void SetMultipleCursor(RaycastHit hit)
    {
        GameObject card = hit.collider.gameObject;
        bool isInList = false;
        foreach(var  tmp in multipleChosenCards)
        {
            if (tmp == card)
            {
                isInList = true;
            }
        }

        if (isInList)
        {
            multipleChosenCards.Remove(card);
            return;
        }

        multipleChosenCards.Add(hit.collider.gameObject);
    }

    private void handlePlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            drawFullHand(1);
        }
        else if(Input.GetKeyDown(KeyCode.Escape))
        {
            CardBehaviour cardBehaviour = cardsOnHand[selectedCursor].GetComponent<CardBehaviour>();
            if (cardBehaviour.leftPower != cardBehaviour.Power)
            {
                UseCard();
            }
            selectedCursor = -1;
        }
    }

    private void createStarterDeck()
    {
        //scuffed but idc
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Turystka], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Turystka], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Turystka], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Turystka], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Globtroter], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Globtroter], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Globtroter], Deck.transform));
        cardsInDeck.Add(Instantiate(starterCardPack[(int)CardTypes.Marynarz], Deck.transform));
        //for debug 
        cardsInDeck.Add(Instantiate(starterCardPack[3], Deck.transform));

        for (int i = 0; i < cardsInDeck.Count; i++)
        {
            cardsInDeck[i].SetActive(false);
            cardsInDeck[i].tag = "Card_Deck";
        }

    }

    public void AddCardToDeck(GameObject card)
    {
        card.tag = "Card_Used";
        card.SetActive(false);
        usedCards.Add(card);
        /*card.tag = "Card_Hand";
        card.SetActive(true);
        cardsOnHand.Add(card);*/
    }


    //draw random card from deck
    public void drawCard()
    {
        if(cardsInDeck.Count == 0)
        {
            foreach (var usedCard in usedCards)
            {
                usedCard.tag = "Card_Deck";
                cardsInDeck.Add(usedCard);
            }
            usedCards.Clear();
            Debug.Log("Reshufled the used cards");
        }

        int index = UnityEngine.Random.Range(0, cardsInDeck.Count);
        GameObject card = cardsInDeck[index];
        cardsOnHand.Add(cardsInDeck[index]);
        card.SetActive(true);
        card.tag = "Card_Hand";
        cardsInDeck.RemoveAt(index);
    }

    public void drawFullHand(int numOfCards)
    {
        if (numOfCards <= 0)
        {
            numOfCards = 4 - cardsOnHand.Count;
            if (numOfCards <= 0)
            {
                return;
            }
        }

        if (numOfCards > cardsInDeck.Count)
        {
            foreach (var card in usedCards)
            {
                card.tag = "Card_Deck";
                cardsInDeck.Add(card);
            }
            usedCards.Clear();
        }

        for(int i = 0; i < numOfCards; i++)
        {
            drawCard();
        }
    }

    private int findIndexOfCard(GameObject card)
    {
        int index = -1;
        for (int i = 0; i < cardsOnHand.Count; i++)
        {
            if (cardsOnHand[i].GetInstanceID() == card.GetInstanceID())
            {
                index = i;
                break;
            }
        }
        return index;
    }

    public void UseCard()
    {
        int index = selectedCursor;

        CardBehaviour cardBehaviour = cardsOnHand[index].GetComponent<CardBehaviour>();
        cardBehaviour.leftPower = cardBehaviour.Power;
        cardsOnHand[index].tag = "Card_Used";
        cardsOnHand[index].SetActive(false);
        usedCards.Add(cardsOnHand[index]);
        cardsOnHand.RemoveAt(index);
        selectedCursor = -1;
    }

    public void burnMultipleCards()
    {
        for (int i = 0;i < multipleChosenCards.Count;i++)
        {
            GameObject card = multipleChosenCards[i];
            card.SetActive(false);
            int index = findIndexOfCard(card);
            cardsOnHand.RemoveAt(index);
            Destroy(card);
        }
        multipleChosenCards.Clear();
    }

    public void burnCard()
    {
        int index = selectedCursor;
        cardsOnHand[index].SetActive(false);
        GameObject card = cardsOnHand[index];
        cardsOnHand.RemoveAt(index);
        Destroy(card);
        selectedCursor = -1;
    }

    public void specialEffectBurn(int numOfCardsToBurn)
    {
        Debug.Log(InformationTxt);
        cardsToBurn = numOfCardsToBurn;
        InformationTxt.GetComponent<TextMeshProUGUI>().text = "Choose " + cardsToBurn + " cards to burn";
        InformationTxt.SetActive(true);
    }
}
