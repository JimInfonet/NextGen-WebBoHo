namespace InfonetCSCR.Web.Abstractions.Services;

public interface IEmailService
{
  Task<bool> SendMail(string recipientEmail, string subject, string body, string attachmentFilePath);
  Task<bool> EmailTextReportAsync(EmailViewModel email);
}
