using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using System;
using static Unity.Burst.Intrinsics.X86.Avx;
using Unity.Netcode;
using System.Reflection;
using TMPro;
using UnityEngine.Serialization;

public class ShopBehaviour : NetworkBehaviour
{
    [SerializeField] List<GameObject> activeShopPositions;
    [SerializeField] GameObject[] looseCardPositions;
    [SerializeField] List<GameObject> cards;
    public GameObject[] cardsInShop;
    public List<GameObject> looseCards;

    public static Action<GameObject> AddCardToDeck;
    [SerializeField] public GameObject InformationBoard;
    public GameObject InformationTxt;
    public DeckManager deckManager;

    void Start()
    {
        GameObject tmp;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            tmp = Instantiate(cards[i], activeShopPositions[i].transform);
            tmp.tag = "Card_Shop";
            cardsInShop[i] = tmp;
        }

        for (int i = 0; i < 12; i++)
        {
            tmp = Instantiate(cards[i + 6], looseCardPositions[i].transform);
            tmp.tag = "Card_Shop";
            looseCards.Add(tmp);
        }
    }

    private void OnEnable()
    {
        MouseController.BuyCard += BuyCard;
        MouseController.buyAnyCardEffect += buyAnyCardEffect;
    }

    private void OnDisable()
    {
        MouseController.BuyCard -= BuyCard;
        MouseController.buyAnyCardEffect -= buyAnyCardEffect;
    }

    public int FindCardInShop(GameObject card)
    {
        int index = -1;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            if (cardsInShop[i] != null)
            {
                if (card.name == cardsInShop[i].name)
                {
                    return i;
                }
            }
        }

        return index;
    }

    public int FindCardInLoose(GameObject card)
    {
        int index = -1;
        for (int i = 0; i < looseCards.Count; i++)
        {
            if (looseCards[i] != null && card.name == looseCards[i].name)
            {
                return i;
            }
        }

        return index;
    }

    public int FindEmptySlotInShop()
    {
        int index = -1;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            if (cardsInShop[i] == null)
            {
                index = i;
                return index;
            }
        }

        return index;
    }

    public void buyAnyCardEffect(RaycastHit hit)
    {
        GameObject card = hit.collider.gameObject;

        GameObject deck = GameObject.Find("Deck");

        GameObject tmp2 = Instantiate(card, deck.transform);
        tmp2.transform.Rotate(0, 180, 0);
        AddCardToDeck?.Invoke(tmp2);

        int index = FindCardInShop(card);
        bool isInShop = true;
        if (index == -1)
        {
            isInShop = false;
            index = FindCardInLoose(card);
        }

        updateShopServerRpc(index, isInShop);

        InformationTxt.SetActive(false);
        InformationBoard.SetActive(false);
        deckManager.buyAnyCard = false;
    }

    private ErrorMsg BuyCard(GameObject card, float coins)
    {
        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        if (coins == cardBehaviour.price)
        {
            int index = FindEmptySlotInShop();
            if (!cardBehaviour.isBuyable && index == -1)
            {
                return ErrorMsg.SHOP_FULL;
            }

            GameObject deck = GameObject.Find("Deck");

            GameObject tmp2 = Instantiate(card, deck.transform);
            tmp2.transform.Rotate(0, 180, 0);
            AddCardToDeck?.Invoke(tmp2);

            index = FindCardInShop(card);
            bool isInShop = true;
            if (index == -1)
            {
                isInShop = false;
                index = FindCardInLoose(card);
            }

            updateShopServerRpc(index, isInShop);
        }
        else if (coins < cardBehaviour.price)
        {
            return ErrorMsg.NOT_ENOUGH_COINS;
        }
        else if (coins > cardBehaviour.price)
        {
            return ErrorMsg.TOO_MUCH_COINS;
        }

        return ErrorMsg.OK;
    }

    [ServerRpc(RequireOwnership = false)]
    public void updateShopServerRpc(int index, bool isInShop)
    {
        updateShopClientRpc(index, isInShop);
    }

    [ClientRpc]
    public void updateShopClientRpc(int indexOfCard, bool isInShop)
    {
        GameObject card;
        if (isInShop)
        {
            card = cardsInShop[indexOfCard];
        }
        else
        {
            card = looseCards[indexOfCard];
        }

        int index = FindEmptySlotInShop();
        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        if (!cardBehaviour.isBuyable && (index != -1) && !deckManager.buyAnyCard)
        {
            card.transform.position = activeShopPositions[index].transform.position;

            card.transform.parent = activeShopPositions[index].transform;
            CardBehaviour tmpBehaviour = card.GetComponent<CardBehaviour>();
            tmpBehaviour.isBuyable = true;
            looseCards[indexOfCard] = null;
            cardsInShop[index] = card;
        }

        cardBehaviour.UpdateQuantity();

        string updatedCardQuantityText = card.GetComponent<CardBehaviour>().quantityInShop.ToString();
        if (updatedCardQuantityText == "0")
        {
            updatedCardQuantityText = " ";
        }

        if (isInShop)
        {
            UpdateCardQuantityUI(indexOfCard, updatedCardQuantityText, true);
        }
        else if (!deckManager.buyAnyCard)
        {
            UpdateCardQuantityUI(indexOfCard, " ", false);
            UpdateCardQuantityUI(index, updatedCardQuantityText, true);
        }
        else if (deckManager.buyAnyCard)
        {
            UpdateCardQuantityUI(indexOfCard, updatedCardQuantityText, false);
        }

        if (cardBehaviour.quantityInShop == 0)
        {
            if (isInShop)
            {
                cardsInShop[indexOfCard] = null;
            }
            else
            {
                looseCards[indexOfCard] = null;
            }

            Destroy(card);
        }
    }

    private void UpdateCardQuantityUI(int index, string newQuantity, bool active)
    {
        Canvas canvas = null;
        if (active == true)
        {
            canvas = activeShopPositions[index].GetComponentInChildren<Canvas>();
        }
        else
        {
            canvas = looseCardPositions[index].GetComponentInChildren<Canvas>();
        }

        TextMeshProUGUI text = canvas.GetComponentInChildren<TextMeshProUGUI>();
        text.text = newQuantity;
    }
}