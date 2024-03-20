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

    public bool BuyCard(int coins)
    {
        return false;
    }
}
