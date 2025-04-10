namespace InfonetCSCR.Web.Abstractions.Services;

public interface IAssociatedChargesService
{
  Task<IEnumerable<AssocViewModel>?> GetAssociations();
  Task<bool> AddAssociation(AssocViewModel assocViewModel);
  Task<bool> UpdateAssociation(AssocViewModel assocViewModel);
  Task<bool> DeleteAssociation(string code);
  Task<bool> IsAssociationExists(string code);
  Task<IEnumerable<TaxViewModel>?> GetTaxes(string code);
  Task<IEnumerable<TaxViewModel>?> GetAvailableTaxes(string code);
  Task<bool> AddTax(AssocTaxViewModel assocTax);
  Task<bool> DeleteTax(AssocTaxViewModel assocTax);
  Task<bool> IsChargeInUse(string code);
}
