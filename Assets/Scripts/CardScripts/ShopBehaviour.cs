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

    public void ClearCard(GameObject card)
    {
        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        for (int i = 0; i < cards.Count; i++)
        {
            if (card.name == cards[i].name)
            {
                Destroy(card);
            }
        }
    }

    public bool BuyCard(GameObject card, int coins)
    {
        if (!card.GetComponent("CardBehaviour"))
        {
            CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
            if (cardBehaviour.isBuyable && coins>= cardBehaviour.price)
            {
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
