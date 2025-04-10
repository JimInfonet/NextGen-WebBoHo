namespace InfonetCSCR.Web.Models;

public class ReadNegativeCardLogViewModel
{
  public int NumberOfLines { get; set; }
  public int NumberOfLinesAddedToNegativeCard { get; set; }
  public int NumberOfLinesAddedToPositiveCard { get; set; }
  public int NumberOfLinesDeletedFromNegativeCard { get; set; }
  public int NumberOfLinesDeletedFromPositiveCard { get; set; }
  public int NumberOfLinesCannotAddedToNegativeCard { get; set; }
  public int NumberOfLinesCannotAddedToPositiveCard { get; set; }
  public int NumberOfLinesCannotDeletedFromNegativeCard { get; set; }
  public int NumberOfLinesCannotDeletedFromPositiveCard { get; set; }
  public int NumberOfLinesErrorFromFile { get; set; }
  public string? FilePath { get; set; }
  public string? ErrorFilePath { get; set; }
}
