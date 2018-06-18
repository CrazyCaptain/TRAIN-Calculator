using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TRAIN_Calculator.Models;

namespace TRAIN_Calculator.ViewModels
{
    public class PayrollViewModel
    {
        public PaySlip PaySlips { get; set; }
        public List<PayrollCompensationViewModel> Compensations { get; set; }
        public List<PayrollDeMinimisViewModel> DeMinimis { get; set; }
        public List<PayrollDeductionViewModel> Deductions { get; set; }
    }

    public class PayrollViewModelItem
    {
        public decimal Value { get; set; }
    }

    public class PayrollCompensationViewModel : PayrollViewModelItem
    {
        public CompensationType Compensation { get; set; }
    }

    public class PayrollDeMinimisViewModel : PayrollViewModelItem
    {
        public DeMinimisType DeMinimis { get; set; }
    }

    public class PayrollDeductionViewModel : PayrollViewModelItem
    {
        public DeductionType Deduction { get; set; }
    }

}