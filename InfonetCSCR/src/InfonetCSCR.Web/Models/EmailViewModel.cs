namespace InfonetCSCR.Web.Models;

public class EmailViewModel
{
  public string? RecipientEmail { get; set; }
  public string? Subject { get; set; }
  public string? Body { get; set; }
  public string? AttachmentName { get; set; }
  public string? AttachmentContent { get; set; }
}
