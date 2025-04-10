namespace InfonetCSCR.Services;

public class HotButtonPagesService : IHotButtonPagesService
{
  private readonly IUnitOfWork _unitOfWork;

  public HotButtonPagesService(IApplicationConfig applicationConfig)
  {
    this._unitOfWork = new UnitOfWork(applicationConfig, applicationConfig.CSCMasterConnectionString);
  }

  public async Task<IList<HotButtonPages>> GetAllHotButtonPages()
  {
    IList<HotButtonPages> hotButtonPages = await _unitOfWork.HotButtonPages.GetAllHotButtonPagesAsync();
    _unitOfWork.Commit();
    return hotButtonPages;
  }

  public async Task<bool> UpdateHotButtonPage(HotButtonPages hotButtonPages)
  {
    bool isDone = false;
    HotButtonPages? existingHotButtonPageData = await _unitOfWork.HotButtonPages.GetHotButtonPageByIdAsync(hotButtonPages.PageID);
    
    if (existingHotButtonPageData != null)
    {
      isDone = await _unitOfWork.HotButtonPages.UpdateHotButtonPageAsync(hotButtonPages);
    }
    else
    {
      isDone = await _unitOfWork.HotButtonPages.InsertHotButtonPageAsync(hotButtonPages);
    }

    _unitOfWork.Commit();
    return isDone;
  }
}
