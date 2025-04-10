namespace InfonetCSCR.Web.Services;

public class PurchasingAndReceivingService : IPurchasingAndReceivingService
{
  private readonly IHttpService _http;
  private readonly IApplicationConfig _config;
  private readonly IPolicyService _policyService;
  private readonly IHttpContextAccessor _contextAccessor;

  public PurchasingAndReceivingService(
    IHttpService http,
    IApplicationConfig config,
    IPolicyService policyService,
    IHttpContextAccessor contextAccessor)
  {
    _http = http;
    _config = config;
    _policyService = policyService;
    _contextAccessor = contextAccessor;
  }

  public PurchasingAndReceivingPoliciesViewModel GetPolicies()
  {
    PurchasingAndReceivingPoliciesViewModel policies = new()
    {
      Purchase = _policyService.GetPolicy(PolicyName.PURCHASE),
      PChangeInPO = _policyService.GetPolicy(PolicyName.PCHNG_IN_PO),
      PackCostPolicy = _policyService.GetPolicy(PolicyName.PACKCOST),
      UpdateAllVendor = _policyService.GetPolicy(PolicyName.UPD_NETW),
      SetVend = _policyService.GetPolicy(PolicyName.SET_VEND),
      ExptTaxPolicy = _policyService.GetPolicy(PolicyName.EXPT_TAX),
      KitsInPoPolicy = _policyService.GetPolicy(PolicyName.KITS_IN_PO),
      CostDecPolicy = _policyService.GetPolicy(PolicyName.COST_DEC),
      ConfirmPC = _policyService.GetPolicy(PolicyName.CONFIRM_PC),
      DefaultMargin = _policyService.GetPolicy(PolicyName.DEF_PR_CHG)
    };

    return policies;
  }

