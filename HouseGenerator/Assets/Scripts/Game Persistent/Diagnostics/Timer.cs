using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Timer
{

    public Timer(bool printWhenAdding = true)
    {
#if UNITY_EDITOR
        this.printWhenAdding = printWhenAdding;
#endif
    }

    private bool printWhenAdding;

    private CheckPoint last;

    private List<string> printMessage = new List<string>();

    public long totalTime;

    public void start(string startMessage = "")
    {
        last = new CheckPoint(null, startMessage);
        totalTime = 0;
        if (startMessage != "") {
            addAndPrint(startMessage);
        }
    }

    private void addAndPrint(string message)
    {
        printMessage.Add(message);
        if (printWhenAdding)
        {
          //  Debug.Log(message);
        }
    }
    
    public void addCheckPoint(string taskDescription = "")
    {
        last = new CheckPoint(last, taskDescription);
        totalTime += last.getTimeDiff();
        string message = last.getInfo();
        addAndPrint(message);
    }

    /// <summary>
    /// prints all checkPoint times and resets the Timer afterwards
    /// </summary>
    public void finish(string endMessage = "")
    {
        addCheckPoint(endMessage);
        Debug.Log("Total Milliseconds elapsed:" + totalTime);
        reset();
    }

    public void reset()
    {
        printMessage = new List<string>();
        totalTime = 0;
        last = null;
    }

    public void printCheckPointTimes()
    {
#if UNITY_EDITOR
        foreach(string s in printMessage)
        {
            Debug.Log(s);
        }
        Debug.Log("Total Time elapsed:" + totalTime);
#endif
    }
    
}
