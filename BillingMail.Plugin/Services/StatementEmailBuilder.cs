using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BillingMail.Plugin.Models;
using BillingMail.Plugin.Options;

namespace BillingMail.Plugin.Services;

public class StatementEmailBuilder : IStatementEmailBuilder
{
    private readonly IBillingRepository _repository;
    private readonly BillingMailOptions _options;

    public StatementEmailBuilder(IBillingRepository repository, IOptions<BillingMailOptions> options)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
    }

    public async Task<(string Subject, string HtmlBody)> BuildStatementEmailAsync(ScheduledMailItem mailItem, CancellationToken cancellationToken = default)
    {
        var subject = $"Statement of account for {mailItem.ClientName} - {mailItem.StatementDate.AddDays(-1):dd MMM yyyy}";
        
        var sb = new StringBuilder();
        sb.Append(GetCssHeader());
        sb.Append("<body><div class='container'>");
        sb.Append("<div class='pt-3 row'><div class='col-11'>");
        sb.Append("<div class='pt-5 font font-weight-bold'>Dear Sir/Ma'am,</div>");
        sb.Append("<div class='pt-3'><p>We are reaching out to remind you of your outstanding invoice(s):</p></div>");
        sb.Append("</div></div>");

        // Statement Table Header
        sb.Append("<div class='py-3'><table class='table table-2 table-3'><thead><tr>");
        sb.Append("<th class='header' rowspan='2'>Invoice Date</th>");
        sb.Append("<th class='header' rowspan='2'>Invoice No.</th>");
        sb.Append("<th class='header' rowspan='2'>Bill No.</th>");
        sb.Append("<th class='header aged' rowspan='2'>Age (Days)</th>");
        sb.Append("<th class='header' colspan='3' style='border-bottom: 2px solid black !important;'>Outstanding Balance</th>");
        sb.Append("</tr><tr>");
        sb.Append("<th style='border-right:2px solid black !important'>Other Currency Code</th>");
        sb.Append("<th style='border-right: 2px solid black; border-top: 2px solid black;'>Amount in Other Currency</th>");
        sb.Append("<th class='header' style='min-width: 150px;'>Amount in Peso</th>");
        sb.Append("</tr></thead>");

        decimal grandTotal = 0;
        var practiceEntities = await _repository.GetPracticeEntitiesAsync(mailItem.ClientCode, mailItem.StatementDate, cancellationToken);

        foreach (var entity in practiceEntities)
        {
            decimal subTotal = 0;
            int counter = 1;

            sb.Append("<tbody><tr>");
            sb.Append($"<td colspan='7' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;color:white;'><div style='padding-left:5px;font-weight:bold;'>{entity.AccountName}</div></td>");
            sb.Append("</tr>");

            var lineItems = await _repository.GetStatementLineItemsAsync(mailItem.ClientCode, mailItem.StatementDate, entity.vcPracticeName, cancellationToken);

            foreach (var item in lineItems)
            {
                sb.Append("<tr>");
                sb.Append($"<td class='mydata'><div class='d-flex'><span style='font-size:12px;'>{counter}</span><div class='pl-2'>{item.TransDate:MM/dd/yyyy}</div></div></td>");
                sb.Append($"<td class='mydata'>{item.BillNo}</td>");
                sb.Append($"<td class='mydata'>{item.ReferenceNo}</td>");
                sb.Append($"<td class='mydata aged' style='text-align:center;'>{item.Age:#,##0}</td>");
                sb.Append($"<td class='mydata' style='text-align:center;'>{item.Currency}</td>");
                sb.Append($"<td class='mydata' style='text-align:right;'>{item.Dollar:#,##0.#0}</td>");
                sb.Append($"<td class='mydata' style='text-align:right;'>{item.Peso:#,##0.#0}</td>");
                sb.Append("</tr>");

                counter++;
                grandTotal += item.Peso;
                subTotal += item.Peso;
            }

            sb.Append("<tr>");
            sb.Append("<td colspan='1' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid gray !important;padding:0;text-align:right;font-weight:bold;color:white;'></td>");
            sb.Append($"<td colspan='4' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid gray !important;padding:0;text-align:right;font-weight:bold;color:white;'>{entity.AccountName}</td>");
            sb.Append("<td colspan='1' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid gray !important;padding:0;text-align:right;font-weight:bold;color:white;'></td>");
            sb.Append($"<td colspan='1' style='background-color:gray;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;text-align:right;font-weight:bold;color:white;'><div style='padding-right:10px;'>{subTotal:#,##0.#0}</div></td>");
            sb.Append("</tr>");
            sb.Append("<tr><td colspan='8' style='background-color:white;border-top: 2px solid black !important; border-right: 2px solid black !important;padding:0;'><div style='min-height:10px;'></div></td></tr></tbody>");
        }

        // Grand Total Footer
        sb.Append("<tfoot><tr>");
        sb.Append("<td colspan='3'></td>");
        sb.Append("<td colspan='2' class='font-weight-bolder' style='text-align: right;'><span style='margin-right:3px;'>Grand Total</span> :</td>");
        sb.Append("<td colspan='1'></td>");
        sb.Append($"<td colspan='1' class='font-weight-bolder' style='text-align: right;'><span style='margin-left:15px; border-bottom: 4px double black;text-align:right;'>{grandTotal:#,##0.#0}</span></td>");
        sb.Append("</tr></tfoot></table></div>");

        // Notes & Disclaimers
        sb.Append("<div class='d-flex py-2'><p><b>Notes:</b></p><ol>");
        sb.Append("<li>For accounts receivable incurred within the last seven days, please expect the invoice(s) within the week when this email is sent.</li>");
        sb.Append("<li>We email the invoice(s) separately. Please refer to them for the engagement(s) being billed.</li>");
        sb.Append("</ol></div>");
        sb.Append("<div class='d-flex py-3'><p>We would appreciate it if you could settle the above invoice(s). Kindly refer to our bank details below.</p></div>");

        // Bank Details Section
        sb.Append("<div><div class='w-100 font font-weight-bold p-2' style='color: white; background-color:#d04a02;'>Bank details</div></div>");
        sb.Append("<div class='pt-1'><table class='table p-1'><thead><tr>");
        sb.Append("<th style='text-align: center;'>Account Name</th>");
        sb.Append("<th style='text-align: center;'>Account Number</th>");
        sb.Append("<th style='text-align: center;'>Bank Name</th>");
        sb.Append("<th style='text-align: center;'>Branch</th>");
        sb.Append("<th style='border-right:none;text-align: center;'>Swift code</th>");
        sb.Append("</tr></thead><tbody>");

        foreach (var entity in practiceEntities)
        {
            var bankDetails = await _repository.GetBankDetailsAsync(entity.EntityCode, cancellationToken);
            foreach (var bank in bankDetails)
            {
                sb.Append("<tr>");
                sb.Append($"<td style='width:300px;'>{bank.AccountName}</td>");
                sb.Append($"<td style='width:300px;'>{bank.AccountNumber}</td>");
                sb.Append($"<td>{bank.BankName}</td>");
                sb.Append($"<td>{bank.Branch}</td>");
                sb.Append($"<td>{bank.SwiftCode}</td>");
                sb.Append("</tr>");
            }
        }

        sb.Append("</tbody></table></div>");

        // Closing & Footer
        sb.Append("<div class='pt-2'><p>If payment has already been made, kindly email the proof of payment for our reference.</p></div>");
        sb.Append("<div class='my-5'><table><tr><td colspan='8' style='text-align: center;'>");
        sb.Append($"<div><div class='ml-4 font-weight-bold'>Got any concerns or clarifications regarding this email?</div>");
        sb.Append($"<div class='ml-4'>You may reach out to <a href='mailto:{_options.SupportEmail}'>PH PwC Collections MBX (PH)</a>.</div></div>");
        sb.Append("</td></tr></table>");
        sb.Append("<div class='pt-4' style='text-align:left'><p><small>© 2022 Isla Lipana & Co. All rights reserved. Not for further distribution without the permission of PwC. PwC refers to Isla Lipana & Co., a Philippine member firm, and may sometimes refer to the PwC network. Each member firm is a separate legal entity. Please see www.pwc.com/structure for further details.</small></p></div>");
        sb.Append("</div></div></body></html>");

        return (subject, sb.ToString());
    }

    private static string GetCssHeader()
    {
        return @"<html><head><meta charset='utf-8' /><style type='text/css'>
            *, *::before, *::after { box-sizing: border-box; }
            thead { display: table-header-group !important; }
            tfoot { display: table-row-group !important; }
            tr { page-break-inside: avoid !important; }
            html { font-family: sans-serif; line-height: 1.15; }
            body { margin: 0; font-size: 1rem; font-weight: 400; line-height: 1.5; color: #212529; background-color: #fff; }
            p { margin-top: 0; margin-bottom: 1rem; }
            a { color: #007bff; text-decoration: none; }
            table { border-collapse: collapse; }
            .container { width: 100%; padding-right: 15px; padding-left: 15px; margin-right: auto; margin-left: auto; }
            .table { width: 100%; margin-bottom: 1rem; color: #212529; font-size: 14px; }
            .table th, .table td { padding: 0.75rem; vertical-align: top; border-top: 1px solid #dee2e6; }
            .table th { background-color: black; color: white; border-right: 4px solid white; }
            .table tbody td { background-color: lightgrey; border-top: 4px solid white !important; border-right: 4px solid white !important; }
            .table-2 { font-size: 14px; border: 2px solid black; }
            .table-2 thead th { vertical-align: middle; background-color: #D85604; color: white; text-align: center; }
            .table-2 thead th.header { border: 2px solid black; }
            .table-3 tbody td { background-color: white; border-top: 4px solid white !important; }
            .table-3 tbody td.mydata { border-top: 2px dotted !important; }
            .font-weight-bold { font-weight: 700 !important; }
            .font-weight-bolder { font-weight: bolder !important; }
            .d-flex { display: flex !important; }
            .py-2 { padding-top: 0.5rem !important; padding-bottom: 0.5rem !important; }
            .py-3 { padding-top: 1rem !important; padding-bottom: 1rem !important; }
            .pt-1 { padding-top: 0.25rem !important; }
            .pt-2 { padding-top: 0.5rem !important; }
            .pt-3 { padding-top: 1rem !important; }
            .pt-4 { padding-top: 1.5rem !important; }
            .pt-5 { padding-top: 3rem !important; }
            .p-1 { padding: 0.25rem !important; }
            .p-2 { padding: 0.5rem !important; }
            .w-100 { width: 100% !important; }
        </style></head>";
    }
}
