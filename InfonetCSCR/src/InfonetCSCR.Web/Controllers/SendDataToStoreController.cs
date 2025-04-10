namespace InfonetCSCR.Web.Controllers;

[Authorize]
public class SendDataToStoreController : Controller
{
  private readonly ISendDataToStoreService _sendDataToStoreService;

  public SendDataToStoreController(ISendDataToStoreService sendDataToStoreService)
  {
    _sendDataToStoreService = sendDataToStoreService;
  }

  public async Task<IActionResult> Index()
  {
    ViewBag.Regions = await _sendDataToStoreService.GetRegionsAsync();
    ViewBag.Stores = await _sendDataToStoreService.GetAllStoresListAsync();
    ViewBag.DataTransferTypeList = await _sendDataToStoreService.GetDataTransferTypeListAsync();
    return View();
  }

  public async Task<IActionResult> GetStoresByRegion([FromBody] IEnumerable<string> regionIds)
  {
    return Ok(await _sendDataToStoreService.GetStoresByRegionAsync(regionIds));
  }

  public async Task<IActionResult> GetItems(string tableName)
  {
    return Ok(await _sendDataToStoreService.GetItemsAsync(tableName));
  }

  public async Task<IActionResult> RemoveItemFromTransferDatabase([FromBody] RemoveItemRequestViewModel request)
  {
    return Ok(await _sendDataToStoreService.RemoveItemFromTransferDatabaseAsync(request));
  }

  public async Task<IActionResult> RemoveSelectedStoresFromTransferDatabase(string tableName)
  {
    return Ok(await _sendDataToStoreService.RemoveSelectedStoresFromTransferDatabaseAsync(tableName));
  }

  public async Task<IActionResult> CreateTable([FromBody] SendDataToStoreItems request)
  {
    return Ok(await _sendDataToStoreService.CreateTableAsync(request));
  }

  public async Task<IActionResult> DeleteExpTableData(string tableName)
  {
    return Ok(await _sendDataToStoreService.DeleteExpTableDataAsync(tableName));
  }

  public async Task<IActionResult> TransferData([FromBody] SendDataToStoreItems request)
  {
    return Ok(await _sendDataToStoreService.TransferDataAsync(request));
  }

  public async Task<IActionResult> UpdateFlag([FromBody] SendDataToStoreItems request)
  {
    return Ok(await _sendDataToStoreService.UpdateFlagAsync(request));
  }

  public async Task<IActionResult> GenerateTextReportPDF([FromBody] string response)
  {
    using (var stream = new MemoryStream())
    {
      PdfDocument document = new PdfDocument();
      PdfPage page = document.AddPage();
      XGraphics gfx = XGraphics.FromPdfPage(page);
      // Use a monospaced font with a properly sized font
      XFont font = new XFont("Consolas", 8, XFontStyle.Regular);
      double yPosition = 40; // Start Y position for the content
      double marginLeft = 30; // Left margin (30px as per your requirement)
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
        // Draw the line of text with the custom darker color, left-aligned
        gfx.DrawString(line, font, darkBrush, new XPoint(marginLeft, yPosition));
        // Increment yPosition for the next line, adjusting line spacing to reduce overflow
        yPosition += font.Height + 2; // Slightly tighter line spacing
      }
      document.Save(stream, false);
      byte[] pdfBytes = stream.ToArray();
      return File(pdfBytes, "application/pdf", "Report.pdf");
    }
  }
}
