namespace InfonetCSCR.Core.Dtos;

public class GetRepCloseBatchDto
{
    public Policies? policies { get; set; }
    public string? BatchID { get; set; }
    public string? BatchNumber { get; set; }
    public string? TerminalId { get; set; }
    public DateOnly BatchDate { get; set; }
    public string? BatchTime { get; set; }
}
