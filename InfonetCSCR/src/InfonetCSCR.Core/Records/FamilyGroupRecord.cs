namespace InfonetCSCR.Core.Records;

public class FamilyGroupRecord
{
  public string GroupID { get; set; } = default!;
  public string GroupName { get; set; } = default!;
  public string[] Families { get; set; } = default!;
}
