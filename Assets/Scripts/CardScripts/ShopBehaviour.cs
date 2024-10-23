using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GeneralEnumerations;
using System;
using static Unity.Burst.Intrinsics.X86.Avx;
using Unity.Netcode;

public class ShopBehaviour : NetworkBehaviour
{
    [SerializeField] List<GameObject> activeShopPositions;
    [SerializeField] List<GameObject> looseCardPositions;
    [SerializeField] List<GameObject> cards;
    public GameObject[] cardsInShop;
    public List<GameObject> looseCards;
    //public GameObject Deck;

    public static Action<GameObject> AddCardToDeck;
    public GameObject InformationTxt;
    public DeckManager deckManager;

    // Start is called before the first frame update
    void Start()
    {
        GameObject tmp;
        for (int i = 0; i < activeShopPositions.Count; i++)
        {
            tmp = Instantiate(cards[i], activeShopPositions[i].transform);
            tmp.tag = "Card_Shop";
            cardsInShop[i] = tmp;
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
        MouseController.buyAnyCardEffect += buyAnyCardEffect;
    }

    private void OnDisable()
    {
        MouseController.BuyCard -= BuyCard;
        MouseController.buyAnyCardEffect -= buyAnyCardEffect;
    }

    public int FindCardInShop(GameObject card)
    {
        //string cardname = card.name.Substring(0, card.name.Length - 7);
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
        //string cardname = card.name.Substring(0, card.name.Length - 7);
        int index = -1;
        for (int i = 0; i < looseCards.Count; i++)
        {
            if (card.name == looseCards[i].name)
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

        InformationTxt.SetActive(false);
        deckManager.buyAnyCard = false;
    }

    private ErrorMsg BuyCard(GameObject card, int coins)
    {
        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();
        if (coins == cardBehaviour.price)
        {
            /*int index = FindEmptySlotInShop();
            if (!cardBehaviour.isBuyable && (index != -1))
            {
                Destroy(card);
                GameObject tmp = Instantiate(card, activeShopPositions[index].transform);
                CardBehaviour tmpBehaviour = tmp.GetComponent<CardBehaviour>();
                tmpBehaviour.isBuyable = true;
                tmpBehaviour.UpdateQuantity();
                cardsInShop.Add(tmp);
            }*/
            int index = FindEmptySlotInShop();
            if (!cardBehaviour.isBuyable && index == -1)
            {
                return ErrorMsg.SHOP_FULL;
            }
            //cardBehaviour.UpdateQuantity();

            GameObject deck = GameObject.Find("Deck");

            GameObject tmp2 = Instantiate(card, deck.transform);
            tmp2.transform.Rotate(0, 180, 0);
            AddCardToDeck?.Invoke(tmp2);
            /*if (cardBehaviour.quantityInShop == 0)
            {
                Destroy(card);
            }*/

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
        Debug.Log("Im calling the updateshopclientrpc method, i am: " + OwnerClientId);
        if (!IsOwner)
        {
            Debug.Log("Since im not the owner, updateshop was not called, i am: " + OwnerClientId);
            //return;
        }

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
        if (!cardBehaviour.isBuyable && (index != -1))
        {
            //Destroy(card);
            card.transform.position = activeShopPositions[index].transform.position;

            //GameObject tmp = Instantiate(card, activeShopPositions[index].transform);
            card.transform.parent = activeShopPositions[index].transform;
            CardBehaviour tmpBehaviour = card.GetComponent<CardBehaviour>();
            tmpBehaviour.isBuyable = true;
            //tmpBehaviour.UpdateQuantity();
            looseCards.RemoveAt(indexOfCard);
            cardsInShop[index] = card;
        }

        cardBehaviour.UpdateQuantity();

        if (cardBehaviour.quantityInShop == 0)
        {
            if(isInShop)
            {
                cardsInShop[indexOfCard] = null;
            }
            else
            {
                looseCards.RemoveAt(indexOfCard);
            }
            Destroy(card);
        }
    }
}
