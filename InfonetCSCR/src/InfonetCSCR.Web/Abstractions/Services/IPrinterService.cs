namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPrinterService
{
  List<string>? GetPrinters();
  List<string>? GetFonts();
}
