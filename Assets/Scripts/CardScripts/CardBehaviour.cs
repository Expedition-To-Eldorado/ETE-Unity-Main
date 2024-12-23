using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    [SerializeField] public bool isBurnable;
    [SerializeField] public Texture2D inspectionImage;

    public static Action drawCard;
    public static Action useCard;
    public static Action burnCard;
    public static Action<int> specialEffectBurn;
    public static Action specialEffectBuy;


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

        if(cardBehaviour.NameOfCard == "DrBotaniki")
        {
            drawCard?.Invoke();
            useCard?.Invoke();
            specialEffectBurn?.Invoke(1);
        }
        else if (cardBehaviour.NameOfCard == "DziennikPodrozy")
        {
            drawCard?.Invoke();
            drawCard?.Invoke();
            burnCard?.Invoke();
            specialEffectBurn?.Invoke(2);
        }
        else if (cardBehaviour.NameOfCard == "Kartograf")
        {
            drawCard?.Invoke();
            drawCard?.Invoke();
            useCard?.Invoke();
        }
        else if (cardBehaviour.NameOfCard == "Kompas")
        {
            drawCard?.Invoke();
            drawCard?.Invoke();
            drawCard?.Invoke();
            burnCard?.Invoke();
        }
        else if (cardBehaviour.NameOfCard == "Nadajnik")
        {
            specialEffectBuy?.Invoke();
            burnCard?.Invoke();
        }
    }

    public void UpdateQuantity()
    {
        Debug.Log("bought a card: " + NameOfCard);
        quantityInShop--;
    }
    
}
