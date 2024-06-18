using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartWindowUI : MonoBehaviour
{
    [SerializeField] private Button createGame;
    [SerializeField] private Button joinGame;
    
    public void Awake()
    {
        createGame.onClick.AddListener(() => {
            CreateGameUI.Instance.Show();
        });
        joinGame.onClick.AddListener(() => {
            JoinGameUI.Instance.Show();
        });
    }
}
