using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI displayText;

    public ulong ClientId { get; private set; }
    public FixedString32Bytes PlayerName { get; private set; }
    
    private int _playerCoins;
    
    public void Initialize(ulong clientId, FixedString32Bytes playerName, int playerCoins)
    {
        ClientId = clientId;
        PlayerName = playerName;
        
        UpdatePlayerCoins(playerCoins);
    }

    public void UpdatePlayerCoins(int coins)
    {
        _playerCoins = coins;
        
        UpdateText();
    }

    private void UpdateText()
    {
        displayText.text = $"1. {PlayerName} ({_playerCoins})";
    }
}
