using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardBehaviour : MonoBehaviour
{
    [SerializeField] public int Power;
    [SerializeField] public int leftPower;
    [SerializeField] public string NameOfCard;
    [SerializeField] public string Typ;
    [SerializeField] public bool isStartingCard;
    [SerializeField] public bool isBuyable;
    [SerializeField] public int quantityInShop;
    [SerializeField] public int price;
    [SerializeField] public bool isSelected;

    public DeckManager deckManager;

    public static Action drawCard;
    public static Action useCard;
    public static Action<int> specialEffectBurn;


    private void Awake()
    {
        leftPower = Power;

    }

    public void OnEnable()
    {
        DeckManager.ExecuteSpecialEffect += ExecuteSpecialEffect;
    }

    public void OnDisable()
    {
        DeckManager.ExecuteSpecialEffect -= ExecuteSpecialEffect;
    }

    public void ExecuteSpecialEffect(RaycastHit hit)
    {
        if (hit.collider.gameObject != gameObject) return;

        GameObject card = hit.collider.gameObject;
        CardBehaviour cardBehaviour = card.GetComponent<CardBehaviour>();

        //TODO - decide if string is enough or should be changed to enum
        if(cardBehaviour.NameOfCard == "DrBotaniki")
        {
            drawCard?.Invoke();
            useCard?.Invoke();
            specialEffectBurn?.Invoke(2);
        }
        
    }

    public void UpdateQuantity()
    {
        quantityInShop--;
    }
    
}
