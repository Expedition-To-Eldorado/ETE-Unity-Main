using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using System;
using static Unity.Burst.Intrinsics.X86.Avx;

public class ShopBehaviour : Singleton<ShopBehaviour>
{
    [SerializeField] List<GameObject> activeShopPositions;
    [SerializeField] List<GameObject> looseCardPositions;
    [SerializeField] List<GameObject> cards;
    public List<GameObject> cardsInShop;
    public List<GameObject> looseCards;
    public GameObject Deck;

    public Action<GameObject> AddCardToDeck;

    // Start is called before the first frame update
    void Start()
    {
        GameObject tmp;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            tmp = Instantiate(cards[i], activeShopPositions[i].transform);
            tmp.tag = "Card_Shop";
            cardsInShop.Add(tmp);
        }
        for (int i = 0; i < looseCardPositions.Count; i++)
        {
            tmp = Instantiate(cards[i + 6], looseCardPositions[i].transform);
            tmp.tag = "Card_Shop";
            looseCards.Add(tmp);
        }
    }

    private void OnEnable()
    {
        MouseController.BuyCard += BuyCard;
    }

    private void OnDisable()
    {
        MouseController.BuyCard -= BuyCard;
    }

    public int FindCardInShop(GameObject card)
    {
        int index = -1;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            GameObject cardInShop = activeShopPositions[i].transform.GetChild(0).gameObject;
            if (card.name == cardInShop.name)
            {
                index = i;
                return index;
            }
        }
        return index;
    }

    public int FindEmptySlotInShop()
    {
        int index = -1;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            if (activeShopPositions[i].transform.childCount <= 0)
            {
                index = i;
                return index;
            }
        }
        return index;
    }

    private ErrorMsg BuyCard(GameObject card, int coins)
    {

        //choseCardFromDeck;

        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        if (coins == cardBehaviour.price)
        {
            int index = FindEmptySlotInShop();
            if (!cardBehaviour.isBuyable && (index != -1))
            {
                Destroy(card);
                GameObject tmp = Instantiate(card, activeShopPositions[index].transform);
                CardBehaviour tmpBehaviour = tmp.GetComponent<CardBehaviour>();
                tmpBehaviour.isBuyable = true;
                tmpBehaviour.UpdateQuantity();
                cardsInShop.Add(tmp);
            }
            else if (!cardBehaviour.isBuyable && index == -1)
            {
                return ErrorMsg.SHOP_FULL;
            }
            cardBehaviour.UpdateQuantity();

            GameObject tmp2 = Instantiate(card, Deck.transform);
            tmp2.transform.Rotate(0, 180, 0);
            AddCardToDeck?.Invoke(tmp2);
            if (cardBehaviour.quantityInShop == 0)
            {
                Destroy(card);
            }
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
}
