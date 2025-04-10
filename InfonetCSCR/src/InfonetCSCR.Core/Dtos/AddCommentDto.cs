namespace InfonetCSCR.Core.Dtos;

public class AddCommentDto
{
  public bool CheckOutPreviousDay {  get; set; }
  public string UserCode {  get; set; }
  public string Comment {  get; set; }
}
