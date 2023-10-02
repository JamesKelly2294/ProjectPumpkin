using info.jacobingalls.jamkit;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScoreIndicator : MonoBehaviour
{
    public TextMeshProUGUI ScoreLabel;

    private int _totalCrystals;
    private int _collectedCrystals;

    // Start is called before the first frame update
    void Start()
    {
        ScoreLabel.text = "";
    }
    public void CrystalsCollected(PubSubListenerEvent e)
    {
        int collected = (int)e.value;
        _collectedCrystals += collected;
        Redraw();
    }

    public void TotalCrystalsChanged(PubSubListenerEvent e)
    {
        int total = (int)e.value;
        _totalCrystals = total;
        Redraw();
    }

    public void Redraw()
    {
        ScoreLabel.text = $"{_collectedCrystals}/{_totalCrystals}";
    }
}
