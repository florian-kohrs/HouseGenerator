
[System.AttributeUsage(System.AttributeTargets.Field, AllowMultiple = false)]
public class SaveAttribute : System.Attribute
{
    public bool saveForScene = true;
    public bool saveForGame = true;
}
