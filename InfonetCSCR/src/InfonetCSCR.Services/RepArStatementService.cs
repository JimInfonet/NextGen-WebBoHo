

using Microsoft.VisualBasic;
using System.Collections.Generic;

namespace InfonetCSCR.Services
{
    public class RepArStatService : IRepArStatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationConfig _config;
        //private readonly IPolicyService _policy;

        public RepArStatService(IApplicationConfig _applicationConfig)
        {
            _config = _applicationConfig;
            this._unitOfWork = new UnitOfWork(_applicationConfig, _applicationConfig.CSCMasterConnectionString);
        }

        public async Task<IEnumerable<CustomersDto>> GetAllCustomers(bool orderByCode)
        {
            IEnumerable<CustomersDto> customers = await _unitOfWork.RepArStat.GetAllCustomersAsync(orderByCode);
            _unitOfWork.Commit();
            return customers;
        }
        public void printPolicies(GetRepArStatDto model)
        {
            Console.WriteLine($"BaseCurr:{model.policies.BaseCurr}");
            Console.WriteLine($"CBonusName:{model.policies.CBonusName}");
            Console.WriteLine($"CBonusTend:{model.policies.CBonusTend}");
            Console.WriteLine($"CashBonus:{model.policies.CashBonus}");
            Console.WriteLine($"CcMode:{model.policies.CcMode}");
            Console.WriteLine($"CombineCr:{model.policies.CombineCr}");
            Console.WriteLine($"CombineFleet:{model.policies.CombineFleet}");
            Console.WriteLine($"CountCash:{model.policies.CountCash}");
            Console.WriteLine($"CountType:{model.policies.CountType}");
            Console.WriteLine($"CloseIncluded:{model.policies.CloseIncluded}");
            Console.WriteLine($"EodGroup:{model.policies.EodGroup}");
            Console.WriteLine($"PrtTillClCol:{model.policies.PrtTillClCol}");
            Console.WriteLine($"ShowAllExempt:{model.policies.ShowAllExempt}");
            Console.WriteLine($"SupportEzi:{model.policies.SupportEzi}");
            Console.WriteLine($"Show_NRGT:{model.policies.Show_NRGT}");
            Console.WriteLine($"ShowCodes:{model.policies.ShowCodes}");
            Console.WriteLine($"TaxRebate:{model.policies.TaxRebate}");
            Console.WriteLine($"TaxExempt:{model.policies.TaxExempt}");
            Console.WriteLine($"Te_Type:{model.policies.Te_Type}");
            Console.WriteLine($"TidelSafe:{model.policies.TidelSafe}");
            Console.WriteLine($"TillCDays:{model.policies.TillCDays}");
            Console.WriteLine($"UseOvrPymnt:{model.policies.UseOvrPymnt}");
            Console.WriteLine($"UseShifts:{model.policies.UseShifts}");

        }

        public async Task<ArStatementDto> GetRepArStat(string client, DateTime dDate)
        {
            var result = await _unitOfWork.RepArStat.GetRepArStat(client, dDate);
            _unitOfWork.Commit();

            return result;
        }

        public async Task<bool> GenMonthClosing(string client, DateTime dDate)
        {
            var result = await _unitOfWork.RepArStat.GenMonthClosing(client, dDate);
            _unitOfWork.Commit();

            return result;
        }

        //Task<string[]> IRepArStatService.GetRepArStat(string sCode, DateTime dDate)
        //{
        //    throw new NotImplementedException();
        //}
    }
}
