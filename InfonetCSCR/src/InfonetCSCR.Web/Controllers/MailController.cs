namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class MailController : Controller
{
  private readonly IEmailService _emailService;

  public MailController(IEmailService emailService)
  {
    _emailService = emailService;
  }

  public async Task<IActionResult> SendMail(string recipientEmail, string subject, string body, string attachmentFilePath)
  {
    bool sent = await _emailService.SendMail(recipientEmail, subject, body, attachmentFilePath);
    return Ok(sent);
  }

  public async Task<IActionResult> EmailTextReport([FromBody] EmailViewModel email)
  {
    return Ok(await _emailService.EmailTextReportAsync(email));
  }
}
