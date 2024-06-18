using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyAssets : MonoBehaviour
{
    public static LobbyAssets Instance { get; private set; }


    [SerializeField] private Sprite redSprite;
    [SerializeField] private Sprite purpleSprite;
    [SerializeField] private Sprite whiteSprite;
    [SerializeField] private Sprite blueSprite;


    private void Awake() {
        Instance = this;
    }

    public Sprite GetSprite(LobbyManager.PlayerColor playerColor) {
        switch (playerColor) {
            default:
            case LobbyManager.PlayerColor.Red:   return redSprite;
            case LobbyManager.PlayerColor.Purple:    return purpleSprite;
            case LobbyManager.PlayerColor.White:   return whiteSprite;
            case LobbyManager.PlayerColor.Blue:   return blueSprite;
        }
    }
}
