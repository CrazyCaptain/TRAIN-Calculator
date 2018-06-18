using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TRAIN_Calculator.ViewModels;
using TRAIN_Calculator.Models;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;

namespace TRAIN_Calculator.Controllers
{
    public class PayrollController : Controller
    {
        // GET: Payroll
        public ActionResult Index(PayrollViewModel model)
        {
            model.PaySlips = new PaySlip();
            model.Compensations = model.Compensations ?? new List<PayrollCompensationViewModel>();
            model.Deductions = model.Deductions ?? new List<PayrollDeductionViewModel>();
            model.DeMinimis = model.DeMinimis ?? new List<PayrollDeMinimisViewModel>();

            SetDropdowns();

            return View(model);
        }

       

        [HttpPost]
        public ActionResult Calculate(PayrollViewModel model)
        {
            model.PaySlips = model.PaySlips;
            model.Compensations = model.Compensations ?? new List<PayrollCompensationViewModel>();
            model.Deductions = model.Deductions ?? new List<PayrollDeductionViewModel>();
            model.DeMinimis = model.DeMinimis ?? new List<PayrollDeMinimisViewModel>();

            if (model.PaySlips == null)
            {
                return View("Index", model);
            }

            if (!ModelState.IsValid)
            {
                if (model.PaySlips.BasicSalary != 0 && model.PaySlips.EarnerType != null)
                    return View("Details", model);
                else
                    return View("Index", model);
            }

            var BasicPay = ComputeExpectedPay(model.PaySlips.BasicSalary, model.PaySlips.TotalHoursWorked, model.PaySlips.ExpectedWorkedHours);

            List<PayrollCompensationViewModel> compensationsList = model.Compensations ?? new List<PayrollCompensationViewModel>();
            List<PayrollDeductionViewModel> deductionsList = model.Deductions ?? new List<PayrollDeductionViewModel>();

            // Default Values 
            var SSS = ComputeSSS(BasicPay);
            var PHIC = ComputePHIC(BasicPay);
            var PAGIBIG = ComputePAGIBIG(BasicPay);
            var OTPAY = ComputeOvertimePay(model.PaySlips.OvertimeWorkedHours == null || model.PaySlips.OvertimeWorkedHours == 0 ? 0 : (int)model.PaySlips.OvertimeWorkedHours, model.PaySlips.OvertimeRatePerHour == null || model.PaySlips.OvertimeRatePerHour == 0 ? 0 : (decimal)model.PaySlips.OvertimeRatePerHour);


            // Add Overtime if applicable    
            if (!compensationsList.Any(m => m.Compensation == CompensationType.OvertimePay) && OTPAY > 0)
            {
                var otpay_value = OTPAY > 0 ? 0 : compensationsList.Where(m => m.Compensation == CompensationType.OvertimePay).SingleOrDefault().Value;
                compensationsList.Add(new PayrollCompensationViewModel()
                {
                    Compensation = CompensationType.OvertimePay,
                    Value = otpay_value > 0 ? otpay_value : OTPAY
                });
            }
            // Add SSS if not already in list           
            if (!deductionsList.Any(m => m.Deduction == DeductionType.SSS))
            {
                deductionsList.Add(new PayrollDeductionViewModel() { Deduction = DeductionType.SSS, Value = SSS });
            }
            // Add PHIC if not already in list
            if (!deductionsList.Any(m => m.Deduction == DeductionType.PHIC))
            {
                deductionsList.Add(new PayrollDeductionViewModel() { Deduction = DeductionType.PHIC, Value = PHIC });
            }
            // Add PAGIBIG if not already in list
            if (!deductionsList.Any(m => m.Deduction == DeductionType.PAGIBIG))
            {
                deductionsList.Add(new PayrollDeductionViewModel() { Deduction = DeductionType.PAGIBIG, Value = PAGIBIG });
            }

         
            model.PaySlips.TotalDeductions = model.Deductions.Sum(m => m.Value);
            model.PaySlips.TotalDeMinimis = ComputeDeMinimis(model.DeMinimis, model.PaySlips, false);         
            model.PaySlips.TotalDeminimisExcess = ComputeDeMinimis(model.DeMinimis, model.PaySlips, true);
            model.PaySlips.TotalCompensations = model.Compensations.Sum(m => m.Value) + BasicPay + model.PaySlips.TotalDeminimisExcess;
            model.PaySlips.TaxableIncome = ComputeTaxableIncome(BasicPay + OTPAY + model.PaySlips.TotalDeminimisExcess, model.Deductions.Select(m => m.Value).ToArray());
            model.PaySlips.WithHoldingTax = ComputeTax(model.PaySlips.TaxableIncome);
            model.PaySlips.TakeHomePay = ComputeTakeHomePay(model.PaySlips.TotalCompensations + model.PaySlips.TotalDeMinimis, model.PaySlips.TotalDeductions, model.PaySlips.WithHoldingTax);

            SetDropdowns();

            return View("Details", model); ;
        }


