namespace InfonetCSCR.Web.Services;

public class PrinterService : IPrinterService
{
  public PrinterService() { }

  public List<string>? GetPrinters()
  {
    ProcessStartInfo psi = new()
    {
      FileName = "wmic",
      Arguments = "printer get name",
      RedirectStandardOutput = true,
      UseShellExecute = false,
      CreateNoWindow = true
    };

    using Process process = new() { StartInfo = psi };
    process.Start();

    string output = process.StandardOutput.ReadToEnd();

    process.WaitForExit();

    List<string> printerNames = new();

    string[] lines = output.Split('\n');

    foreach (string line in lines[1..])
    {
      string trimmedLine = line.Trim();
      if (!string.IsNullOrWhiteSpace(trimmedLine))
      {
        printerNames.Add(trimmedLine);
      }
    }

    return printerNames;
  }

  public List<string>? GetFonts()
  {
    List<string> fonts = new();

    using (InstalledFontCollection fontsCollection = new())
    {
      System.Drawing.FontFamily[] fontFamilies = fontsCollection.Families;
      foreach (System.Drawing.FontFamily font in fontFamilies)
      {
        fonts.Add(font.Name);
      }
    }

    return fonts;
  }
}
