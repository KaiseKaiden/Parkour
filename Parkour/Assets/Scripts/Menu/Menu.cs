using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    [SerializeField] GameObject myInGameUI;
    [SerializeField] GameObject myPlayer;

    public void StartGame()
    {
        myInGameUI.SetActive(true);
        myPlayer.SetActive(true);

        gameObject.SetActive(false);
    }
}
