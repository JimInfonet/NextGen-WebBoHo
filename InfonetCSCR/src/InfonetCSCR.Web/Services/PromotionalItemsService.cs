namespace InfonetCSCR.Web.Services;

public class PromotionalItemsService : IPromotionalItemsService
{
  private readonly IHttpService _http;

  public PromotionalItemsService(IHttpService http)
  {
    _http = http;
  }

  public async Task<bool> IsPromoIdExist(string promoId)
  {
    string url = $"PromotionalItems/IsPromoIdAlreadyExists?promoId={promoId}";
    var exist = await _http.GetAndParseAsync<bool>(url);
    return exist;
  }

  public async Task SavePromotionalItems(PromoHeaderViewModel model)
  {
    if (model == null || model.PromoDetails == null)
    {
      throw new Exception("There is no available data to insert. Please try again");
    }

    model.PromoDetailViewModels = ProcessPromoDetailProperties(model.PromoDetails, model.PromoId);

    bool isPromoIdExist = await IsPromoIdExist(model.PromoId);

    model.PrType = model.PrType == null ? null : model.PrType == "optDollar" ? "$" : "%";
    model.Price = model.DiscType == "B" ? 0 : model.Price;
    model.Items = AddQty(model.PromoDetailViewModels);

    string promoHeaderUrl = isPromoIdExist ? "PromotionalItems/UpdatePromotionalItemHeader"
                            : "PromotionalItems/AddPromotionalItemHeader";

    var response = await _http.PostAsync(promoHeaderUrl, model);
    if (!response.IsSuccessStatusCode)
    {
      throw new HttpRequestException($"Request failed POST {promoHeaderUrl} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
    }

    if (isPromoIdExist)
    {
      var promoDetailDeleteUrl = "PromotionalItems/DeletePromotionalItemDetails";
      response = await _http.PostAsync(promoDetailDeleteUrl, model.PromoId);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"Request failed POST {promoDetailDeleteUrl} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
      }
    }

    var promoDetailAddUrl = "PromotionalItems/AddPromotionalItemDetails";

    if (isPromoIdExist && response.IsSuccessStatusCode)
    {
      response = await _http.PostAsync(promoDetailAddUrl, model.PromoDetailViewModels);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"Request failed POST {promoDetailAddUrl} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
      }
    }

    if (!isPromoIdExist)
    {
      response = await _http.PostAsync(promoDetailAddUrl, model.PromoDetailViewModels);
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"Request failed POST {promoDetailAddUrl} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
      }
    }
  }

  private IEnumerable<PromoDetailViewModel> ProcessPromoDetailProperties(string promo, string promoId)
  {
    var promoDetails = JsonSerializer.Deserialize<IEnumerable<PromoDetailViewModel>>(promo);

    foreach (var promoDetail in promoDetails)
    {
      promoDetail.PromoID = promoId;
      promoDetail.StockCode ??= "<NONE>";
      promoDetail.Dept ??= "<NONE>";
      promoDetail.SubDept ??= "<NONE>";
      promoDetail.SubDetail ??= "<NONE>";
      promoDetail.Amount ??= 0.00m;
    }

    return promoDetails;
  }

  private int AddQty(IEnumerable<PromoDetailViewModel> promoDetails)
  {
    var qty = 0;
    foreach (var promoDetail in promoDetails)
    {
      qty += promoDetail.Qty.GetValueOrDefault();
    }

    return qty;
  }
}
