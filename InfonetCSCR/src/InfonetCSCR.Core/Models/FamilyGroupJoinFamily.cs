namespace InfonetCSCR.Core.Models;

public class FamilyGroupJoinFamily
{
    public string GroupID { get; set; } = default!;
    public string? GroupName { get; set; }
    public string FamilyID { get; set; } = default!;
    public string? Description { get; set; }
}
