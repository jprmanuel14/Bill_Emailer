using BillColl_Main.Filters;
using BillColl_Main.Helper;
using BillColl_Main.Models;
using BillColl_Main.Securities;
using BillColl_Main.Services;
using BillColl_Main.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using ClosedXML.Excel;

namespace BillColl_Main.Controllers
{
    [Authorize]
    [AdminOnly]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public class SettingsController : Controller
    {
        private readonly IExceptions exceptions;
        private readonly IClients clients;
        private readonly IBillingMailStatus billingMailStatus;
        private readonly IUserRole userRole;
        private readonly IEngagementSecretary engagementSecretary;
        private readonly IEmployeeDirectory employeeDirectory;
        private readonly ISettingsRepository settings;
        private readonly IDataProtector dataProtector;

        public SettingsController(IExceptions exceptions, IClients clients, IBillingMailStatus billingMailStatus
            , IUserRole userRole, IEngagementSecretary engagementSecretary, IEmployeeDirectory employeeDirectory, ISettingsRepository settings
            , IDataProtectionProvider dataProtectionProvider, DPPurposeStrings dPPurposeStrings)
        {
            this.exceptions = exceptions;
            this.clients = clients;
            this.billingMailStatus = billingMailStatus;
            this.userRole = userRole;
            this.engagementSecretary = engagementSecretary;
            this.employeeDirectory = employeeDirectory;
            this.settings = settings;
            dataProtector = dataProtectionProvider.CreateProtector(dPPurposeStrings.ClientIDKey);
        }

        [Authorize]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public IActionResult Index()
        {
            // The admin check lives on the controller via [AdminOnly] so that every action —
            // including the write endpoints that grant roles — is gated, not just this page.
            return View();
        }

        [Route("get_name_from_username")]

        public IActionResult Get_Name_From_Username()
        {
            Nre_Uer_New user_new = new();

            user_new.Employee_Name = employeeDirectory.GetNameByEmployeeCode(User.Identity.Name);

            return Json(user_new);

        }

        public IActionResult GetData(int pageNumber, int pageSize = 10, string clientName = "", string dateSentPrev = "", string dateSortPrev = "")
        {

            var exceptionlist = exceptions.GetExceptions(clientName, dateSentPrev, dateSortPrev);

            exceptionlist.ToList().ForEach(e => { e.eId = dataProtector.Protect(e.Id.ToString()); });

            var pagedData = Pagination.PagedResult(exceptionlist.ToList(), pageNumber, pageSize);

            return Json(pagedData);

        }

        public IActionResult GetClients(string ClientName)
        {
            var myclient = clients.GetClients(ClientName);
            var myException = exceptions.GetExceptions();

            var excludedClientCode = new HashSet<string>(myException.Select(p => p.ClientCode));
            var joingroup = myclient.Where(p => !excludedClientCode.Contains(p.ClientId));


            return Json(joingroup);
        }

        //public IActionResult CheckIfExisting([FromQuery(Name = "Reference")] string reference)
        //{
        //    var myexception = exceptions.GetException(reference);
        //    if (myexception != null)
        //    {
        //        return Json("Yes");
        //    }

        //    return Json("No");
        //}

        [HttpDelete]
        public IActionResult RemoveException([FromQuery(Name = "Reference")] string reference)
        {
            int myid = Convert.ToInt32(dataProtector.Unprotect(reference));

            var removeException = exceptions.RemoveExceptions(myid);

            billingMailStatus.UpdateTblScheduleMailStatus(removeException.ClientCode, "");

            return Json("Removal of exception complete");
        }

        [HttpPost]
        public IActionResult Submit(ExceptionViewModel exceptionViewModel)
        {
            if (ModelState.IsValid)
            {
                Exceptions newException = new Exceptions()
                {
                    ClientCode = exceptionViewModel.ClientCode.Trim(),
                    ClientName = exceptionViewModel.ClientName.Trim(),
                    Reason = exceptionViewModel.Reasons
                };

                var addException = exceptions.AddExceptions(newException);

                billingMailStatus.UpdateTblScheduleMailStatus(exceptionViewModel.ClientCode.Trim(), "HOLD");

                return RedirectToAction("Index");
            }
            return View("Index");
        }






        //=================== Mga personalized na method by JPRM
        // Role
        [Route("add_the_role")]
        public IActionResult Add_Role([FromQuery] Nre_Uer_New admin_variable)
        {
            userRole.AddUserRole(admin_variable, admin_variable.Action_Taker, admin_variable.Action_Taken);

            return Json(admin_variable);

        }

        [Route("delete_role")]
        public IActionResult Delete_Role([FromQuery] Nre_Uer_New admin_variable)
        {
            userRole.DeleteUserRole(admin_variable.Id, admin_variable.Action_Taker, admin_variable.Action_Taken);

            return Json(new List<Nre_Uer_New>());

        }

        [Route("view_role")]
        public IActionResult View_Role([FromQuery] string i_variable)
        {
            List<Nre_Uer_New> examples = new List<Nre_Uer_New>();

            var role = userRole.GetUserRole(i_variable);

            if (role != null)
            {
                examples.Add(role);
            }

            return Json(examples);

        }

        [Route("update_role")]
        public IActionResult Update_Role([FromQuery] Nre_Uer_New admin_variable)
        {
            userRole.UpdateUserRole(admin_variable, admin_variable.Action_Taker, admin_variable.Action_Taken);

            return Json(admin_variable);

        }

        [Route("load_role")] // For displaying the user roles from the table
        public IActionResult Load_Deletion_Reason()
        {
            return Json(userRole.GetUserRoles());

        }

        [Route("admin_logs")] // For displaying the admin logs
        public IActionResult Admin_Logs()
        {
            return Json(settings.GetAdminLogs());
        }


        // Secretary
        [Route("add_engagement_secretary")]
        public IActionResult Add_Engagement_Secretary([FromQuery] Engagement_Secretary admin_variable)
        {
            engagementSecretary.AddEngagementSecretary(admin_variable, admin_variable.Action_Taker, admin_variable.Action_Taken);

            return Json(admin_variable);

        }

        [Route("delete_engagement_secretary")]
        public IActionResult Delete_Engagement_Secretary([FromQuery] Engagement_Secretary admin_variable)
        {
            engagementSecretary.DeleteEngagementSecretary(admin_variable.Id, admin_variable.Action_Taker, admin_variable.Action_Taken);

            return Json(new List<Engagement_Secretary>());

        }

        [Route("view_engagement_secretary")]
        public IActionResult View_Engagement_Secretary([FromQuery] string i_variable)
        {
            List<Engagement_Secretary> examples = new List<Engagement_Secretary>();

            var secretary = settings.GetEngagementSecretary(i_variable);

            if (secretary != null)
            {
                examples.Add(secretary);
            }

            return Json(examples);

        }

        [Route("validate_engagement_secretary")]
        public IActionResult Validate_Engagement_Secretary([FromQuery] string secretary_variable, string partner_variable, string group_variable)
        {
            List<Engagement_Secretary> examples = new List<Engagement_Secretary>();

            Engagement_Secretary ex = new();

            ex.Engagement_Secretary_Count = engagementSecretary.CountBySecretaryPartnerGroup(secretary_variable, partner_variable, group_variable);

            examples.Add(ex);

            return Json(examples);

        }

        [Route("validate_adding_engagement_secretary")]
        public IActionResult Validate_Adding_Engagement_Secretary([FromQuery] Engagement_Secretary admin_variable)
        {
            List<Engagement_Secretary> examples = new List<Engagement_Secretary>();

            Engagement_Secretary ex = new();

            ex.Engagement_Secretary_Count = engagementSecretary.CountByPartnerGroup(admin_variable.Partner, admin_variable.Group);

            examples.Add(ex);

            return Json(examples);

        }

        [Route("update_engagement_secretary")]
        public IActionResult Update_Engagement_Secretary([FromQuery] Engagement_Secretary admin_variable)
        {
            engagementSecretary.UpdateEngagementSecretary(admin_variable, admin_variable.Action_Taker, admin_variable.Action_Taken);

            return Json(admin_variable);

        }

        [Route("load_engagement_secretary")] // For displaying the load engagement secretary
        public IActionResult Load_Engagement_Secretary()
        {
            return Json(settings.GetEngagementSecretaries());
        }

        [Route("engagement_secretary_logs")]
        public IActionResult Engagement_Secretary_Logs()
        {
            return Json(settings.GetSecretaryLogs());
        }

        [Route("stafflist_executive")] // For searching of executive secretaries
        public IActionResult Get_Staffs_Executive([FromQuery] string i_variable = "")
        {
            return Json(employeeDirectory.GetStaff(StaffSelection.Executive));
        }

        [Route("stafflist_partner")] // For searching of partners
        public IActionResult Get_Staffs_Partner([FromQuery] string i_variable = "")
        {
            return Json(employeeDirectory.GetStaff(StaffSelection.Partner));
        }

        [Route("stafflist")] // For searching of employee list under engagement
        public IActionResult Get_Staffs()
        {
            return Json(StaffUnderEngagement());
        }

        [Route("stafflist_for_engagement_users")] // Client.js asks for this exact route
        public IActionResult Get_Staffs_For_Engagement_Users()
        {
            return Json(StaffUnderEngagement());
        }

        private List<Example> StaffUnderEngagement()
        {
            var staff = employeeDirectory.GetStaff(StaffSelection.All).ToList();
            var alreadyRuled = settings.GetUserRoleEmployeeCodes();

            return staff
                .Where(s => !alreadyRuled.Contains(s.Id ?? ""))
                .OrderBy(s => s.Name)
                .ToList();
        }

        [Route("view_reports_new")] // For displaying the view_reports_new
        public IActionResult view_reports([FromQuery] string i_variable, string ex_statement_date_variable, string ex_LOS_variable)
        {
            return Json(settings.GetViewReports(i_variable, ex_statement_date_variable, ex_LOS_variable));
        }

        [Route("download_admin_logs")]
        public IActionResult Download_Admin_Logs()
        {
            return ExcelFile(settings.GetAdminLogsExport(), "Admin_Logs.xlsx");
        }

        [Route("download_secretary_logs")]
        public IActionResult Download_Secretary_Logs()
        {
            return ExcelFile(settings.GetSecretaryLogsExport(), "Secretary_Logs.xlsx");
        }

        private IActionResult ExcelFile(DataSet data, string fileName)
        {
            using (XLWorkbook work_book = new())
            {
                work_book.Worksheets.Add(data);

                using (MemoryStream stream_value = new())
                {
                    work_book.SaveAs(stream_value);

                    return File(stream_value.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
    }
}