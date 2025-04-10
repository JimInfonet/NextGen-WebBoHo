namespace InfonetCSCR.Core.Abstractions.Services;

public interface IPurchaseHeaderService
{
  Task<IEnumerable<PurchaseHeader>> GetPurchaseHeadersByPOStatus(string poStatus, int columnIndex);
  Task<bool> AddPurchaseHeader(PurchaseHeaderRecord purchaseHeaderRecord, Vendor vendor);
  Task<bool> UpdatePurchaseHeaderEmailedStatus(PurchaseHeader purchaseheader);
  Task<PurchaseHeader?> GetPurchaseHeadersByPurchaseNo(string purchaseNo);
  Task<bool> UpdatePurchaseHeader(PurchaseHeaderUpdateRecord purchaseHeaderUpdateRecord);
  Task<bool> UpdatePurchaseHeaderSurChargeDiscount(PurchaseHeader purchaseheader);
  Task<PurchaseHeader?> GetPurchaseHeadersByPurchaseNo(long? purchaseNo);
  Task<string> POPayment(PurchaseAndReceivePostDto purchaseAndReceivePostDto);
  Task<TransferPOFileDto?> GetPOTransferFilesData(long purchaseNo, string headerFile, string detailFile);
  }
