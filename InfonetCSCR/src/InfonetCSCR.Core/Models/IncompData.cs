namespace InfonetCSCR.Core.Models;

public class IncompData
{
    public string? Sale_No { get; set; }
    public DateOnly Sale_Date { get; set; }
    public TimeOnly Sale_Time { get; set; }
    public double CompletionAmount { get; set; }
}
