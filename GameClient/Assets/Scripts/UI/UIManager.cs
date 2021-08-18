using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
   public static UIManager Instance;

   public GameObject startMenu;
   public InputField UsernameField;
   
   
   private void Awake()
   {
      if (Instance == null)
      {
         Instance = this;
      }
      
      else if (Instance != null)
      {
         Debug.Log($"{Instance} mevcut , obje destroy ediliyor!");
         Destroy(this);
      }
   }

   public void ConnectToServer()
   {
      startMenu.SetActive(false);
      UsernameField.interactable = false;
      Client.Instance.ConnectToServer();
   }
}
