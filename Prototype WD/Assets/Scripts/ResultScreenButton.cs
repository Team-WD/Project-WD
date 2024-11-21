using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResultScreenButton : MonoBehaviour
{
    public void ToLobby()
    {
        SceneManager.LoadScene("Lobby");
    }
}
