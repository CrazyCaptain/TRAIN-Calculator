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


namespace TRAIN_Calculator.Controllers
{
    public class PayrollController : Controller
    {
        // GET: Payroll
        public ActionResult Index()
        {
            var model = new PayrollViewModel()
            {
                PaySlips = new PaySlip(),
                PaySlipCompensations = new PaySlipCompensation(),
                PaySlipDeductions = new PaySlipDeduction(),
                PaySlipDeMinimis = new PaySlipDeMinimis()
            };

            return View(model);
        }


        [HttpPost]
        public ActionResult Calculate(PayrollViewModel payroll)
        {
            var pSlip = payroll.PaySlips;
            var cList = payroll.PaySlipCompensations;
            var dList = payroll.PaySlipDeductions;
            var dmList = payroll.PaySlipDeMinimis;


            if (!ModelState.IsValid)
            {
                if (pSlip.BasicSalary != 0)
                    return View("Details", payroll);
                else
                    return View("Index", payroll);
            }
            


            
            var BasicPay = ComputeExpectedPay(pSlip.BasicSalary, pSlip.TotalHoursWorked, pSlip.ExpectedWorkedHours);
            
            var CompensationList = new PaySlipCompensation()
            {
                OvertimePay = ComputeOvertimePay(pSlip.OvertimeWorkedHours == null ? 0 : (int)pSlip.OvertimeWorkedHours, pSlip.OvertimeRatePerHour == null ? 0 : (decimal)pSlip.OvertimeRatePerHour),
                Allowance = cList == null ? 0 : cList.Allowance,
                ECOLA = cList == null ? 0 : cList.ECOLA,
                HolidayPay = cList == null ? 0 : cList.HolidayPay
            };

            var DeductionList = new PaySlipDeduction()
            {
                SSS = ComputeSSS(BasicPay),
                PHIC = ComputePHIC(BasicPay),
                PAGIBIG = ComputePAGIBIG(BasicPay),
                SSSLoan = dList == null ? 0 : dList.SSSLoan,
                PAGIBIGLoan = dList == null ? 0 : dList.PAGIBIGLoan
            };


            var DeminimisList = new PaySlipDeMinimis()
            {
                RiceAllowance = dmList == null ? 0 : dmList.RiceAllowance,
                LaundryAllowance = dmList == null ? 0 : dmList.LaundryAllowance

            };

            pSlip.TotalDeductions = ComputeTotalDeductions(DeductionList);
            pSlip.TotalCompensations = ComputeTotalCompensations(CompensationList, BasicPay);
            pSlip.TaxableIncome = ComputeTaxableIncome(BasicPay + CompensationList.OvertimePay, DeductionList.SSS, DeductionList.PHIC, DeductionList.PAGIBIG);
            pSlip.WithHoldingTax = ComputeTax(pSlip.TaxableIncome);
            pSlip.TakeHomePay = ComputeTakeHomePay(pSlip.TotalCompensations, pSlip.TotalDeductions, pSlip.WithHoldingTax);
            
            var model = new PayrollViewModel()
            {
                PaySlips = pSlip,
                PaySlipCompensations = CompensationList,
                PaySlipDeductions = DeductionList,
                PaySlipDeMinimis = DeminimisList
            };

            return View("Details", model); ;
        }


        public ActionResult Details()
        {
            return View();
        }

        public decimal ComputeSSS(decimal BasicPay)
        {
            decimal SSS = 0;

            string file = Server.MapPath("~/Controllers/SSSTable.json");
            string Json = System.IO.File.ReadAllText(file);
            JavaScriptSerializer ser = new JavaScriptSerializer();
            var SSSTable = ser.Deserialize<List<SSSTable>>(Json);

            foreach(var data in SSSTable)
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

        public decimal ComputeTaxableIncome(decimal BasicPay, decimal SSS, decimal PHIC, decimal PAGIBIG)
        {
            decimal TaxableIncome = 0;

            TaxableIncome = BasicPay - (SSS + PHIC + PAGIBIG);

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

        public decimal ComputeTotalDeductions(PaySlipDeduction model)
        {
            decimal Total = 0;

            Total = model.SSS + model.PAGIBIG + model.SSS + model.SSSLoan + model.PAGIBIGLoan;

            return Total;
        }

        public decimal ComputeTotalCompensations(PaySlipCompensation model, decimal BasicPay)
        {
            decimal Total = 0;

            Total = BasicPay + model.OvertimePay + model.Allowance + model.ECOLA + model.HolidayPay;

            return Total;
        }



    }




}
