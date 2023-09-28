using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Services.Analytics;
using Unity.Services.Core;
using Unity.Services.Core.Analytics;

public class JamAnalytics : MonoBehaviour
{
    private static JamAnalytics _instance;
    public static JamAnalytics Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<JamAnalytics>();
                if (!_instance)
                {
                    Debug.Log("Unable to locate JamAnalytics in the scene. Creating a new JamAnalytics.");
                    var go = (GameObject)Instantiate(Resources.Load("JamAnalytics"), Vector3.zero, Quaternion.identity);
                    _instance = go.GetComponent<JamAnalytics>();
                }

                Debug.Log("JamAnalytics is " + _instance.transform.name);
            }
            return _instance;
        }
        protected set
        {
            _instance = value;
        }
    }

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync();
        }
        catch (ConsentCheckException e)
        {
            Debug.Log(e.ToString());
        }
    }

    public void FireLevelCompletedEvent(
        int levelNumber, 
        bool levelWon,
        int enemiesKilledTotal, 
        int moneyEarnedTotal, 
        int packagesDeliveredTotal, 
        int packagesLostTotal,
        int enemiesKilledThisLevel, 
        int moneyEarnedThisLevel, 
        int packagesDeliveredThisLevel, 
        int packagesLostThisLevel)
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>()
       {
           { "levelNumber", levelNumber },
           { "levelWon", levelWon },
           { "enemiesKilledTotal", enemiesKilledTotal },
           { "moneyEarnedTotal", moneyEarnedTotal },
           { "packagesDeliveredTotal", packagesDeliveredTotal },
           { "packagesLostTotal", packagesLostTotal },
           { "enemiesKilledThisLevel", enemiesKilledThisLevel },
           { "moneyEarnedThisLevel", moneyEarnedThisLevel },
           { "packagesDeliveredThisLevel", packagesDeliveredThisLevel },
           { "packagesLostThisLevel", packagesLostThisLevel },
       };

        // The event will get cached locally 
        //and sent during the next scheduled upload, within 1 minute
        AnalyticsService.Instance.CustomData("levelCompleted", parameters);

        // Can call Events.Flush() to send the event immediately
        AnalyticsService.Instance.Flush();
    }
}