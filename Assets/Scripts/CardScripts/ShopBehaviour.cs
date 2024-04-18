using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;

public class ShopBehaviour : MonoBehaviour
{
    [SerializeField] List<GameObject> activeShopPositions;
    [SerializeField] List<GameObject> looseCardPositions;
    [SerializeField] List<GameObject> cards;
    public List<GameObject> cardsInShop;
    public List<GameObject> looseCards;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            cardsInShop.Add(Instantiate(cards[i], activeShopPositions[i].transform));
        }
        for (int i = 0; i < looseCardPositions.Count; i++)
        {
            looseCards.Add(Instantiate(cards[i+6], looseCardPositions[i].transform));
        }
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

    public bool BuyCard(GameObject card, int coins)
    {
        if (!card.GetComponent("CardBehaviour"))
        {
            CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
            if (coins>= cardBehaviour.price)
            {
                int index = FindEmptySlotInShop();
                if (!cardBehaviour.isBuyable && (index!=-1))
                {
                    int quantity = cardBehaviour.quantityInShop;
                    cardsInShop.Add(Instantiate(card, activeShopPositions[index].transform));
                }
                else
                {
                    return false;
                }
                cardBehaviour.UpdateQuantity();
                if (cardBehaviour.quantityInShop == 0)
                {
                    ClearCard(card);
                }
                return true;
            }else
            return false;
        }else 
            return false;
    }
}
