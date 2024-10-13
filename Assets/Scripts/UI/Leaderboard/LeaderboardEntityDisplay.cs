using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class LeaderboardEntityDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI displayText;
    [SerializeField] private Color ownedByPlayerColor;

    public ulong ClientId { get; private set; }
    public int PlayerCoins { get; private set; }
    
    private FixedString32Bytes _playerName;
    
    public void Initialize(ulong clientId, FixedString32Bytes playerName, int playerCoins)
    {
        ClientId = clientId;
        _playerName = playerName;

        if (clientId == NetworkManager.Singleton.LocalClientId)
            displayText.color = ownedByPlayerColor;
        
        UpdatePlayerCoins(playerCoins);
    }

    public void UpdatePlayerCoins(int coins)
    {
        PlayerCoins = coins;
        
        UpdateText();
    }

    public void UpdateText()
    {
        displayText.text = $"{transform.GetSiblingIndex() + 1}. {_playerName} ({PlayerCoins})";
    }
}
