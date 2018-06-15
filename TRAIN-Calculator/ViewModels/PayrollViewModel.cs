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
        public PaySlipCompensation PaySlipCompensations { get; set; }
        public PaySlipDeduction PaySlipDeductions { get; set; }
        public PaySlipDeMinimis PaySlipDeMinimis { get; set; }

    }

    
}