namespace InfonetCSCR.Web.Abstractions.Services;

public interface IPurchasingAndReceivingService
{
  PurchasingAndReceivingPoliciesViewModel GetPolicies();
  Task<TransferPOFileViewModel?> POPayment(PurchaseAndReceivePostViewModel purchaseAndReceivePostViewModel, bool costChange);
}
