using BillColl_Main.Class;
using BillColl_Main.Helper;
using BillColl_Main.Models;
using BillColl_Main.Securities;
using BillColl_Main.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClients clients;
        private readonly IRemarks remarks;
        private readonly IContacts contacts;
        private readonly IBillingMailStatus billingMailStatus;
        private readonly IMailService mailService;
        private readonly IDataProtector dataProtector;
        private Logger log = LogManager.GetCurrentClassLogger();
        public HomeController(ILogger<HomeController> logger, IClients clients, IRemarks remarks, IContacts contacts
            , IBillingMailStatus billingMailStatus, IMailService mailService, IDataProtectionProvider dataProtectionProvider, DPPurposeStrings dPPurposeStrings)
        {
            _logger = logger;
            this.clients = clients;
            this.remarks = remarks;
            this.contacts = contacts;
            this.billingMailStatus = billingMailStatus;
            this.mailService = mailService;
            dataProtector = dataProtectionProvider.CreateProtector(dPPurposeStrings.ClientIDKey);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetUData(int pageNumber = 1, int pageSize = 10, string clientName = "", string dateSent = "", string dateSort = "")
        {
            var myUData = billingMailStatus.GetUndelivered_New(clientName, dateSent, dateSort, pageNumber * 10).ToList();

            myUData.ForEach(e =>
            {
                e.eClientId = dataProtector.Protect(e.ClientId + "|" + e.Statement_Date);
                var rem = remarks.GetRemarks(e.ClientId, Convert.ToDateTime(e.Statement_Date));
                e.Remarks = rem != null ? rem.Remarks : "";
            });

            var pagedData = Pagination.PagedResult(myUData, pageNumber, (myUData.Count > 0 ? myUData[0].TotalPages : 0));

            return Json(pagedData);
        }

        [HttpGet]
        public IActionResult GetDData(int pageNumber = 1, int pageSize = 10, string clientName = "", string dateSent = "", string dateSort = "")
        {
            var myDData = billingMailStatus.GetDelivered_New(clientName, dateSent, dateSort, pageNumber * 10).ToList();

            myDData.ForEach(e =>
            {
                e.eClientId = dataProtector.Protect(e.ClientId + "|" + e.Statement_Date);
            });

            var pagedData = Pagination.PagedResult(myDData, pageNumber, (myDData.Count > 0 ? myDData[0].TotalPages : 0));

            return Json(pagedData);
        }

        [HttpGet]
        public IActionResult GetPData(int pageNumber = 1, int pageSize = 10, string clientName = "", string dateSent = "", string dateSort = "")
        {
            var myPData = billingMailStatus.GetMailPercentageStatuses(pageNumber * 10).OrderByDescending(e => e.StatementDate).ToList();

            var pagedData = Pagination.PagedResult(myPData, pageNumber, (myPData.Count > 0 ? myPData[0].TotalPages : 0));

            return Json(pagedData);
        }

        [HttpPost]
        public async Task<IActionResult> SendMail(string qstring)
        {
            
            string myqstring = dataProtector.Unprotect(qstring);
            string[] myarr = myqstring.Split("|");
            string dStatementDate = Convert.ToDateTime(myarr[1]).AddDays(1).ToString("dd MMM yyyy");
            
            var mySOA = billingMailStatus.GetStatementOfAccounts(myarr[0], dStatementDate);
            var phEntitylist = billingMailStatus.GetPHEntityLists(myarr[0], dStatementDate);

            var mybankDetails = billingMailStatus.GetBanks();
            var clientinfo = clients.GetClient(myarr[0]);

            decimal grandtotal = 0;
            decimal subtotal = 0;
            int ctr = 1;
            string body;
            bool withImg = false;

            body = @"<html>
<head>
    <meta charset='utf-8' />
    <title></title>
    <style type='text/css'>
        *,
        *::before,
        *::after {
            box-sizing: border-box;
        }


thead {
    display: table-header-group !important
}

tfoot {
    display: table-row-group !important
}

tr {
    page-break-inside: avoid !important
}

        html {
            font-family: sans-serif;
            line-height: 1.15;
            -webkit-text-size-adjust: 100%;
            -webkit-tap-highlight-color: rgba(0, 0, 0, 0);
        }

        body {
            margin: 0;
            font-size: 1rem;
            font-weight: 400;
            line-height: 1.5;
            color: #212529;
            text-align: left;
            background-color: #fff;
        }   

        p {
            margin-top: 0;
            margin-bottom: 1rem;
        }


        a {
            color: #007bff;
            text-decoration: none;
            background-color: transparent;
        }

        img {
            vertical-align: middle;
            border-style: none;
        }

        table {
            border-collapse: collapse;
        }

        th {
            text-align: inherit;
        }

        label {
            display: inline-block;
            margin-bottom: 0.5rem;
        }

        .container {
            width: 100%;
            padding-right: 15px;
            padding-left: 15px;
            margin-right: auto;
            margin-left: auto;
        }

        @media (min-width: 576px) {
            .container {
                max-width: 540px;
            }
        }

        @media (min-width: 768px) {
            .container {
                max-width: 720px;
            }
        }

        @media (min-width: 992px) {
            .container {
                max-width: 960px;
            }
        }

        @media (min-width: 1200px) {
            .container {
                max-width: 1140px;
            }
        }

        .w-75 {
            width: 75% !important;
        }

        .w-100 {
            width: 100% !important;
        }


        .table {
            width: 100%;
            margin-bottom: 1rem;
            color: #212529;
            font-size: 14px;
        }

            .table th,
            .table td {
                padding: 0.75rem;
                vertical-align: top;
                border-top: 1px solid #dee2e6;
            }

            .table thead th {
                vertical-align: bottom;
                border-bottom: 2px solid #dee2e6;
            }

            .table tbody + tbody {
                border-top: 2px solid #dee2e6;
            }

            .table th {
                border-bottom: none !important;
                border-top: none !important;
                background-color: black;
                color: white;
                border-right: 4px solid white;
            }

            .table tbody td {
                background-color: lightgrey;
                border-top: 4px solid white !important;
                border-right: 4px solid white !important;
            }

        .table-2 {
            font-size: 14px;
            border: 2px solid black;
        }

            .table-2 thead th {
                vertical-align: middle;
                background-color: #D85604;
                color: white;
                
                width: 118px;
                text-align: center;
            }

                .table-2 thead th.header {
                    border: 2px solid black;
                }

                    .table-2 thead th.header.descript {
                        width: 280px;
                    }

                    .table-2 thead th.header.aged {
                        width: 50px;
                    }

        .table-3 {
            font-size: 14px;
            
        }

            .table-3 tbody tr {
                line-height: 40px;
            }

            .table-3 tbody td {
                width: 118px;
                background-color: white;
                border-top: 4px solid white !important;
                border-right: 4px solid white !important;
            }

                .table-3 tbody td.mydata {
                    border-top: 2px dotted !important;
                    border-right: none !important;
                }

                    .table-3 tbody td.mydata.descript {
                        width: 280px;
                    }

                    .table-3 tbody td.mydata.aged {
                        width: 50px;
                    }


        .font-weight-light {
            font-weight: 300 !important;
        }

        .font-weight-lighter {
            font-weight: lighter !important;
        }

        .font-weight-normal {
            font-weight: 400 !important;
        }

        .font-weight-bold {
            font-weight: 700 !important;
        }

        .font-weight-bolder {
            font-weight: bolder !important;
        }

        .d-flex {
            display: -ms-flexbox !important;
            display: flex !important;
        }

        .justify-content-end {
            -ms-flex-pack: end !important;
            justify-content: flex-end !important;
        }

        .justify-content-center {
            -ms-flex-pack: center !important;
            justify-content: center !important;
        }

        .justify-content-between {
            -ms-flex-pack: justify !important;
            justify-content: space-between !important;
        }

        .align-items-end {
            -ms-flex-align: end !important;
            align-items: flex-end !important;
        }

        .align-items-center {
            -ms-flex-align: center !important;
            align-items: center !important;
        }

        .row {
            display: -ms-flexbox;
            display: flex;
            -ms-flex-wrap: wrap;
            flex-wrap: wrap;
            margin-right: -15px;
            margin-left: -15px;
        }

        .col-1, .col-2, .col-3, .col-4, .col-5, .col-6, .col-7, .col-8, .col-9, .col-10, .col-11, .col-12, .col,
        .col-auto, .col-sm-1, .col-sm-2, .col-sm-3, .col-sm-4, .col-sm-5, .col-sm-6, .col-sm-7, .col-sm-8, .col-sm-9, .col-sm-10, .col-sm-11, .col-sm-12, .col-sm,
        .col-sm-auto, .col-md-1, .col-md-2, .col-md-3, .col-md-4, .col-md-5, .col-md-6, .col-md-7, .col-md-8, .col-md-9, .col-md-10, .col-md-11, .col-md-12, .col-md,
        .col-md-auto, .col-lg-1, .col-lg-2, .col-lg-3, .col-lg-4, .col-lg-5, .col-lg-6, .col-lg-7, .col-lg-8, .col-lg-9, .col-lg-10, .col-lg-11, .col-lg-12, .col-lg,
        .col-lg-auto, .col-xl-1, .col-xl-2, .col-xl-3, .col-xl-4, .col-xl-5, .col-xl-6, .col-xl-7, .col-xl-8, .col-xl-9, .col-xl-10, .col-xl-11, .col-xl-12, .col-xl,
        .col-xl-auto {
            position: relative;
            width: 100%;
            padding-right: 15px;
            padding-left: 15px;
        }

        .col-1 {
            -ms-flex: 0 0 8.333333%;
            flex: 0 0 8.333333%;
            max-width: 8.333333%;
        }

        .col-2 {
            -ms-flex: 0 0 16.666667%;
            flex: 0 0 16.666667%;
            max-width: 16.666667%;
        }

        .col-3 {
            -ms-flex: 0 0 25%;
            flex: 0 0 25%;
            max-width: 25%;
        }

        .col-4 {
            -ms-flex: 0 0 33.333333%;
            flex: 0 0 33.333333%;
            max-width: 33.333333%;
        }

        .col-5 {
            -ms-flex: 0 0 41.666667%;
            flex: 0 0 41.666667%;
            max-width: 41.666667%;
        }

        .col-6 {
            -ms-flex: 0 0 50%;
            flex: 0 0 50%;
            max-width: 50%;
        }

        .col-7 {
            -ms-flex: 0 0 58.333333%;
            flex: 0 0 58.333333%;
            max-width: 58.333333%;
        }

        .col-8 {
            -ms-flex: 0 0 66.666667%;
            flex: 0 0 66.666667%;
            max-width: 66.666667%;
        }

        .col-9 {
            -ms-flex: 0 0 75%;
            flex: 0 0 75%;
            max-width: 75%;
        }

        .col-10 {
            -ms-flex: 0 0 83.333333%;
            flex: 0 0 83.333333%;
            max-width: 83.333333%;
        }

        .col-11 {
            -ms-flex: 0 0 91.666667%;
            flex: 0 0 91.666667%;
            max-width: 91.666667%;
        }

        .col-12 {
            -ms-flex: 0 0 100%;
            flex: 0 0 100%;
            max-width: 100%;
        }

        .m-0 {
            margin: 0 !important;
        }

        .mt-0,
        .my-0 {
            margin-top: 0 !important;
        }

        .mr-0,
        .mx-0 {
            margin-right: 0 !important;
        }

        .mb-0,
        .my-0 {
            margin-bottom: 0 !important;
        }

        .ml-0,
        .mx-0 {
            margin-left: 0 !important;
        }

        .m-1 {
            margin: 0.25rem !important;
        }

        .mt-1,
        .my-1 {
            margin-top: 0.25rem !important;
        }

        .mr-1,
        .mx-1 {
            margin-right: 0.25rem !important;
        }

        .mb-1,
        .my-1 {
            margin-bottom: 0.25rem !important;
        }

        .ml-1,
        .mx-1 {
            margin-left: 0.25rem !important;
        }

        .m-2 {
            margin: 0.5rem !important;
        }

        .mt-2,
        .my-2 {
            margin-top: 0.5rem !important;
        }

        .mr-2,
        .mx-2 {
            margin-right: 0.5rem !important;
        }

        .mb-2,
        .my-2 {
            margin-bottom: 0.5rem !important;
        }

        .ml-2,
        .mx-2 {
            margin-left: 0.5rem !important;
        }

        .m-3 {
            margin: 1rem !important;
        }

        .mt-3,
        .my-3 {
            margin-top: 1rem !important;
        }

        .mr-3,
        .mx-3 {
            margin-right: 1rem !important;
        }

        .mb-3,
        .my-3 {
            margin-bottom: 1rem !important;
        }

        .ml-3,
        .mx-3 {
            margin-left: 1rem !important;
        }

        .m-4 {
            margin: 1.5rem !important;
        }

        .mt-4,
        .my-4 {
            margin-top: 1.5rem !important;
        }

        .mr-4,
        .mx-4 {
            margin-right: 1.5rem !important;
        }

        .mb-4,
        .my-4 {
            margin-bottom: 1.5rem !important;
        }

        .ml-4,
        .mx-4 {
            margin-left: 1.5rem !important;
        }

        .m-5 {
            margin: 3rem !important;
        }

        .mt-5,
        .my-5 {
            margin-top: 3rem !important;
        }

        .mr-5,
        .mx-5 {
            margin-right: 3rem !important;
        }

        .mb-5,
        .my-5 {
            margin-bottom: 3rem !important;
        }

        .ml-5,
        .mx-5 {
            margin-left: 3rem !important;
        }

        .p-0 {
            padding: 0 !important;
        }

        .pt-0,
        .py-0 {
            padding-top: 0 !important;
        }

        .pr-0,
        .px-0 {
            padding-right: 0 !important;
        }

        .pb-0,
        .py-0 {
            padding-bottom: 0 !important;
        }

        .pl-0,
        .px-0 {
            padding-left: 0 !important;
        }

        .p-1 {
            padding: 0.25rem !important;
        }

        .pt-1,
        .py-1 {
            padding-top: 0.25rem !important;
        }

        .pr-1,
        .px-1 {
            padding-right: 0.25rem !important;
        }

        .pb-1,
        .py-1 {
            padding-bottom: 0.25rem !important;
        }

        .pl-1,
        .px-1 {
            padding-left: 0.25rem !important;
        }

        .p-2 {
            padding: 0.5rem !important;
        }

        .pt-2,
        .py-2 {
            padding-top: 0.5rem !important;
        }

        .pr-2,
        .px-2 {
            padding-right: 0.5rem !important;
        }

        .pb-2,
        .py-2 {
            padding-bottom: 0.5rem !important;
        }

        .pl-2,
        .px-2 {
            padding-left: 0.5rem !important;
        }

        .p-3 {
            padding: 1rem !important;
        }

        .pt-3,
        .py-3 {
            padding-top: 1rem !important;
        }

        .pr-3,
        .px-3 {
            padding-right: 1rem !important;
        }

        .pb-3,
        .py-3 {
            padding-bottom: 1rem !important;
        }

        .pl-3,
        .px-3 {
            padding-left: 1rem !important;
        }

        .p-4 {
            padding: 1.5rem !important;
        }

        .pt-4,
        .py-4 {
            padding-top: 1.5rem !important;
        }

        .pr-4,
        .px-4 {
            padding-right: 1.5rem !important;
        }

        .pb-4,
        .py-4 {
            padding-bottom: 1.5rem !important;
        }

        .pl-4,
        .px-4 {
            padding-left: 1.5rem !important;
        }

        .p-5 {
            padding: 3rem !important;
        }

        .pt-5,
        .py-5 {
            padding-top: 3rem !important;
        }

        .pr-5,
        .px-5 {
            padding-right: 3rem !important;
        }

        .pb-5,
        .py-5 {
            padding-bottom: 3rem !important;
        }

        .pl-5,
        .px-5 {
            padding-left: 3rem !important;
        }
    </style>
</head>



            ";

            body += @"
            <body style='font-family:Arial; font-size:14px;'>
                <div class='container'>";
            //Img 1
            if (withImg == true)
            {
                body += @"<div>
                        <img class='w-100' id='1' src='cid:BillingAndCollection.png' />
                    </div>";
            }

            //Img 2
            if (withImg == true)
            {
                body += @"
                    <div class='pt-3 row'>
                        <div class='col-6'>
                            <div class='pt-5 font font-weight-bold'>
                                Dear Sir/Ma'am,
                            </div>
                            <div class='pt-5'>
                                <p>Good day! We hope you are keeping safe and well.</p>
                                <p>
                                    We are reaching out in relation to your unpaid invoices. As we were clearing our billing records, we noted that the same is yet to be settled.
                                    In this regard, we would appreciate it if you could expedite the settlement of this/these invoices.
                                </p>
                            </div>
                        </div>
                        <div class='col-6'>";

                body += @"<div class='d-flex align-items-center justify-content-center h-100'>
                           <img class='w-75' id='2' src='cid:graph-pwc.png' style='height:300px;' />
                      </div>";
            }
            else
            {
                body += @"
                    <div class='pt-3 row'>
                        <div class='col-11'>
                            <div class='pt-3 font font-weight-bold'>
                                Dear Sir/Ma'am,
                            </div>
                            <div class='pt-3'>
                                <p>We are reaching out to remind you of your outstanding invoice(s):</p>                   
                            </div>
                        </div>
                    </div>
                       ";
            }

            body += @"
            <div class='py-3'>

                <table class='table table-2 table-3'>

                     <thead>
                        <tr>
                            <th class='header' rowspan='2'>
                                Invoice Date
                            </th>
                            <th class='header' rowspan='2'>
                                Invoice No.
                            </th>
                            <th class='header' rowspan='2'>
                                Bill No.
                            </th>
                            <th class='header aged ' rowspan='2'>
                                Age
                                (Days)
                            </th>
                          
                            <th class='header' colspan='3' style='border-bottom: 2px solid black !important;'>
                                Outstanding Balance
                            </th>
                        </tr>
                        <tr>
                            <th style='border-right:2px solid black !important'>Other Currency Code</th>
                            <th style='border-right: 2px solid black; border-top: 2px solid black;'>Amount in Other Currency</th>
                            <th class='header' style='min-width: 150px;'>Amount in Peso</th>
                        </tr>
                    </thead>";



            foreach (var myentitydata in phEntitylist)
            {
                ctr = 1;
                subtotal = 0;


                body += $@" <tbody>
                        <tr>
                            <td colspan='7' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;color:white;'><div style='padding-left:5px;font-weight:bold;'>{myentitydata.EntityNameD}</div></td>
                        </tr>
                       ";

                foreach (var mydata in mySOA.Where(e => e.EntityCode == myentitydata.EntityCode).OrderByDescending(e=>e.Age))
                {
                    body += $@"
                        <tr>
                        <td class='mydata'>
                            <div class='d-flex'>
                                <span style='font-size:12px;'>{ctr}</span>
                                <div class='pl-2'>{mydata.InvoiceDate}</div>
                            </div>
                        </td>
                        <td class='mydata'>
                            {mydata.InvoiceNo}
                        </td>
                        <td class='mydata'>
                            {mydata.ReferenceNo}
                        </td>
                        <td class='mydata aged' style='text-align:center;'>
                            {mydata.Age.ToString("#,##0")}
                        </td>
                  
                        <td class='mydata' style='text-align:center;'>
                        {mydata.OtherCurrCode}
                        </td>
                        <td class='mydata' style='text-align:right;'>
                        {mydata.AmountInOtherCurr.ToString("#,##0.#0")}
                        </td>
                        <td class='mydata' style='text-align:right;'>
                        {mydata.AmountInPeso.ToString("#,##0.#0")}
                        </td>
                    </tr>
                ";

                    ctr++;
                    subtotal += mydata.AmountInPeso;
                    grandtotal += mydata.AmountInPeso;
                }
          
                body += $@"
                         <tr>
                            <td colspan='1' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid gray !important;padding:0;text-align:right;font-weight:bold;color:white;'>
                            </td>
                            <td colspan='4' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid gray !important;padding:0;text-align:right;font-weight:bold;color:white;'>                            
                           {myentitydata.EntityNameD}
                            </td>
                            <td colspan='1' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid gray !important;padding:0;text-align:right;font-weight:bold;color:white;'></td>                            
                            <td colspan='1' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;text-align:right;font-weight:bold;color:white;'>
                            <div style='padding-right:10px;'>{subtotal.ToString("#,##0.#0")}</div>
                            </td>
                        </tr>
                        <tr>
                            <td colspan='8' style='background-color:white;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;'><div style='min-height:10px;'></div></td>
                        </tr>                        
                </tbody> ";


            }
               body += $@"                            
                         <tfoot>
                            <tr>
                                <td colspan='3'></td>
                                <td colspan='2' class='font-weight-bolder' style='text-align: right;'>
                                <span style='margin-right:3px;'>Grand Total</span> :
                                </td>
                                <td colspan='1'></td>
                                <td colspan='1' class='font-weight-bolder' style='text-align: right;'>
                           
                                    
                                    <span style='margin-left:15px; border-bottom: 4px double black;text-align:right;'>
                                        {grandtotal.ToString("#,##0.#0")}
                                    </span>
                               
                                </td>
                            </tr>
                        </tfoot>


            </table>
        </div>

       
                <div class='d-flex py-2'>                     
                    <p><b>Note:</b></p>
                    <ol>
                      <li>For accounts receivable incurred within the last seven days, please expect the invoice(s) within the week when this email is sent.</li>
                      <li>We email the invoice(s) separately. Please refer to them for the engagement(s) being billed.</li>
                    </ol> 

                </div>


    <div class='d-flex py-3'>
            <p>We would appreciate it if you could settle the above invoice(s). Kindly refer to our bank details below.</p>
    </div>

                ";


            // Add Debtor/Client Info
            body += $@"
                    <div>
                        <div class='w-100 font font-weight-bold p-2' style='color: white; background-color:#d04a02;'>Bank details</div>
                    </div>
                 <div class='pt-1'>
                        <table class='table p-1'>
                            <thead>
                                <tr>
                                    <th style='text-align: center;'>
                                        Account Name
                                    </th>
                                    <th style='text-align: center;'>
                                        Account Number
                                    </th>
                                    <th style='text-align: center;'>
                                        Bank Name
                                    </th>
                                    <th style='text-align: center;'>
                                        Branch
                                    </th>
                                    <th style='border-right:none;text-align: center;'>
                                        Swift code
                                    </th>
                                </tr>
                            </thead>";

            foreach (var myentitydata in phEntitylist)
            {

                body += $@" <tbody>";
                foreach (var mbd in mybankDetails.Where(e => e.AccountCode == myentitydata.EntityCode))
                {
                    body += $@" <tr>
                                            <td style='width:300px;'>{mbd.AccountName}</td>
                                            <td style='width:300px;'>
                                                {mbd.AccountNumber}
                                            </td>
                                            <td>{mbd.BankName}</td>
                                            <td>{mbd.Branch}</td>
                                            <td>{mbd.SwiftCode}</td>
                                   </tr>";
                }

            }
            
            body += $@" </tbody>
                        </table>
                      </div> ";

            body += @"
                <div class='pt-2'>
                            <p>If payment has already been made, kindly email the proof of payment for our reference. </p>
                        </div>";
            //img3
            if (withImg == true)
            {
                body += $@" <div>
                            <img class='w-100' id='3' src='cid:meeting-pwc.png' />
                        </div>";
            }

            body += $@" <div class='my-5> <table> 

                            <tr>
                            <td colspan='8' style='text-align: center;'>
                        ";

            //img4
            if (withImg == true)
            {
                body += $@" <img id='4' src='cid:message-pwc.png' style='width:45px;height:40px;' />";
            }

            body += $@"     
                           
                            <div>
                                <div class='ml-4 font-weight-bold'>
                                    Got any concerns or clarifications regarding this email?
                                </div>
                                <div class='ml-4'>
                                    You may reach out to <a href='mailto:ph_pwc_collections@pwc.com'>PH PwC Collections MBX (PH)</a>.
                                </div>
                            </div>
                            </td>
                            </tr>
                            </table>

                            <div class='pt-4' style='text-align:left'>
                                <p><small>
                                © 2022 Isla Lipana & Co. All rights reserved. Not for further distribution without the permission of PwC. PwC refers to Isla Lipana & Co., a Philippine member firm, and may sometimes refer to the PwC network. Each member firm is a separate legal entity. Please see www.pwc.com/structure for further details.
                                </small>
                                </p>
                            </div>

                        </div>
                    </div>
                </body>
            </html>";


            string contactinfo = contacts.GetContactFromScheduleMail(myarr[0], Convert.ToDateTime(dStatementDate));

            string[] checkcontact = contactinfo.Split(",");

            string econtactinfo = contacts.GetEngagementFromScheduleMail(myarr[0], Convert.ToDateTime(dStatementDate));

            string[] echeckcontact = econtactinfo.Split(",");

            MailRequest mailRequest = new MailRequest()
            {
                Subject = $"Statement of account for {clientinfo.ClientName} - {dStatementDate}",

                ToEmail = (checkcontact.Count() > 1) ? "" : contactinfo,
                ToEmails = (checkcontact.Count() > 1) ? contactinfo : "",
               
                ToCCEmails = (echeckcontact.Count() > 1) ? "" : econtactinfo,
                ToCCEmail = (echeckcontact.Count() > 1) ? econtactinfo : "",
                Body = body,
                WithImage = withImg
            };
            await mailService.SendEmailAsync(mailRequest);

            //Update senddate value
            billingMailStatus.UpdateSendDate(myarr[0], dStatementDate);

            return RedirectToAction("Index");

         
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            var feature = this.HttpContext.Features.Get<IExceptionHandlerFeature>();
            log.Error(HttpContext.TraceIdentifier.ToString());
            log.Error(feature.Error.ToString());
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    
        [HttpPost]
        public IActionResult SaveRemarks(string Remarks, string Ref )
        {
            string myref = dataProtector.Unprotect(Ref);
            string[] myarr = myref.Split("|");
            var checkremarks = remarks.GetRemarks(myarr[0], Convert.ToDateTime(myarr[1]));
            if(checkremarks != null)
            {
                checkremarks.Remarks = Remarks;
               
                var updateRemarks = remarks.UpdateRemarks(checkremarks);
            }
            else
            {
                ClientRemarks addRemark = new ClientRemarks() { 
                    ClientCode=myarr[0],
                    Remarks=Remarks,
                    StatementDate= Convert.ToDateTime(myarr[1])
                };
                var newRemarks = remarks.AddRemarks(addRemark);

            }

            return Json("Update complete");
        }

        public async Task<IActionResult> LogOut() {

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect("https://login.pwc.com:443/openam/UI/Logout");
        }

    }
}
