using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NameSelector : MonoBehaviour
{
     [Header("References")]
     [SerializeField] private TMP_InputField nameField;
     [SerializeField] private Button connectButton;

     [Header("Settings")] 
     [SerializeField] private int minNameLength = 1;
     [SerializeField] private int maxNameLength = 12;
     
     public const string PlayerNameKey = "PlayerName";

     private void Start()
     {
          if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
          {
               LoadNextScene();
               return;
          }
          
          nameField.text = PlayerPrefs.GetString(PlayerNameKey, "");
          HandleNameChanged();
     }

     public void HandleNameChanged()
     {
          connectButton.interactable = 
               nameField.text.Length >= minNameLength && 
               nameField.text.Length <= maxNameLength;
     }

     public void Connect()
     {
          PlayerPrefs.SetString(PlayerNameKey, nameField.text);
          LoadNextScene();
     }

     private void LoadNextScene()
     {
          SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
     }
}

