using System;

public class CheckPoint
{

    public CheckPoint(CheckPoint previous, string message = "")
    {
        this.message = message;
        this.previous = previous;
        this.time = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }

    public CheckPoint previous;

    public long time;

    public string message;

    public long getTimeDiff()
    {
        if(previous == null)
        {
            return 0;
        }
        return time - previous.time;
    }

    public string getInfo()
    {
        string result = getTimeDiff().ToString();

        if (message != "")
        {
            result = "Time needed for " + message + " was :" + result + " milliseconds.";
        }

        return result;
    }

}
