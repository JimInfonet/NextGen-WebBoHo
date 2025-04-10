namespace InfonetCSCR.Core.Dtos;

public class FamilyGroupDto
{
  public string GroupID { get; set; } = default!;
  public string? GroupName { get; set; }
  public string FamilyID { get; set; } = default!;
  public string? Description { get; set; }
}
