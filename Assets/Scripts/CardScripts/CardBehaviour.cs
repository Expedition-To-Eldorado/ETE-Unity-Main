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


    private void Awake()
    {
        leftPower = Power;
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateQuantity()
    {
        quantityInShop--;
    }
    
}
