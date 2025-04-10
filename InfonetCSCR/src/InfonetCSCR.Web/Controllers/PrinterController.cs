namespace InfonetCSCR.Web.Controllers;
using System.Collections.Generic;

[Authorize]
public class PrinterController : Controller
{
  private readonly IPrinterService _printerService;

  public PrinterController(IPrinterService printerService)
  {
    _printerService = printerService;
  }

  public List<string>? GetPrinters()
  {
    var printers = _printerService.GetPrinters();
    return printers;
  }

  public List<string>? GetFonts()
  {
    var fonts = _printerService.GetFonts();
    return fonts;
  }
}
