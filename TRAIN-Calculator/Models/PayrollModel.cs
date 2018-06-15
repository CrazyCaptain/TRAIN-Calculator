using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace TRAIN_Calculator.Models
{
    public class PaySlip
    {
        [Required]
        public EarnerType? EarnerType { get; set; }

        [Required]
        [Display(Name = "Total Hours Worked")]
        public int TotalHoursWorked { get; set; }

        [Display(Name = "Expected Worked Hours")]
        public int ExpectedWorkedHours { get; set; }

        
        
        [Display(Name = "Overtime Worked Hours")]
        public int? OvertimeWorkedHours { get; set; }

       
        [Display(Name = "Overtime Rate Per Hour")]
        public decimal? OvertimeRatePerHour { get; set; }

        [Range(1, 1000000, ErrorMessage = "Basic Salary Should not be zero.")]
        [Display(Name = "Basic Salary")]
        public decimal BasicSalary { get; set; }

        [Display(Name = "Total Compensations")]
        public decimal TotalCompensations { get; set; }

        [Display(Name = "Total Deductions")]
        public decimal TotalDeductions { get; set; }

        [Display(Name = "Taxable Income")]
        public decimal TaxableIncome { get; set; }

        [Display(Name = "WithHolding Tax")]
        public decimal WithHoldingTax { get; set; }
        public decimal TakeHomePay { get; set; }
    }

    public class PaySlipCompensation
    {
        [Display(Name = "Overtime Pay")]
        public decimal OvertimePay { get; set; }
        public decimal Bonus { get; set; }
        public decimal Commission { get; set; }
        public decimal Allowance { get; set; }
        public decimal HolidayPay { get; set; }
        public decimal ECOLA { get; set; }

        public Compensations? Compensations { get; set; }
    }

    public class PaySlipDeduction
    {
        public decimal SSS { get; set; }
        public decimal PHIC { get; set; }
        public decimal PAGIBIG { get; set; }

        public decimal SSSLoan { get; set; }
        public decimal PAGIBIGLoan { get; set; }

        public Deductions? Deductions { get; set; }

    }

    public class PaySlipDeMinimis
    {
        
        public decimal RiceAllowance { get; set; }
        public decimal LaundryAllowance { get; set; }
        public DeMinimis? DeMinimis { get; set; }

    }


    public class SSSTable
    {

        public decimal SalaryFrom { get; set; }
        public decimal SalaryTo { get; set; }
        public decimal MSC { get; set; }
        public decimal SS_ER { get; set; }
        public decimal SS_EE { get; set; }
        public decimal TC_ER { get; set; }
        public decimal TC_EE { get; set; }



    }

    public enum EarnerType
    {
        //Daily,
        //Weekly,
        //SemiMonthly,
        Monthly
    }

    public enum Compensations
    {
        Allowance,
        HolidayPay,
        ECOLA
    }

    public enum Deductions
    {
        [Display(Name = "SSS Loan")]
        SSSLoan,
        [Display(Name = "PAGIBIG Loan")]
        PAGIBIGLoan
    }

    public enum DeMinimis
    {
        RiceAllowance,
        LaundryAllowance
    }


}