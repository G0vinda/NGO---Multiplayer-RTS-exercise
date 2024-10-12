using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections;
using UnityEngine;

public class PlayerNameDisplay : MonoBehaviour
{
    [SerializeField] private TankPlayer tankPlayer;
    [SerializeField] private TextMeshProUGUI playerNameText;
    
    private void Start()
    {
        HandlePlayerNameChanged(string.Empty, tankPlayer.PlayerName.Value);
        
        tankPlayer.PlayerName.OnValueChanged += HandlePlayerNameChanged;
    }

    private void HandlePlayerNameChanged(FixedString32Bytes oldName, FixedString32Bytes newName)
    {
        playerNameText.text = newName.ToString();
    }

    private void OnDestroy()
    {
        tankPlayer.PlayerName.OnValueChanged -= HandlePlayerNameChanged;
    }
}
