using System;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class MenuUI : MonoBehaviour
{
    [SerializeField] public Button QuitGameButton;

    private void Awake()
    {
        QuitGameButton.onClick.AddListener(async () =>
        {
            RelayManager.Instance.LeaveRelay();
            AuthenticationService.Instance.SignOut();
            ScenesManager.Instance.LoadScene(ScenesManager.Scene.StartWindow);
        });
    }
}