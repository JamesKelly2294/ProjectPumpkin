using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoseScene : MonoBehaviour
{

    public TextMeshProUGUI turns, points, kills, crystals;

    // Start is called before the first frame update
    void Start()
    {
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            turns.text = "" + gameManager.PastState.Turns;
            points.text = "" + gameManager.PastState.Points;
            kills.text = "" + gameManager.PastState.EnemiesSlain;
            crystals.text = "" + gameManager.PastState.CrystalsDelivered;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ReturnToMainMenu()
    {
        GameObject.FindObjectOfType<GameManager>().ShowMainWindow();
    }
}
