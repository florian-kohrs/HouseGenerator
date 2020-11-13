using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{

    public Text fpsText;

    private Queue<float> fpsTimes = new Queue<float>(15);

    private void Start()
    {
        fpsText.enabled = PersistentGameDataController.Settings.displayFps;
        timeToNextDisplay = updateDelay / 2;
    }

    private float timeToNextDisplay = 1;
    private float updateDelay = 1;

    void Update()
    {
        fpsTimes.Enqueue(Mathf.Pow(Time.deltaTime, -1));
        if (fpsTimes.Count == 15)
        {
            fpsTimes.Dequeue();
        }

        timeToNextDisplay -= Time.deltaTime;

        if(timeToNextDisplay <= 0)
        {
            timeToNextDisplay += updateDelay;
            fpsText.text = ((int)(fpsTimes.Aggregate(0f, (t, s) => t + s) / fpsTimes.Count)).ToString();
        }
    }
    
}
