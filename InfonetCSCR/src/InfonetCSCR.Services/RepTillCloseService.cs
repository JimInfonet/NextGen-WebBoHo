namespace InfonetCSCR.Services
{
    public class RepTillCloseService : IRepTillCloseService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IApplicationConfig _config;
        private readonly IPolicyService _policy;

        public RepTillCloseService(IApplicationConfig config,
                                    IPolicyService policy)
        {
            _config = config;
            _policy = policy;
            this._unitOfWork = new UnitOfWork(_config, _config.CSCMasterConnectionString);
        }

        public void printPolicies(GetRepTillCloseDto model)
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
        
        //public async Task<IEnumerable<RepTillClose>> GetRepTillCloses()
        //{
        //    IEnumerable<RepTillClose> repTillCloses = await _unitOfWork.RepTillCloses.GetRepTillCloses();
        //    _unitOfWork.Commit();

        //    return repTillCloses;
        //}
        public async Task<IEnumerable<RepTillClose>> GetRepTillCloses(DateTime date1, DateTime date2)
        {
            IEnumerable<RepTillClose> repTillCloses = await _unitOfWork.RepTillCloses.GetRepTillCloses(date1, date2);
            _unitOfWork.Commit();

            return repTillCloses;
        }
        public async Task<IEnumerable<int>> GetShiftNos(DateTime shiftDate)
        {
            IEnumerable<int> shifts = await _unitOfWork.RepTillCloses.GetShiftNos(shiftDate);
            _unitOfWork.Commit();

            return shifts;
        }

        //public async Task<string[]> GetRepByShift(GetRepTillCloseDto model, string shiftDate, string shift)
        public async Task<string[]> GetRepByShift(GetRepTillCloseDto model)
        {
            printPolicies(model);

            string[] TillClose = await _unitOfWork.RepTillCloses.GetRepByShift(model);
            _unitOfWork.Commit();

            return TillClose;
        }

        //public async Task<string[]> GetRepByDates(GetRepTillCloseDto model, string date1, string date2)
        public async Task<string[]> GetRepByDates(GetRepTillCloseDto model)
        {
            printPolicies(model);

            string[] TillClose = await _unitOfWork.RepTillCloses.GetRepByDates(model);
            _unitOfWork.Commit();

            return TillClose;
        }

        public async Task<string[]> GetRepByTillNo(GetRepTillCloseDto model)
        {
            printPolicies(model);

            string[] TillClose = await _unitOfWork.RepTillCloses.GetRepByTillNo(model);
            _unitOfWork.Commit();

            return TillClose;
        }

    }
}
