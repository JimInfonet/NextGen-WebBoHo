namespace InfonetCSCR.Web.Services;

public class EmailService : IEmailService
{
  private readonly IApplicationConfig _config;
  private readonly ILogger<EmailService> _logger;

  public EmailService(IApplicationConfig config, ILogger<EmailService> logger)
  {
    _config = config;
    _logger = logger;
  }

  public async Task<bool> SendMail(string recipientEmail, string subject, string body, string attachmentFilePath)
  {
    try
    {
      string smtpHost = _config.SmtpHost;
      int smtpPort = _config.SmtpPort;
      string senderName = _config.SenderName;
      string senderEmail = _config.SenderEmailAddress;
      string senderAppPassword = _config.SenderAppPassword;

      var mailMessage = new MailMessage();
      mailMessage.From = new MailAddress(senderEmail, senderName);
      mailMessage.To.Add(recipientEmail);
      mailMessage.Subject = subject;
      mailMessage.Body = body;
      if (!string.IsNullOrEmpty(attachmentFilePath) && File.Exists(attachmentFilePath))
      {
        mailMessage.Attachments.Add(new Attachment(attachmentFilePath));
      }

      using var smtpClient = new SmtpClient(smtpHost, smtpPort) { EnableSsl = true };
      smtpClient.UseDefaultCredentials = false;
      smtpClient.Credentials = new NetworkCredential(senderEmail, senderAppPassword);
      smtpClient.EnableSsl = _config.UseHttps;

      await smtpClient.SendMailAsync(mailMessage);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(Convert.ToString(ex.Message));
      throw new Exception("Could not send email");
    }
  }

  public async Task<bool> EmailTextReportAsync(EmailViewModel email)
  {
    try
    {
      var senderEmail = _config.SenderEmailAddress;
      var mailMessage = new MailMessage();
      mailMessage.From = new MailAddress(senderEmail, _config.SenderName);
      mailMessage.To.Add(email.RecipientEmail);
      mailMessage.Subject = email.Subject;
      mailMessage.Body = email.Body;

      // If attachment content is provided
      if (!string.IsNullOrEmpty(email.AttachmentName) && !string.IsNullOrEmpty(email.AttachmentContent))
      {
        var attachmentBytes = Encoding.UTF8.GetBytes(email.AttachmentContent);
        var memoryStream = new MemoryStream(attachmentBytes);
        memoryStream.Seek(0, SeekOrigin.Begin);
        mailMessage.Attachments.Add(new Attachment(memoryStream, email.AttachmentName));
      }

      using var smtpClient = new SmtpClient(_config.SmtpHost, _config.SmtpPort) { EnableSsl = true };
      smtpClient.UseDefaultCredentials = false;
      smtpClient.Credentials = new NetworkCredential(senderEmail, _config.SenderAppPassword);
      smtpClient.EnableSsl = _config.UseHttps;

      await smtpClient.SendMailAsync(mailMessage);
      return true;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex.Message);
      throw new Exception("Could not send email");
    }
  }

}
