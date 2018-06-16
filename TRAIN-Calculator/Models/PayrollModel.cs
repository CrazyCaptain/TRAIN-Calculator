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
        [Display(Name = "Earner Type")]
        public EarnerType? EarnerType { get; set; }

        [Required]
        [Display(Name = "Total Hours Worked")]
        public int TotalHoursWorked { get; set; }

        [Display(Name = "Expected Work Hours")]
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

        [Display(Name = "Total De Minimis")]
        public decimal TotalDeMinimis { get; set; }

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

        [Display(Name = "Holiday Pay")]
        public decimal HolidayPay { get; set; }
        public decimal ECOLA { get; set; }


        [Display(Name = "Thirteen Month Pay")]
        public decimal ThirteenMonthPay { get; set; }


        public CompensationType? Compensations { get; set; }
    }

    public class PaySlipDeduction
    {
        public decimal SSS { get; set; }
        public decimal PHIC { get; set; }
        public decimal PAGIBIG { get; set; }

        [Display(Name = "SSS Loan")]
        public decimal SSSLoan { get; set; }
     
        [Display(Name = "PAGIBIG Loan")]
        public decimal PAGIBIGLoan { get; set; }

        [Display(Name = "Retirement")]
        public decimal Retirement { get; set; }

        [Display(Name = "Cash Advances")]
        public decimal CashAdvances { get; set; }


        public DeductionType? Deductions { get; set; }

    }

    public class PaySlipDeMinimis
    {

        [Display(Name = "Unused Vacation Leave")]
        public decimal UnusedVacationLeave { get; set; }

        [Display(Name = "Vacation and Sick Leave Credits")]
        public decimal VacationandSickLeaveCredits { get; set; }

        [Display(Name = "Medical Allowance to Employee Dependents")]
        public decimal MedicalAllowancetoEmployeeDependents { get; set; }

        [Display(Name = "Rice Subsidy")]
        public decimal RiceSubsidy { get; set; }

        [Display(Name = "Uniform and Clothing")]
        public decimal UniformandClothing { get; set; }

        [Display(Name = "Medical Expenses")]
        public decimal MedicalExpenses { get; set; }

        [Display(Name = "Laundry Allowance")]
        public decimal LaundryAllowance { get; set; }

        [Display(Name = "Achievement Awards")]
        public decimal AchievementAwards { get; set; }

        [Display(Name = "Christmas Gifts")]
        public decimal ChristmasGifts { get; set; }

        [Display(Name = "Daily Meal Allowance")]
        public decimal DailyMealAllowance { get; set; }

        [Display(Name = "Collective Bargaining Agreement")]
        public decimal CollectiveBargainingAgreement { get; set; }


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

    public enum CompensationType : byte
    {
        [Display(Name = "Overtime Pay")]
        OvertimePay = 0,

        [Display(Name = "Allowance")]
        Allowance = 1,
        
        [Display(Name = "Holiday Pay")]
        HolidayPay = 2,

        [Display(Name = "ECOLA")]
        ECOLA = 3,

        [Display(Name = "Thirteenth Month Pay")]
        ThirteenthMonthPay = 4,
    }

    public enum DeMinimisType : byte
    {
        [Display(Name = "Unused Vacation Leave")]
        UnusedVacationLeave = 0,

        [Display(Name = "Vacation and Sick Leave Credits")]
        VacationandSickLeaveCredits = 1,

        [Display(Name = "Medical Allowance to Employee Dependents")]
        MedicalAllowancetoEmployeeDependents = 2,

        [Display(Name = "Rice Subsidy")]
        RiceSubsidy = 3,

        [Display(Name = "Uniform and Clothing")]
        UniformandClothing = 4,

        [Display(Name = "Medical Expenses")]
        MedicalExpenses = 5,

        [Display(Name = "Laundry Allowance")]
        LaundryAllowance = 6,

        [Display(Name = "Achievement Awards")]
        AchievementAwards = 7,

        [Display(Name = "Christmas Gifts")]
        ChristmasGifts = 8,

        [Display(Name = "Daily Meal Allowance")]
        DailyMealAllowance = 9,

        [Display(Name = "Collective Bargaining Agreement")]
        CollectiveBargainingAgreement = 10,
    }

    public enum DeductionType : byte
    {
        [Display(Name = "SSS")]
        SSS = 0,

        [Display(Name = "PAGIBIG")]
        PAGIBIG = 1,

        [Display(Name = "PHIC,")]
        PHIC = 2,
        
        [Display(Name = "SSS Loan")]
        SSSLoan = 3,

        [Display(Name = "PAGIBIG Loan")]
        PAGIBIGLoan = 4,

        [Display(Name = "Retirement")]
        Retirement = 5,

        [Display(Name = "Cash Advances")]
        CashAdvances = 6
    }

}