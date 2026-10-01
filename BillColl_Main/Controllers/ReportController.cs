using BillColl_Main.Securities;
using BillColl_Main.Services;
using BillColl_Main.ViewModel;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService reportService;
        private readonly IDataProtectionProvider dataProtectionProvider;
        private readonly IClients clients;
        private readonly IBillingMailStatus billingMailStatus;
        private readonly DPPurposeStrings dPPurposeStrings;
        private readonly IDataProtector dataProtector;
        public ReportController(IReportService reportService, IDataProtectionProvider dataProtectionProvider
            , IClients clients, IBillingMailStatus billingMailStatus, DPPurposeStrings dPPurposeStrings)
        {
            this.reportService = reportService;
            this.dataProtectionProvider = dataProtectionProvider;
            this.clients = clients;
            this.billingMailStatus = billingMailStatus;
            this.dPPurposeStrings = dPPurposeStrings;
            dataProtector = dataProtectionProvider.CreateProtector(dPPurposeStrings.ClientIDKey);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult pSOA([FromQuery(Name = "reference")] string qstring)
        {
            string myqstring = dataProtector.Unprotect(qstring);
            string[] myarr = myqstring.Split("|");
            string url = $"{this.Request.Scheme}://{this.Request.Host}";
            var pdfFile = reportService.GenerateSOA(myarr[0], myarr[1], url);
            Response.ContentType = "Application/pdf";
            Response.Headers["Content-Disposition"] = $"inline; filename=Draft_SOA_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfFile, "application/pdf");
        }

        public IActionResult pMail([FromQuery(Name = "reference")] string qstring)
        {
            string myqstring = dataProtector.Unprotect(qstring);
            string[] myarr = myqstring.Split("|");
            string accountCode;
            accountCode = "PwC001";
            bool withImage = false;
            string url = $"{this.Request.Scheme}://{this.Request.Host}";
            var pdfFile = reportService.GenerateMail(myarr[0], accountCode, myarr[1], withImage, url);
            Response.ContentType = "Application/pdf";
            Response.Headers["Content-Disposition"] = $"inline; filename=Draft_Mail_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            return File(pdfFile, "application/pdf");
        }

        public IActionResult SOA()
        {
            SOAViewModel sOAViewModel = new SOAViewModel()
            {
                StatementOfAccounts = billingMailStatus.GetStatementOfAccounts("0002", "08 Feb 2022").ToList()
            };
            return View(sOAViewModel);
        }

        public IActionResult DownloadUndelivered([FromQuery(Name = "file")] string myreference)
        {
            string clientname, datesent;
            clientname = "";
            datesent = "";
            if (myreference != null)
            {
                string[] myarr = myreference.Split("|");

                clientname = myarr[0];
                datesent = myarr[1];

            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(reportService.GetUndelivered(clientname, datesent));
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "UndeliveredList.xlsx");
                }
            }
        }

        public IActionResult DownloadDelivered([FromQuery(Name = "file")] string myreference)
        {

            string clientname, datesent;
            clientname = "";
            datesent = "";
            if (myreference != null)
            {
                string[] myarr = myreference.Split("|");

                clientname = myarr[0];
                datesent = myarr[1];

            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(reportService.GetDelivered(clientname, datesent));
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DeliveredList.xlsx");
                }
            }
        }

        public IActionResult DownloadClientList([FromQuery(Name = "file")] string myreference)
        {
            string clientname;
            clientname = "";
            if (myreference != null)
            {
           
                clientname = myreference;
          

            }
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(reportService.GetClients(clientname));
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ClientList.xlsx");
                }
            }
        }

        public IActionResult DownloadExceptions([FromQuery(Name = "file")] string myreference)
        {
            string clientcode,datesentprev;
            clientcode = "";
            datesentprev = "";
            if (myreference != null)
            {
                string[] myarr = myreference.Split("|");

                clientcode = myarr[0];
                datesentprev = myarr[1];

            }

            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(reportService.GetClientExceptions(clientcode, datesentprev));
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExcemptionList.xlsx");
                }
            }
        }


        [Route("download_viewlogreport_clientengagement")]
        public IActionResult Download__Logs()
        {
            using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(reportService.GetClientActivityLogs());
                using (MemoryStream stream = new MemoryStream())
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Activity_Logs.xlsx");
                }
            }
        }

    }
}
