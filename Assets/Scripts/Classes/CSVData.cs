// Description: This script defines the data type that stores the CSV data
// Stored in a list that can be shuffled through the persistent data manager

[System.Serializable]
public class CSVData
{
    public string Level;
    public string Starting;
    public string Target;
    public float StartingX;
    public float StartingY;
    public float StartingZ;
    public float TargetX;
    public float TargetY;
    public float TargetZ;
    public float StartingHorizontalAngle;
    public float StartingVerticalAngle;
    public float TrueHorizontalAngle;
    public float TrueVerticalAngle;
}
