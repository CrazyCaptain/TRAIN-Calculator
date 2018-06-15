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
        public List<PayrollDeductionViewModel> Deductions { get; set; }
    }

    public class PayrollCompensationViewModel
    {
        public Compensations Compensation { get; set; }
        public decimal Value { get; set; }
    }

    public class PayrollDeductionViewModel
    {
        public Deductions Deduction { get; set; }
        public decimal Value { get; set; }
    }

}