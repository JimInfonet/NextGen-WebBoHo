namespace InfonetCSCR.Core.Dtos;

public class GetRepTillCloseDto
{
    public Policies? policies { get; set; }
    public DateTime startDate { get; set; }
    public string? startTime { get; set; }
    public DateTime endDate { get; set; }
    public string? endTime { get; set; }
    public int closeNo { get; set; }
    public int tillNo { get; set; }
    public DateTime shiftDate { get; set; }
    public string? shiftNo { get; set; }
}
