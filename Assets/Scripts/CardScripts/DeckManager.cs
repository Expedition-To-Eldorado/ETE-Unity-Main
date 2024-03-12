using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;

public class DeckManager : MonoBehaviour
{
    [SerializeField] GameObject mainCamera;
    [SerializeField]List<GameObject> cardsInDeck;
    [SerializeField] List<GameObject> cardsOnHand;
    [SerializeField] List<GameObject> usedCards;
    [SerializeField] float speedOfCard = 5;
    [SerializeField] double spaceBetweenCard = 5;
    public GameObject Deck;
    public GameObject[] starterCardPack = new GameObject[3];
    int viewNumber = (int)ViewTypes.CardsOnly; 
    int cursor = -1;
    //Obecnie tworzony jest widok 3 - widok na karty



    // Start is called before the first frame update
    void Start()
    {
        createStarterDeck();
        drawCards(4);

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
                viewModifierZ = 0;
                break;
        }

        Vector3[] cardPosition = new Vector3[cardsOnHand.Count];
        double range = spaceBetweenCard * cardsOnHand.Count;
        double spaces = range / (cardsOnHand.Count - 1);
        if (cardsOnHand.Count == 1)
        {
            spaces = 0;
        }

        for (int i = 0; i < cardsOnHand.Count; i++)
        {
            cardPosition[i].x = (float)(mainCamera.transform.position.x + (i * spaces) - range / 2);
            cardPosition[i].y = mainCamera.transform.position.y + viewModifierY;
            cardPosition[i].z = mainCamera.transform.position.z + viewModifierZ;

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

    private void handlePlayerInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (cursor > 1 && cursor <= cardsOnHand.Count)
            {
                cursor--;
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (cursor >= 1 && cursor < cardsOnHand.Count)
            {
                cursor++;
            }
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            cursor *= -1;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            drawCards(1);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (cursor >= 1 && cursor <= cardsOnHand.Count)
            {
                Debug.Log(cardsOnHand[cursor - 1].GetComponent<CardBehaviour>().NameOfCard);
                useCard();
            }

            if (cursor < 1 || cursor > cardsOnHand.Count)
            {
                cursor = 1;
            }
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

        for (int i = 0; i < cardsInDeck.Count; i++)
        {
            cardsInDeck[i].SetActive(false);
        }

    }

    public void drawCards(int numOfCards)
    {
        for(int i = 0; i < numOfCards; i++)
        {
            if(cardsInDeck.Count != 0)
            {
                int index = Random.Range(0, cardsInDeck.Count);
                GameObject card = cardsInDeck[index];
                cardsOnHand.Add(cardsInDeck[index]);
                card.SetActive(true);
                cardsInDeck.RemoveAt(index);
            }
            else
            {
                Debug.Log("not enough cards in deck: " + (numOfCards - i) + " left");
            }
        }
    }


    public void useCard()
    {
        cardsOnHand[cursor - 1].SetActive(false);
        usedCards.Add(cardsOnHand[cursor - 1]);
        cardsOnHand.RemoveAt(cursor - 1);
    }

    public void burnCard()
    {
        cardsOnHand[cursor - 1].SetActive(false);
        GameObject card = cardsOnHand[cursor - 1];
        cardsOnHand.RemoveAt(cursor - 1);
        Destroy(card);
    }
}
