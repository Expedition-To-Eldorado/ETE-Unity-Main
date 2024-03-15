using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralEnumerations 
{
    //zdecydujcie czy wolicie po kolorze czy po nazwie
    public enum CardTypes
    {
        Turystka,
        Globtroter,
        Marynarz
    }

    public enum ViewTypes
    {
        CardsOnly,
        BoardCards,
        BoardOnly,
        Shop,
        NumOfViewTypes
    }

    public enum BoardPiece
    {
        A, B, C, D, E, F, G, H, I, J, K, M, N
    }
}
