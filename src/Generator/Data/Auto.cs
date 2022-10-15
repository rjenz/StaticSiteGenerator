namespace Generator.Data;

public class Auto
{
    public Auto()
    {
        Salt = ((int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds).ToString();
    }

    public string Date { get; set; } = DateTime.Now.ToShortDateString();
    public string Salt { get; set; }
}