using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using System;

public class ShopBehaviour : MonoBehaviour
{
    [SerializeField] List<GameObject> activeShopPositions;
    [SerializeField] List<GameObject> looseCardPositions;
    [SerializeField] List<GameObject> cards;
    public List<GameObject> cardsInShop;
    public List<GameObject> looseCards;

    public static Action<GameObject> AddCardToDeck;

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
        MouseController.instance.BuyCard += BuyCard;
    }

    private void OnDisable()
    {
        MouseController.instance.BuyCard -= BuyCard;
    }

    // Update is called once per frame
    void Update()
    {
        
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

    public void ClearCard(GameObject card)
    {
        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        for (int i = 0; i < cards.Count; i++)
        {
            if (card.name == cards[i].name)
            {
                int index = FindCardInShop(card);
                if (index >= 0)activeShopPositions.RemoveAt(index);
                Destroy(card);
            }
        }
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

    private void BuyCard(GameObject card, int coins)
    {

        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        if (coins >= cardBehaviour.price)
        {
            int index = FindEmptySlotInShop();
            if (!cardBehaviour.isBuyable && (index != -1))
            {
                cardsInShop.Add(Instantiate(card, activeShopPositions[index].transform));
            }
            cardBehaviour.UpdateQuantity();
            Debug.Log("i think im buying a card " + card);
            AddCardToDeck?.Invoke(card);
            if (cardBehaviour.quantityInShop == 0)
            {
                ClearCard(card);
            }
        }
    }
}