        public ActionResult Details()
        {
            return View();
        }

        private void SetDropdowns()
        {

            // Set list items for Compensation
            Dictionary<byte, string> compenstationDropdownList = new Dictionary<byte, string>();
            foreach (var item in Enum.GetValues(typeof(CompensationType)))
            {
                compenstationDropdownList.Add((byte)item, item.GetType().GetMember(item.ToString()).First().GetCustomAttribute<DisplayAttribute>().Name);
            }
            ViewBag.CompensationDropdownList = compenstationDropdownList;

            // Set list items for DeMinimis
            Dictionary<byte, string> deMinimisDropdownList = new Dictionary<byte, string>();
            foreach (var item in Enum.GetValues(typeof(DeMinimisType)))
            {
                deMinimisDropdownList.Add((byte)item, item.GetType().GetMember(item.ToString()).First().GetCustomAttribute<DisplayAttribute>().Name);
            }
            ViewBag.DeMinimisDropdownList = deMinimisDropdownList;

            // Set list items for Deduction
            Dictionary<byte, string> deductionDropdownList = new Dictionary<byte, string>();
            foreach (var item in Enum.GetValues(typeof(DeductionType)))
            {
                deductionDropdownList.Add((byte)item, item.GetType().GetMember(item.ToString()).First().GetCustomAttribute<DisplayAttribute>().Name);
            }
            ViewBag.DeductionDropdownList = deductionDropdownList;
        }
        public decimal ComputeSSS(decimal BasicPay)
        {
            decimal SSS = 0;

            string file = Server.MapPath("~/Controllers/SSSTable.json");
            string Json = System.IO.File.ReadAllText(file);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var SSSTable = ser.Deserialize<List<SSSTable>>(Json);

            foreach (var data in SSSTable)
            {
                if (BasicPay >= data.SalaryFrom && BasicPay < data.SalaryTo)
                {
                    SSS = data.TC_EE;
                }
                else if (BasicPay >= data.SalaryFrom && BasicPay >= data.SalaryTo)
                {
                    SSS = data.TC_EE;
                }
            }

            return SSS;
        }

        public decimal ComputePHIC(decimal BasicPay)
        {
            decimal PHIC = 0;

            if (BasicPay < 40000)
            {
                PHIC = (BasicPay * 0.0275M) / 2;
            }
            else
            {
                PHIC = 550;
            }

            return PHIC;
        }

        public decimal ComputePAGIBIG(decimal BasicPay)
        {
            decimal PAGIBIG = 0;

            if (BasicPay <= 1500)
            {
                PAGIBIG = BasicPay * 0.01M;
            }
            else
            {
                PAGIBIG = BasicPay * 0.02M;
            }

            if (PAGIBIG >= 100)
            {
                PAGIBIG = 100;
            }

            return PAGIBIG;
        }

        public decimal ComputeTaxableIncome(decimal TotalCompensation, decimal[] deductibles)
        {
            decimal TaxableIncome = 0;

            TaxableIncome = TotalCompensation - (deductibles.Sum());

            return TaxableIncome;
        }

