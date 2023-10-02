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

    private Mossy _mossy;

    // Start is called before the first frame update
    void Start()
    {
        ScoreLabel.text = "";
        _mossy = FindObjectOfType<Mossy>();
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

    public void Update()
    {
        if (_mossy.GlobalCrystalCount != _collectedCrystals)
        {
            _totalCrystals = _mossy.GlobalCrystalCount;
            Redraw();
        }
    }

    public void Redraw()
    {
        ScoreLabel.text = $"{_collectedCrystals}/{_totalCrystals}";
    }
}
