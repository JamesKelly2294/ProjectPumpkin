using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LoseScene : MonoBehaviour
{

    public TextMeshProUGUI shots, xp;

    // Start is called before the first frame update
    void Start()
    {
        GameManager gameManager = GameObject.FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            shots.text = "" + gameManager.PastState.PackagesDelivered + " Packages Delivered";
            xp.text = "$" + gameManager.PastState.TotalMoneyEarned + " Earned";
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