        public decimal ComputeTax(decimal TaxableIncome)
        {
            decimal TAX = 0;

            if (TaxableIncome <= 20833)
            {
                TAX = 0;
            }
            else if (TaxableIncome > 20833 && TaxableIncome < 33333)
            {
                TAX = (TaxableIncome - 20833) * 0.20M;
            }
            else if (TaxableIncome >= 33333 && TaxableIncome < 66667)
            {
                TAX = 2500 + ((TaxableIncome - 33333) * 0.25M);
            }
            else if (TaxableIncome >= 66667 && TaxableIncome < 166667)
            {
                TAX = 10833.33M + ((TaxableIncome - 66667) * 0.30M);
            }
            else if (TaxableIncome >= 166667 && TaxableIncome < 666667)
            {
                TAX = 40833.33M + ((TaxableIncome - 166667) * 0.32M);
            }
            else if (TaxableIncome >= 666667)
            {
                TAX = 200833.33M + ((TaxableIncome - 666667) * 0.35M);
            }

            return TAX;

        }

        public decimal ComputeExpectedPay(decimal BasicPay, decimal THW, decimal EWH)
        {
            decimal ExpectedPay = 0;

            if (EWH != 0)
            {
                ExpectedPay = (THW / EWH) * BasicPay;
            }

            return ExpectedPay;
        }

        public decimal ComputeOvertimePay(decimal OTWH, decimal OTRPH)
        {
            decimal OTPAY = 0;

            OTPAY = OTWH * OTRPH;

            return OTPAY;
        }

        public decimal ComputeTakeHomePay(decimal TotalCompensations, decimal TotalDeductions, decimal WithHoldingTax)
        {
            decimal TakeHomePay = 0;

            TakeHomePay = TotalCompensations - (TotalDeductions + WithHoldingTax);

            return TakeHomePay;
        }



        public decimal ComputeTotalDeMinimis(PaySlipDeMinimis model)
        {
            decimal Total = 0;

            Total = model.UnusedVacationLeave +
                    model.VacationandSickLeaveCredits +
                    model.MedicalAllowancetoEmployeeDependents +
                    model.RiceSubsidy +
                    model.UniformandClothing +
                    model.MedicalExpenses +
                    model.LaundryAllowance +
                    model.AchievementAwards +
                    model.ChristmasGifts +
                    model.DailyMealAllowance +
                    model.CollectiveBargainingAgreement;

            return Total;
        }

        public decimal ComputeUnusedVacationLeave(decimal BasicPay, int EWH, decimal value)
        {
            decimal ExcessValue = 0;
            decimal MaxVacationLeavePay = (80 / EWH) * BasicPay;

            if (value > MaxVacationLeavePay)
            {
                ExcessValue = value - MaxVacationLeavePay;
            }

            return ExcessValue;
        }

        public decimal ComputeMedicalAllowance(decimal value)
        {
            decimal ExcessValue = 0;

            if (value > 250)
            {
                ExcessValue = value - 250;
            }

            return ExcessValue;
        }

        public decimal ComputeRiceSubsidy(decimal value)
        {
            decimal ExcessValue = 0;

            if (value > 2000)
            {
                ExcessValue = value - 2000;
            }

            return ExcessValue;
        }

        public decimal ComputeUniformAndClothing(decimal value)
        {
            decimal ExcessValue = 0;
            decimal AnnualUniformAllowance = 6000;
            decimal MonthlyUniformAllowance = 0;

            MonthlyUniformAllowance = AnnualUniformAllowance / 12;

            if (value > MonthlyUniformAllowance)
            {
                ExcessValue = value - MonthlyUniformAllowance;
            }

            return ExcessValue;
        }

