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

    public enum ErrorMsg
    {
        OK,
        DIST_TOO_LONG,
        FIELD_IS_MNTN,
        FIELD_OCCUPIED,
        CARD_NOT_MATCHING,
        NOT_ACTIVE_PAWN
    }
}
