using BillColl_Main.AppDbContext;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public class ReportService : IReportService
    {
        private readonly IBillingMailStatus billingMailStatus;
        private readonly IConverter converter;
        private readonly IClients clients;
        private readonly IExceptions exceptions;
        private readonly IWebHostEnvironment webHostEnvironment;
        private readonly myDBContext context;
        private readonly IContacts contacts;
        private readonly IRemarks remarks;
        private readonly ILogs logs;

        public ReportService(IBillingMailStatus billingMailStatus, IConverter converter, IClients clients, IExceptions exceptions
            , IWebHostEnvironment webHostEnvironment, myDBContext context, IContacts contacts, IRemarks remarks, ILogs logs)
        {
            this.billingMailStatus = billingMailStatus;
            this.converter = converter;
            this.clients = clients;
            this.exceptions = exceptions;
            this.webHostEnvironment = webHostEnvironment;
            this.context = context;
            this.contacts = contacts;
            this.remarks = remarks;
            this.logs = logs;
        }
        public byte[] GenerateMail(string Clientcode, string AccountCode, string StatementDate, bool withImg, string url)
        {
            var csspath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\css", "report.css");
            var path = webHostEnvironment.WebRootPath + "\\img\\BillingAndCollection.png";
            var path2 = webHostEnvironment.WebRootPath + "\\img\\graph-pwc.png";
            var path3 = webHostEnvironment.WebRootPath + "\\img\\meeting-pwc.png";
            var path4 = webHostEnvironment.WebRootPath + "\\img\\message-pwc.png";

            string dStatementDate = Convert.ToDateTime(StatementDate).AddDays(1).ToString("dd MMM yyyy");

            var mySOA = billingMailStatus.GetStatementOfAccounts(Clientcode, dStatementDate);
            var phEntitylist = billingMailStatus.GetPHEntityLists(Clientcode, dStatementDate);
            var mybankDetails = billingMailStatus.GetBanks();


            decimal grandtotal = 0;
            decimal subtotal = 0;

            int ctr = 1;
            string body;

            body = $@"<html>
    <head>
    <meta charset='utf-8' />
    <title></title>
    <link href='{url}/css/report.css' rel='stylesheet' type='text/css' />
   </head>



            ";

            body += @"
            <body style='font-family:Arial; font-size:14px;'>
                <div class='container'>";
            //Img 1
            if (withImg == true)
            {
                body += $@"<div>
                       <img src='{path}' class='w-100' />
                    </div>";
            }

            //Img 2
            if (withImg == true)
            {
                body += @"
                    <div class='pt-3' style='display: -webkit-box;flex-wrap: wrap;'>
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

                body += $@"<div class='align-items-center justify-content-center h-100' style='display: -webkit-box; -webkit-box-pack: center;' >
                                 <img src='{path2}'  style='width:90%;height:300px;' />
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
                    </div>";
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
                    </thead> ";



            foreach (var myentitydata in phEntitylist)
            {
                ctr = 1;
                subtotal = 0;

                body += $@" <tbody>
                        <tr>
                            <td colspan='7' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;color:white;'><div style='padding-left:5px;font-weight:bold;'>{myentitydata.EntityNameD}</div></td>
                        </tr>
            ";

                foreach (var mydata in mySOA.Where(e=>e.EntityCode==myentitydata.EntityCode).OrderByDescending(e => e.Age))
                {
                    body += $@"
                        <tr>
                        <td class='mydata'>
                            <div style='display: -webkit-box; -webkit-box-pack: center;'>
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
        
                        <td class='mydata'  style='text-align:center;'>
                        {mydata.OtherCurrCode}
                        </td>
                        <td class='mydata' style='text-align:right;'>
                        {mydata.AmountInOtherCurr.ToString("#,##0.#0")}
                        </td>
                        <td class='mydata' style='text-align:right;border-right:2px solid black !important;'>
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
                                <td colspan='1' class='font-weight-bolder' style='text-align: right;border-right:2px solid black !important;'>
                           
                                    
                                    <span style='margin-left:15px; border-bottom: 4px double black;text-align:right;'>
                                        {grandtotal.ToString("#,##0.#0")}
                                    </span>
                               
                                </td>
                            </tr>
                        </tfoot>
                    </table>
                </div>


                <div class='d-flex py-2'>                     
                    <p><b>Notes:</b></p>
                    <ol>
                      <li>For accounts receivable incurred within the last seven days, please expect the invoice(s) within the week when this email is sent.</li>
                      <li>We email the invoice(s) separately. Please refer to them for the engagement(s) being billed.</li>
                    </ol> 

                </div>

                <div class='d-flex py-2'>
                        <p>We would appreciate it if you could settle the above invoice(s). Kindly refer to our bank details below.</p>
                </div>

                ";


                //   <div class='d-flex py-2'>
                //     <p>
                //     <b>Note:</b> Accounts receivable aging from 0 – 7 days is not yet available at this time.Please expect the copy(ies) of the new invoice(s)
                //    within the week.
                //     </p>
                //</div>


            //< span class='font-weight-bolder'>
            //                       GRAND TOTAL:
            //                   </span>

            //< div class='font-weight-bolder' style='margin-left:15px; border-bottom: 4px double black;text-align:right;'>
            //                         {grandtotal.ToString("#,##0.#0")}
            //                     </div>



            // Add Debtor/Client Info
            body += $@"
                    <div>
                        <div class='w-100 font font-weight-bold p-2' style='color: white; background-color:#d04a02'>Bank details</div>
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
                            </thead>
                            <tbody> ";

            foreach (var myentitydata in phEntitylist)
            {


                foreach (var mbd in mybankDetails.Where(e=>e.AccountCode== myentitydata.EntityCode))
                {
                    body += $@"  <tr>
                                            <td style='width:300px;'>{mbd.AccountName}</td>
                                            <td style='width:300px;'>
                                                {mbd.AccountNumber}
                                            </td>
                                            <td>{mbd.BankName}</td>
                                            <td>{mbd.Branch}</td>
                                            <td>{mbd.SwiftCode}</td>
                                        </tr> ";
                }

            }


            body += $@"    </tbody>
                                </table>
                            </div>            
            ";


            //mybankDetails


            body += @"
                <div class='pt-2'>
                             <p>If payment has already been made, kindly email the proof of payment for our reference. </p>
                        </div>";
            //img3
            if (withImg == true)
            {
                body += $@" <div>
                             <img src='{path3}' class='w-100' />
                        </div>";
            }

            body += $@" <div class='my-5' style='display: -webkit-box; -webkit-box-pack: center;'>";

            //img4
            if (withImg == true)
            {
                body += $@" <img src='{path4}' style='width:45px;height:40px;'/> ";
            }

            body += $@"     <div style='text-align: center;'>
                                <div class='ml-4 font-weight-bold'>
                                    Got any concerns or clarifications regarding this email?
                                </div>
                                <div class='ml-4'>
                                    You may reach out to <a href='mailto:ph_pwc_collections@pwc.com'>PH PwC Collections MBX (PH)</a>.
                                </div>
                            </div>
                        </div>


                        <div class='pt-4' style='text-align:left'>
                                <p><small>
                                © 2022 Isla Lipana & Co. All rights reserved. Not for further distribution without the permission of PwC. PwC refers to Isla Lipana & Co., a Philippine member firm, and may sometimes refer to the PwC network. Each member firm is a separate legal entity. Please see www.pwc.com/structure for further details.
                                </small>
                                </p>
                            </div>



                    </div>
                </body>
            </html>";

            GlobalSettings globalSettings = new GlobalSettings();
            globalSettings.ColorMode = ColorMode.Color;
            globalSettings.Orientation = Orientation.Portrait;
            globalSettings.PaperSize = PaperKind.Tabloid;
            globalSettings.Margins = new MarginSettings { Top = 10, Bottom = 10 };
            globalSettings.DocumentTitle = "DRAFT MAIL";
            //globalSettings.DPI = 300;
            ObjectSettings objectSettings = new ObjectSettings();
            objectSettings.PagesCount = true;
            objectSettings.HtmlContent = body;
            WebSettings webSettings = new WebSettings();
            webSettings.DefaultEncoding = "utf-8";
            webSettings.EnableIntelligentShrinking = false;
            //webSettings.UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\css", "report.css");


            objectSettings.WebSettings = webSettings;
            HtmlToPdfDocument htmlToPdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };
            return converter.Convert(htmlToPdfDocument);


        }

        public byte[] GenerateSOA(string Clientcode, string StatementDate, string url)
        {
            string dStatementDate = Convert.ToDateTime(StatementDate).AddDays(1).ToString("dd MMM yyyy");
            var mySOA = billingMailStatus.GetStatementOfAccounts(Clientcode, dStatementDate);
            var phEntitylist = billingMailStatus.GetPHEntityLists(Clientcode, dStatementDate);

            decimal grandtotal = 0;
            decimal subtotal = 0;
            int ctr = 1;
            string body;
            body = $@"<html>
                <head>
                <meta charset='utf-8' />
                <title></title>
                 <link href='{url}/css/report.css' rel='stylesheet' type='text/css' />
            </head>
            ";

            body += @"
            <body style='font-family:Arial; font-size:14px;'>
                <div class='container'>";

            // Add SOA Details
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

                foreach (var mydata in mySOA.Where(e => e.EntityCode == myentitydata.EntityCode).OrderByDescending(e => e.Age))
                {
                    body += $@"
                        <tr>
                        <td class='mydata'>
                            <div class='d-flex' style='display: -webkit-box !important; -webkit-box-pack:center;'>
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
                     
                        <td class='mydata'  style='text-align:center;'>
                        {mydata.OtherCurrCode}
                        </td>
                        <td class='mydata' style='text-align:right;'>
                        {mydata.AmountInOtherCurr.ToString("#,##0.#0")}
                        </td>
                        <td class='mydata' style='text-align:right;border-right:2px solid black !important;'>
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
                                <td colspan='1' class='font-weight-bolder' style='text-align: right;border-right:2px solid black !important;'>
                           
                                    
                                    <span style='margin-left:15px; border-bottom: 4px double black;text-align:right;'>
                                        {grandtotal.ToString("#,##0.#0")}
                                    </span>
                               
                                </td>
                            </tr>
                        </tfoot>
            </table>
        </div>

                ";

            body += @"</div></body></html>";

            GlobalSettings globalSettings = new GlobalSettings();
            globalSettings.ColorMode = ColorMode.Color;
            globalSettings.Orientation = Orientation.Portrait;
            globalSettings.PaperSize = PaperKind.Tabloid;
            globalSettings.Margins = new MarginSettings { Top = 10, Bottom = 10 };
            globalSettings.DocumentTitle = "DRAFT SOA";
            //globalSettings.DPI = 300;
            ObjectSettings objectSettings = new ObjectSettings();
            objectSettings.PagesCount = true;
            objectSettings.HtmlContent = body;
            WebSettings webSettings = new WebSettings();
            webSettings.DefaultEncoding = "utf-8";
            webSettings.EnableIntelligentShrinking = false;
            //webSettings.UserStyleSheet = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot\\css", "report.css");


            objectSettings.WebSettings = webSettings;
            HtmlToPdfDocument htmlToPdfDocument = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings },
            };
            return converter.Convert(htmlToPdfDocument);
        }

        public DataTable GetClientExceptions(string clientName = "", string dateSentPrev = "")
        {
            var exceptionlist = exceptions.GetExceptionsAll(clientName, dateSentPrev);

            var table = new DataTable();

            table.Columns.Add("ClientCode", typeof(string));
            table.Columns.Add("ClientName", typeof(string));
            table.Columns.Add("PreviousSentDate", typeof(string));
            table.Columns.Add("Reason", typeof(string));

            foreach (var a in exceptionlist)
            {
                var row = table.NewRow();
                row["ClientCode"] = a.ClientCode;
                row["ClientName"] = a.ClientName;
                row["PreviousSentDate"] = a.PreviousSentDate;
                row["Reason"] = a.Reason;

                table.Rows.Add(row);
            }
            table.TableName = "ExceptionList";

            return table;
        }

        public DataTable GetClients(string clientName = "")
        {
            var clientlist = clients.GetClientsAll(clientName);

            var table = new DataTable();

            table.Columns.Add("ClientCode", typeof(string));
            table.Columns.Add("ClientName", typeof(string));
            table.Columns.Add("ContactEmail", typeof(string));
            //table.Columns.Add("ContactName", typeof(string));
            table.Columns.Add("EngagementTeam", typeof(string));

            clientlist.ToList().ForEach(e =>
            {
                e.ContactEmail = string.Join(", ", contacts.GetContacts(e.ClientId).Select(e => e.Email).ToArray());
                e.PartnerInvolved = string.Join(", ", contacts.GetEngagementTeams(e.ClientId).Select(e => e.Name).ToArray());
            });

            foreach (var a in clientlist)
            {
                var row = table.NewRow();
                row["ClientCode"] = a.ClientId;
                row["ClientName"] = a.ClientName;
                row["ContactEmail"] = a.ContactEmail;
                //row["ContactName"] = a.ContactName;
                row["EngagementTeam"] = a.PartnerInvolved;
                table.Rows.Add(row);
            }

            table.TableName = "ClientList";

            return table;
        }

        public DataTable GetClientActivityLogs()
        {
            var logsList = logs.GetClientActivityLogs();

            var table = new DataTable();
            table.Columns.Add("Date", typeof(string));
            table.Columns.Add("Time", typeof(string));
            table.Columns.Add("User", typeof(string));
            table.Columns.Add("Client Name", typeof(string));
            table.Columns.Add("Invoice Number", typeof(string));
            table.Columns.Add("Action Taken", typeof(string));
            table.Columns.Add("Previous reason of exception", typeof(string));
            table.Columns.Add("Current reason of exception", typeof(string));
            table.Columns.Add("Reason of removal", typeof(string));

            foreach (var a in logsList)
            {
                var row = table.NewRow();
                row["Date"] = DateTime.TryParse(a.Date_Log, out var logDate) ? logDate.ToString("MMM dd, yyyy") : a.Date_Log;
                row["Time"] = DateTime.TryParse(a.Time_Log, out var logTime) ? logTime.ToString("HH:mm") : a.Time_Log;
                row["User"] = a.User_Name;
                row["Client Name"] = a.Client_Name;
                row["Invoice Number"] = a.Invoice_Number;
                row["Action Taken"] = a.Action;
                row["Previous reason of exception"] = a.Previous_Exception_Reason;
                row["Current reason of exception"] = a.Current_Exception_Reason;
                row["Reason of removal"] = a.Deletion_Reason;
                table.Rows.Add(row);
            }
            table.TableName = "ActivityLogs";

            return table;
        }

        public DataTable GetDelivered(string clientName = "", string dateSent = "")
        {
            var billinglist = billingMailStatus.GetDeliveredAll(clientName, dateSent);

            var table = new DataTable();

            table.Columns.Add("ClientCode", typeof(string));
            table.Columns.Add("ClientName", typeof(string));
            table.Columns.Add("ContactEmail", typeof(string));
            table.Columns.Add("ContactName", typeof(string));
            table.Columns.Add("StatementDate", typeof(string));
            table.Columns.Add("Remarks", typeof(string));

            billinglist.ToList().ForEach(e =>
            {
                e.ContactEmail = string.Join(", ", contacts.GetContacts(e.ClientId).Select(e => e.Email).ToArray());
                e.Remarks = remarks.GetRemarks(e.ClientId, e.StatementDate) != null ? remarks.GetRemarks(e.ClientId, e.StatementDate).Remarks : "";
            });

            foreach (var a in billinglist)
            {
                var row = table.NewRow();
                row["ClientCode"] = a.ClientId;
                row["ClientName"] = a.ClientName;
                row["ContactEmail"] = a.ContactEmail;
                row["ContactName"] = a.ContactName;
                row["StatementDate"] = a.StatementDate;
                row["Remarks"] = a.Remarks;

                table.Rows.Add(row);
            }

            table.TableName = "DeliveredList";

            return table;
        }

        public DataTable GetUndelivered(string clientName = "", string dateSent = "")
        {
            var billinglist = billingMailStatus.GetUndeliveredAll(clientName, dateSent);

            var table = new DataTable();

            table.Columns.Add("ClientCode", typeof(string));
            table.Columns.Add("ClientName", typeof(string));
            table.Columns.Add("ContactEmail", typeof(string));
            table.Columns.Add("ContactName", typeof(string));
            table.Columns.Add("StatementDate", typeof(string));
            table.Columns.Add("Remarks", typeof(string));

            billinglist.ToList().ForEach(e =>
            {
                e.ContactEmail = string.Join(", ", contacts.GetContacts(e.ClientId).Select(e => e.Email).ToArray());
                e.Remarks = remarks.GetRemarks(e.ClientId, e.StatementDate) != null ? remarks.GetRemarks(e.ClientId, e.StatementDate).Remarks : "";
            });

            foreach (var a in billinglist)
            {
                var row = table.NewRow();
                row["ClientCode"] = a.ClientId;
                row["ClientName"] = a.ClientName;
                row["ContactEmail"] = a.ContactEmail;
                row["ContactName"] = a.ContactName;
                row["StatementDate"] = a.StatementDate;
                row["Remarks"] = a.Remarks;

                table.Rows.Add(row);
            }

            table.TableName = "UndeliveredList";

            return table;
        }
    }
}