  public async Task<TransferPOFileViewModel?> POPayment(PurchaseAndReceivePostViewModel purchaseAndReceivePostViewModel, bool costChange)
  {
    string userCode = _contextAccessor.HttpContext?.Session?.GetString("UserCode")!;
    purchaseAndReceivePostViewModel.PurchasingAndReceivingPolicies = GetPolicies();
    purchaseAndReceivePostViewModel.CostChange = costChange;
    purchaseAndReceivePostViewModel.UserCode = userCode;

    string url = @$"PurchaseHeader/POPayment";
    var response = await _http.PostAsync(url, purchaseAndReceivePostViewModel);
    long? purchaseNo = purchaseAndReceivePostViewModel.PurchaseNo;

    if (!response.IsSuccessStatusCode)
    {
      throw new HttpRequestException($"Request failed POST {url} - Status code: {(int)response.StatusCode}.", null, response.StatusCode);
    }

    // Prepare File Content

    url = @$"CompanySetup/GetCompanyStoreCode";
    response = await _http.GetAsync(url);
    string? storeCode = null;
    if (response.IsSuccessStatusCode)
    {
      storeCode = await response.Content.ReadAsStringAsync();
    }

    storeCode ??= "";

    string headerFile = "PO" + DateTime.Now.ToString("yyMMdd");
    string detailFile = "PD" + DateTime.Now.ToString("yyMMdd");
    string outputFolderPath = _config.FileConfig.POPaymentOutputFolderPath;
    string archiveFolderPath = _config.FileConfig.POPaymentArchiveFolderPath;

    string filename = headerFile + "??." + storeCode;

    DirectoryInfo outputDirectory = new(outputFolderPath);
    DirectoryInfo archiveDirectory = new(archiveFolderPath);

    if (outputFolderPath != null && !Directory.Exists(outputFolderPath))
    {
      Directory.CreateDirectory(outputFolderPath);
    }
    FileInfo[] outputFiles = outputDirectory.GetFiles();

    bool found = false;
    foreach (FileInfo file in outputFiles)
    {
      if (file.Name.Length > 10 && file.Name[..7] == filename[..7])
        if (file.Name[10..] == storeCode)
        {
          headerFile = file.Name;
          found = true;
        }
    }

    if (found)
    {
      detailFile += headerFile.Substring(8, 2) + "." + storeCode;
    }
    else
    {
      if (archiveFolderPath != null && !Directory.Exists(archiveFolderPath))
      {
        Directory.CreateDirectory(archiveFolderPath);
      }

      FileInfo[] archiveFiles = archiveDirectory.GetFiles(); //Getting Text files
      int sequence = archiveFiles.Length + 1;

      headerFile += sequence.ToString("00") + "." + storeCode;
      detailFile += sequence.ToString("00") + "." + storeCode;
    }

    url = @$"PurchaseHeader/GetPOTransferFilesData/{headerFile}/{detailFile}";
    var poFileViewModel = await _http.PostAndParseAsync<TransferPOFileViewModel?>(url, purchaseNo);

    StringBuilder detailFileContent = new();
    StringBuilder headerFileContent = new();
    bool isFirstLine = true;

    Dictionary<long, double> inventoryAmounts = new();
    Dictionary<long, double> supplierAmounts = new();
    Dictionary<(long, int), bool> alreadyAddedInventoryAmounts = new();
    Dictionary<(long, int), bool> alreadyAddedSupplierAmounts = new();
    Dictionary<(long, int), bool> alreadyAddedDFC = new();

    if (poFileViewModel?.PurchaseDetails != null && (bool)poFileViewModel?.PurchaseDetails.Any()!)
    {
      foreach (PurchaseDetailViewModel purchaseDetailViewModel in poFileViewModel?.PurchaseDetails!)
      {
        StockItemViewModel POStock = new()
        {
          StockCode = purchaseDetailViewModel?.StockCode!
        };

        bool supplyDept = _policyService.GetPolicy(PolicyName.SupplyDept, POStock);

        if (!inventoryAmounts.ContainsKey(purchaseDetailViewModel.PurchaseNo))
          inventoryAmounts.Add(purchaseDetailViewModel.PurchaseNo, 0);
        if (!supplierAmounts.ContainsKey(purchaseDetailViewModel.PurchaseNo))
          supplierAmounts.Add(purchaseDetailViewModel.PurchaseNo, 0);

        if (supplyDept)
        {
          if (!alreadyAddedSupplierAmounts.ContainsKey((purchaseDetailViewModel.PurchaseNo, purchaseDetailViewModel.LineNumber)))
          {
            supplierAmounts[purchaseDetailViewModel.PurchaseNo] += purchaseDetailViewModel?.ExtCost ?? 0;
            alreadyAddedSupplierAmounts.Add((purchaseDetailViewModel.PurchaseNo, purchaseDetailViewModel.LineNumber), true);
          }
        }
        else if (!alreadyAddedInventoryAmounts.ContainsKey((purchaseDetailViewModel.PurchaseNo, purchaseDetailViewModel.LineNumber)))
        {
          inventoryAmounts[purchaseDetailViewModel.PurchaseNo] += purchaseDetailViewModel?.ExtCost ?? 0;
          alreadyAddedInventoryAmounts.Add((purchaseDetailViewModel.PurchaseNo, purchaseDetailViewModel.LineNumber), true);
        }
      }

      foreach (PurchaseDetailViewModel purchaseDetail in poFileViewModel?.PurchaseDetails!)
      {
        if (alreadyAddedDFC.ContainsKey((purchaseDetail.PurchaseNo, purchaseDetail.LineNumber)))
        {
          continue;
        }

        detailFileContent.Append(isFirstLine ? "" : "\n");
        detailFileContent.Append(poFileViewModel?.StoreCode.PadRightFixed(3));
        detailFileContent.Append(purchaseDetail.PurchaseNo.ToString().PadRightFixed(5));
        detailFileContent.Append(purchaseDetail.LineNumber.ToString().PadRightFixed(5));
        detailFileContent.Append(purchaseDetail?.PartNumber.PadRightFixed(10));
        detailFileContent.Append(purchaseDetail?.OrderedQuantity.ToString().FormatSigned(8));
        detailFileContent.Append(purchaseDetail?.UnitOfMeasure.PadRightFixed(2));
        detailFileContent.Append(purchaseDetail?.UnitQuantity.ToString().FormatSigned(8));
        detailFileContent.Append(purchaseDetail?.ReceivedQuantity.ToString().FormatSigned(8));
        detailFileContent.Append(purchaseDetail?.Cost.ToString().FormatSigned(6, 2));
        detailFileContent.Append(purchaseDetail?.Description.PadRightFixed(35));

        double extCost = (purchaseDetail?.ExtCost) ?? 0;
        double calculateDiscount = purchaseDetail?.CalculateDiscount ?? 0;
        double orderedQty = purchaseDetail?.OrderedQuantity ?? 0;

        detailFileContent.Append((extCost - (calculateDiscount * orderedQty)).ToString().FormatSigned(6, 2));
        detailFileContent.Append(purchaseDetail?.GST?.ToString().FormatSigned(6, 2));
        detailFileContent.Append(purchaseDetail?.StockCode.PadRightFixed(17));
        detailFileContent.Append(purchaseDetail?.Department?.PadLeftFixed(3, '0'));
        detailFileContent.Append(purchaseDetail?.SubDepartment?.PadLeftFixed(3, '0'));

        alreadyAddedDFC.Add(((int)purchaseDetail.PurchaseNo, purchaseDetail.LineNumber), true);

        isFirstLine = false;
      }
    }

    isFirstLine = true;
    foreach (var purchaseHeader in poFileViewModel?.PurchaseHeaders!)
    {
      headerFileContent.Append(isFirstLine ? "" : "\n");
      headerFileContent.Append(poFileViewModel?.StoreCode.FormatNumeric(3));
      headerFileContent.Append(purchaseHeader.ReceivedDate?.ToString("yyMMdd"));
      headerFileContent.Append(purchaseHeader.AccountNo.PadRightFixed(10));
      headerFileContent.Append(purchaseHeader.VendorName.PadRightFixed(25));
      headerFileContent.Append(purchaseHeader.PurchaseNo.ToString().FormatNumeric(5));
      headerFileContent.Append((inventoryAmounts.TryGetValue((int)purchaseHeader.PurchaseNo, out var inventoryAmount) ? inventoryAmount : 0 - purchaseHeader?.CalculateDiscount).ToString().FormatSigned(7, 2));
      headerFileContent.Append((supplierAmounts.TryGetValue((int)purchaseHeader.PurchaseNo, out var supplierAmount) ? supplierAmount : 0).ToString().FormatSigned(7, 2));

      headerFileContent.Append(((inventoryAmounts.TryGetValue((int)purchaseHeader.PurchaseNo, out inventoryAmount) ? inventoryAmount : 0)
                                - purchaseHeader?.CalculateDiscount
                                + (supplierAmounts.TryGetValue((int)purchaseHeader.PurchaseNo, out supplierAmount) ? supplierAmount : 0))
                                .ToString().FormatSigned(7, 2));
      headerFileContent.Append(purchaseHeader.SurChargeTotal.ToString().FormatSigned(7, 2));
      headerFileContent.Append(purchaseHeader.TotalGST.ToString().FormatSigned(7, 2));
      headerFileContent.Append(purchaseHeader.PurchaseTotal.ToString().FormatSigned(7, 2));
      headerFileContent.Append(purchaseHeader.PayType.PadRightFixed(6));
      headerFileContent.Append(purchaseHeader.InvoiceNo.PadRightFixed(20));
      isFirstLine = false;
    }

    poFileViewModel.DetailFileContent = detailFileContent.ToString();
    poFileViewModel.HeaderFileContent = headerFileContent.ToString();

    return poFileViewModel;
  }
}
