namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class TextReportsController : Controller
{
  private readonly ITextReportsService _textReportsService;

  #region Private Methods

  private int GetFontSize(string reportName)
  {
    switch (reportName.ToUpper())
    {
      case "T0002":
      case "T0009":
      case "T0010":
      case "T0012":
      case "T0018":
      case "T0019":
        return 7;
      case "T0003":
      case "T0005":
      case "T0008":
      case "T0011":
      case "T0015":
      case "T0016":
        return 8;
      case "T0007":
      case "T0014":
      case "T0017":
        return 9;
      case "T0001":
      case "T0006":
      case "T0013":
        return 10;
      case "T0004":
        return 11;
      default:
        return 7;
    }
  }


  #endregion

  public TextReportsController(ITextReportsService textReportsService)
  {
    _textReportsService = textReportsService;
  }

  public async Task<IActionResult> Index()
  {
    ViewBag.AllTextReporsList = await _textReportsService.GetAllTextReportsListAsync();
    ViewBag.AllStoresList = await _textReportsService.GetAllStoresListAsync();
    ViewBag.CurrentDate = DateTime.Now.ToString("yyyy-MM-dd");
    return View();
  }

  public async Task<string> GetTextReport(TextReportParamsViewModel textReportParams)
  {
    string response = await _textReportsService.GetTextReport(textReportParams);
    return response;
  }

  public async Task<IActionResult> GenerateTextReportPDF([FromBody] TextReportParamsViewModel textReportParams)
  {
    // Fetch the text report content (response)
    string response = await _textReportsService.GetTextReport(textReportParams);

    using (var stream = new MemoryStream())
    {
      PdfDocument document = new PdfDocument();
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);

      // Use a monospaced font with a properly sized font
      XFont font = new XFont("Consolas", GetFontSize(textReportParams.ReportName), XFontStyle.Regular);

      double yPosition = 40; // Start Y position for the content
      double pageWidth = page.Width;

      // Split the string response into lines
      var lines = response.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

      // Darker font color (using XSolidBrush with a custom dark color)
      XBrush darkBrush = new XSolidBrush(XColor.FromArgb(0, 0, 0)); // Pure black color

      foreach (var line in lines)
      {
        // Check if we need to add a new page when the current yPosition exceeds the page height
        if (yPosition + font.Height > page.Height - 40) // Ensure we don't overflow
        {
          page = document.AddPage();
          gfx = XGraphics.FromPdfPage(page);
          yPosition = 40; // Reset yPosition for the new page
        }

        // Center-align the entire line on the page
        double textWidth = gfx.MeasureString(line, font).Width;
        double xPosition = (pageWidth - textWidth) / 2;

        // Draw the line of text with the custom darker color
        gfx.DrawString(line, font, darkBrush, new XPoint(xPosition, yPosition));

        // Increment yPosition for the next line, adjusting line spacing to reduce overflow
        yPosition += font.Height + 2; // Slightly tighter line spacing
      }

      document.Save(stream, false);

      byte[] pdfBytes = stream.ToArray();
      return File(pdfBytes, "application/pdf", "TextReport.pdf");
    }
  }
}