        public decimal ComputeMedicalExpenses(decimal value)
        {
            decimal ExcessValue = 0;
            decimal AnnualMedicalAllowance = 10000;
            decimal MonthlyMedicalAllowance = 0;

            MonthlyMedicalAllowance = AnnualMedicalAllowance / 12;

            if (value > MonthlyMedicalAllowance)
            {
                ExcessValue = value - MonthlyMedicalAllowance;
            }

            return ExcessValue;
        }

        public decimal ComputeLaundryAllowance(decimal value)
        {
            decimal ExcessValue = 0;

            if (value > 300)
            {
                ExcessValue = value - 300;
            }

            return ExcessValue;
        }

        public decimal ComputeAchievementAwards(decimal value)
        {
            decimal ExcessValue = 0;
            decimal AnnualAchievementAwards = 10000;
            decimal MonthlyAchievementAwards = 0;

            MonthlyAchievementAwards = AnnualAchievementAwards / 12;

            if (value > MonthlyAchievementAwards)
            {
                ExcessValue = value - MonthlyAchievementAwards;
            }

            return ExcessValue;
        }

        public decimal ComputeChristmasGift(decimal value)
        {
            decimal ExcessValue = 0;
            decimal AnnualGiftAllowance = 5000;
            decimal MonthlyGiftAllowance = 0;

            MonthlyGiftAllowance = AnnualGiftAllowance / 12;

            if (value > MonthlyGiftAllowance)
            {
                ExcessValue = value - MonthlyGiftAllowance;
            }

            return ExcessValue;
        }

        public decimal ComputeDailyMealAllowance(decimal value)
        {
            decimal ExcessValue = 0;

            return ExcessValue;
        }

        public decimal ComputeCollectiveBargainingAgreement(decimal value)
        {
            decimal ExcessValue = 0;
            decimal AnnualCBABenefits = 10000;
            decimal MonthlyCBABenefits = 0;

            MonthlyCBABenefits = AnnualCBABenefits / 12;

            if (value > MonthlyCBABenefits)
            {
                ExcessValue = value - MonthlyCBABenefits;
            }

            return ExcessValue;
        }

        public decimal ComputeDeMinimis(List<PayrollDeMinimisViewModel> dList, PaySlip pSlip, bool IsExcess)
        {
            decimal TotalCompensation = 0;
            decimal TotalExcess = 0;
            decimal ExcessValue = 0;

            foreach(var item in dList)
            {

                if (item.DeMinimis == DeMinimisType.AchievementAwards)
                {
                    ExcessValue = ComputeAchievementAwards(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.ChristmasGifts)
                {
                    ExcessValue = ComputeChristmasGift(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.CollectiveBargainingAgreement)
                {
                    ExcessValue = ComputeCollectiveBargainingAgreement(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.DailyMealAllowance)
                {
                    ExcessValue = ComputeDailyMealAllowance(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.LaundryAllowance)
                {
                    ExcessValue = ComputeLaundryAllowance(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.MedicalAllowancetoEmployeeDependents)
                {
                    ExcessValue = ComputeMedicalAllowance(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.MedicalExpenses)
                {
                    ExcessValue = ComputeMedicalExpenses(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.RiceSubsidy)
                {
                    ExcessValue = ComputeRiceSubsidy(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.UniformandClothing)
                {
                    ExcessValue = ComputeUniformAndClothing(item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.UnusedVacationLeave)
                {
                    ExcessValue = ComputeUnusedVacationLeave(pSlip.BasicSalary, pSlip.ExpectedWorkedHours, item.Value);
                    TotalCompensation += item.Value - ExcessValue;
                    TotalExcess += ExcessValue;
                }

                if (item.DeMinimis == DeMinimisType.VacationandSickLeaveCredits)
                {

                }

            }

            if (IsExcess)
                return TotalExcess;
            else
                return TotalCompensation;
        }



    }


}


public static class EnumExtensions
{
    public static string GetDisplayName(this Enum enumValue)
    {
        return enumValue.GetType()
                        .GetMember(enumValue.ToString())
                        .First()
                        .GetCustomAttribute<DisplayAttribute>()
                        .GetName();
    }
}