using BillColl_Main.Class;
using BillColl_Main.Helper;
using BillColl_Main.Models;
using BillColl_Main.Securities;
using BillColl_Main.Services;
using BillColl_Main.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Controllers
{
    [Authorize]
    public class ClientsController : Controller
    {
        private readonly IClients clients;
        private readonly IContacts contacts;
        private readonly IExceptions exceptions;
        private readonly IGroup group;
        private readonly ISharedSql sharedSql;
        private readonly ILogs logs;
        private readonly IBillingMailStatus billingMailStatus;
        private readonly IDataProtector dataProtector;
        private Logger log = LogManager.GetCurrentClassLogger();
        public ClientsController(IClients clients, IDataProtectionProvider dataProtectionProvider, DPPurposeStrings dPPurposeStrings
            , IContacts contacts, IExceptions exceptions, IGroup group, ISharedSql sharedSql, IBillingMailStatus billingMailStatus, ILogs logs)
        {
            this.clients = clients;
            this.contacts = contacts;
            this.exceptions = exceptions;
            this.group = group;
            this.sharedSql = sharedSql;
            this.billingMailStatus = billingMailStatus;
            this.logs = logs;
            dataProtector = dataProtectionProvider.CreateProtector(dPPurposeStrings.ClientIDKey);
        }
        public IActionResult Index()
        {
            ContactsViewModel contactsViewModel = new ContactsViewModel()
            {
                Groups = group.GetGroups().ToList()
            };

            return View(contactsViewModel);
        }

        public IActionResult GetClients(int pageNumber = 1, int pageSize = 10, string clientName = "", string clientContact = "", string clientEmail = ""
            , string partner = "", string clientContactSort = "", string clientEmailSort = "", string partnerSort = "", string groupSearch = "", string groupSearchSort = "", string sortH = ""
            )
        {

            try
            {

            

            clientName = ifValueNull(clientName);
            groupSearch = ifValueNull(groupSearch);
            clientEmail = ifValueNull(clientEmail);
            clientContactSort = "";
            clientEmailSort = ifValueNull(clientEmailSort);
            partnerSort = "";
            groupSearchSort = ifValueNull(groupSearchSort);

            var myClients = clients.GetClients(clientName, clientContact, clientEmail, partner, clientContactSort, clientEmailSort, partnerSort, sortH);

            var myException = exceptions.GetExceptionsAll();



            myClients.ToList().ForEach(e =>
            {
                e.eClientId = dataProtector.Protect(e.ClientName + "|" + e.ClientId);
                var clientContactsList = contacts.GetContacts(e.ClientId).ToList();
                e.ContactEmail = string.Join(", ", clientContactsList.Select(c => c.Email));
                e.GroupName = string.Join(", ", contacts.GetEngagementTeamsWithGroupName(e.ClientId).Select(g => g.GroupName).Distinct());
                e.ContactEmailD = clientContactsList.FirstOrDefault()?.Email ?? "";
            });



            var excludedClientCode = new HashSet<string>(myException.Select(p => p.ClientCode));
            var joingroup = myClients.Where(p => !excludedClientCode.Contains(p.ClientId));

            if(groupSearch != "")
            {
                joingroup = joingroup.Where(e => e.GroupName.ToUpper().Contains(groupSearch.ToUpper()));
            }

            if (clientEmail != "")
            {
                joingroup = joingroup.Where(e => e.ContactEmail.ToUpper().Contains(clientEmail.ToUpper()));

            }

            if (groupSearchSort == "A-Z")
            {
                joingroup = joingroup.OrderBy(e => e.GroupName);
            }
            else if (groupSearchSort == "Z-A")
            {
                joingroup = joingroup.OrderByDescending(e => e.GroupName);
            }


            if (clientEmailSort == "A-Z")
            {
                joingroup = joingroup.OrderBy(e => e.ContactEmailD);
            }
            else if (clientEmailSort == "Z-A")
            {
                joingroup = joingroup.OrderByDescending(e => e.ContactEmailD);
            }

            var pagedData = Pagination.PagedResult(joingroup.ToList(), pageNumber, pageSize);

            return Json(pagedData);
            
            }
            catch (Exception myerror)
            {
                log.Error(myerror.Message.ToString());
                throw;
            }

        }

        [HttpPost]
        public IActionResult GetContactEmailList(string searchstr)
        {
            searchstr = ifValueNull(searchstr);

            var mylist = contacts.GetContactsByEmail(searchstr).Select(e=>e.Email).Distinct();
         

            return Json(mylist);
        }


        [HttpPost]
        public IActionResult GetGroupList(string searchstr)
        {
            searchstr = ifValueNull(searchstr);

            var mylist = group.GetGroups(searchstr).Select(e => e.Description).Distinct();

            return Json(mylist);
        }

        public IActionResult GetCContactsLists([FromQuery(Name = "Reference")] string reference)
        {
            string myref = dataProtector.Unprotect(reference);
            string[] arr = myref.Split("|");
            string myClientCode = arr[1];
            var myCContactList = contacts.GetContacts(myClientCode);
            myCContactList.ToList().ForEach(e =>
            {
                e.eId = dataProtector.Protect(e.Id.ToString());
                e.FirstName = ifValueNull(e.FirstName);
                e.MiddleName = ifValueNull(e.MiddleName);
                e.LastName = ifValueNull(e.LastName);
                e.Designation = ifValueNull(e.Designation);
                e.Salutation = ifValueNull(e.Salutation);
            });
            return Json(myCContactList);
        }

        public IActionResult GetEContactsLists([FromQuery(Name = "Reference")] string reference)
        {
            string myref = dataProtector.Unprotect(reference);
            string[] arr = myref.Split("|");
            string myClientCode = arr[1];
            var myEContactList = contacts.GetEngagementTeams(myClientCode);

            myEContactList.ToList().ForEach(e => { e.GroupName = group.GetGroup(e.GroupId).Description; });

            List<EngagementTeamList> engagementTeamLists = new List<EngagementTeamList>();
            foreach (var mydata in myEContactList)
            {
                var addETitems = new EngagementTeamList()
                {
                    eId = dataProtector.Protect(mydata.Id.ToString()),
                    Email = mydata.Email,
                    ClientCode = mydata.ClientCode,
                    ContactNumber = ifValueNull(mydata.ContactNumber),
                    Designation = ifValueNull(mydata.Designation),
                    GroupName = mydata.GroupName,
                    Name = mydata.Name,
                    Id = mydata.Id
                };

                engagementTeamLists.Add(addETitems);
            }


            return Json(engagementTeamLists);
        }

        public IActionResult GetContactInfo([FromQuery(Name = "Reference")] string reference)
        {
            string myref = dataProtector.Unprotect(reference);
            string[] myarr = myref.Split("|");


            return Json(myarr);
        }

        [HttpPost]
        public IActionResult Submit(ContactsViewModel contactsViewModel)
        {
            string myref = dataProtector.Unprotect(contactsViewModel.eClientCode);
            string[] arr = myref.Split("|");
            string myClientCode = arr[1];
            string myCompanyName = clients.GetClient(myClientCode).ClientName;
            string returntoken = "";

            int maxSizeLimit = contacts.GetContactsLimit().Limit;
            int currentLimit = contacts.GetContacts(arr[1]).Count();

            if (ModelState.IsValid)
            {
                if (contactsViewModel.IsEngagement)
                {
                    returntoken = dataProtector.Protect(myCompanyName + "|" + myClientCode + "|" + "E" + "|" + contactsViewModel.pageNumber + "|" + contactsViewModel.pageSize + "|" + contactsViewModel.SearchText);
                    int groupid;
                    //CheckgroupID
                    if (contactsViewModel.GroupName != null)
                    {
                        groupid = group.GetGroup(contactsViewModel.GroupName).Id;
                    }
                    else
                    {
                        groupid = group.GetGroup("Unassigned").Id;
                    }
                    if (contactsViewModel.eId != null && contactsViewModel.eId != "")
                    {
                        int UpdateId = Convert.ToInt32(dataProtector.Unprotect(contactsViewModel.eId));
                        var ExistingEContact = contacts.GetEngagementTeam(UpdateId);

                        ExistingEContact.Name = contactsViewModel.Name;
                        ExistingEContact.Email = contactsViewModel.Email;
                        ExistingEContact.ContactNumber = ifValueNull(contactsViewModel.ContactNumber);
                        ExistingEContact.Designation = contactsViewModel.Designation;
                        ExistingEContact.GroupId = groupid;
                        contacts.UpdateEngagementTeam(ExistingEContact);

                    }
                    else
                    {
                        EngagementTeam newEngagementTeam = new EngagementTeam()
                        {
                            ClientCode = myClientCode,
                            Name = contactsViewModel.Name,
                            Email = contactsViewModel.Email,
                            ContactNumber = ifValueNull(contactsViewModel.ContactNumber),
                            Designation = contactsViewModel.Designation,
                            GroupId = groupid
                        };
                        contacts.AddEngagementTeam(newEngagementTeam);
                    }


                    //Get current statementdate

                    DateTime? currStatementDate = billingMailStatus.GetRecentStatementDate(myClientCode);
                    if (currStatementDate != null)
                    {
                        //Update engagement
                        billingMailStatus.UpdateEngagements(myClientCode, Convert.ToDateTime(currStatementDate));
                    }

                }
                else
                {
             
                    returntoken = dataProtector.Protect(myCompanyName + "|" + myClientCode + "|" + "C" + "|" + contactsViewModel.pageNumber + "|" + contactsViewModel.pageSize + "|" + contactsViewModel.SearchText);
                    if (contactsViewModel.eId != null && contactsViewModel.eId != "")
                    {
                        int UpdateId = Convert.ToInt32(dataProtector.Unprotect(contactsViewModel.eId));
                        var ExistingCContact = contacts.GetContact(UpdateId);

                        ExistingCContact.Email = contactsViewModel.Email;
                        ExistingCContact.ContactNumber = ifValueNull(contactsViewModel.ContactNumber);
                        ExistingCContact.Designation = ifValueNull(contactsViewModel.Designation);
                        ExistingCContact.FirstName = ifValueNull(contactsViewModel.FirstName);
                        ExistingCContact.MiddleName = ifValueNull(contactsViewModel.MiddleName);
                        ExistingCContact.LastName = ifValueNull(contactsViewModel.LastName);
                        ExistingCContact.Salutation= ifValueNull(contactsViewModel.Salutation);
                        contacts.UpdateContact(ExistingCContact);
                    }
                    else
                    {

                        if (currentLimit >= maxSizeLimit)
                        {
                            return RedirectToAction("Error", "Home");
                        }

                        Contacts newContacts = new Contacts()
                        {
                            ClientCode = myClientCode,
                            Email = contactsViewModel.Email,
                            ContactNumber = ifValueNull(contactsViewModel.ContactNumber),
                            Designation = ifValueNull(contactsViewModel.Designation),
                            FirstName = ifValueNull(contactsViewModel.FirstName),
                            MiddleName = ifValueNull(contactsViewModel.MiddleName),
                            LastName = ifValueNull(contactsViewModel.LastName),
                            Salutation = ifValueNull(contactsViewModel.Salutation)
                        };
                        contacts.AddContact(newContacts);
                    }

                    //Get current statementdate

                    DateTime? currStatementDate = billingMailStatus.GetRecentStatementDate(myClientCode);
                    if(currStatementDate != null)
                    {
                        //Update contacts
                        billingMailStatus.UpdateContacts(myClientCode, Convert.ToDateTime(currStatementDate));
                    }

                }
                return RedirectToAction("Index", new { reference = returntoken });

            }

            return RedirectToAction("Index");

        }

        [HttpDelete]
        public IActionResult DeleteC([FromRoute] string id, [FromQuery(Name = "Reference")] string reference)
        {
            int CId = Convert.ToInt32(dataProtector.Unprotect(reference));
            var myclientProfile = clients.GetClient(id);
            var myCprofile = contacts.DeleteContact(CId);
            var myref = dataProtector.Protect(myclientProfile.ClientName + "|" + myclientProfile.ClientId);



            //Get current statementdate

            DateTime? currStatementDate = billingMailStatus.GetRecentStatementDate(myclientProfile.ClientId);
            if (currStatementDate != null)
            {
                //Update contacts
                billingMailStatus.UpdateContacts(myclientProfile.ClientId, Convert.ToDateTime(currStatementDate));
            }

            return Json(myref);
        }

        [HttpDelete]
        public IActionResult DeleteE([FromRoute] string id, [FromQuery(Name = "Reference")] string reference)
        {
            int EId = Convert.ToInt32(dataProtector.Unprotect(reference));
            var myclientProfile = clients.GetClient(id);
            var myEprofile = contacts.DeleteEngagementTeam(EId);
            var myref = dataProtector.Protect(myclientProfile.ClientName + "|" + myclientProfile.ClientId);

            //Get current statementdate

            DateTime? currStatementDate = billingMailStatus.GetRecentStatementDate(myclientProfile.ClientId);
            if (currStatementDate != null)
            {
                //Update engagement
                billingMailStatus.UpdateEngagements(myclientProfile.ClientId, Convert.ToDateTime(currStatementDate));
            }



            return Json(myref);
        }

        [HttpPost]
        public IActionResult MoveToException(string reference, string remarks)
        {
            string mydata = dataProtector.Unprotect(reference);
            string[] myarr = mydata.Split("|");

            Exceptions myexception = new Exceptions()
            {
                ClientCode = myarr[1],
                ClientName = myarr[0],
                Reason = remarks
            };

            var newException = exceptions.AddExceptions(myexception);

            billingMailStatus.UpdateTblScheduleMailStatus(myarr[1], "HOLD");

            return Json("Move to exception list is successful");
        }

        [Route("groupcodelist")]
        public IActionResult Get_GroupAndLOS()
        {
            return Json(group.GetGroupCodeList());
        }

        /// <summary>
        /// Line of Service lookup for a selected group code. The mapping table lives in the
        /// shared SQL Server estate (PH Report Databank), which is a separate connection
        /// from the primary database. Returns an empty list when that connection is not
        /// configured, and the client falls back to manual LoS entry.
        /// </summary>
        [Route("losbygroupcodelist")]
        public IActionResult Get_LOSByGroupcode([FromQuery] Group_Ver group_ver_variable, string GroupType)
        {
            var losByGroup = sharedSql.GetLOSByGroupCode(GroupType);

            return Json(losByGroup == null ? new List<Group_Ver>() : new List<Group_Ver> { losByGroup });
        }

        [Route("load_reason")]
        public IActionResult Load_Reason()
        {
            return Json(exceptions.GetReasons());
        }

        [Route("load_deletion_reason")]
        public IActionResult Load_Deletion_Reason()
        {
            return Json(exceptions.GetDeletionReasons());
        }

        [Route("viewlogreport_clientengagement")]
        public IActionResult View_Log_Report_ClientEngagement()
        {
            return Json(logs.GetClientActivityLogs());
        }

        [Route("view_exception")]
        public IActionResult View_Exception([FromQuery] int i_variable)
        {
            var exception = exceptions.GetExceptionById(i_variable);

            if (exception == null)
            {
                log.Warn("view_exception requested for a missing exception id {ExceptionId}.", i_variable);
                return Json(Array.Empty<ExceptionDetail>());
            }

            return Json(new[] { exception });
        }

        [Route("update_exception")]
        public IActionResult Update_Exception([FromQuery] ExceptionUpdateViewModel exception_variable)
        {
            if (exception_variable == null || exception_variable.Id <= 0 || string.IsNullOrWhiteSpace(exception_variable.Reason))
            {
                return BadRequest(new { message = "A valid exception id and reason are required." });
            }

            var otherReason = exception_variable.Reason == "Others"
                ? exception_variable.Reason + " - " + exception_variable.Other_Reason
                : exception_variable.Reason;

            var updated = exceptions.UpdateExceptionReason(exception_variable.Id, exception_variable.Reason, otherReason);

            if (!updated)
            {
                log.Warn("update_exception did not match any row for exception id {ExceptionId}.", exception_variable.Id);
                return NotFound(new { message = "Exception not found." });
            }

            logs.AddClientActivityLog(
                exception_variable.User_Name,
                exception_variable.ClientName,
                exception_variable.Invoice_Number,
                exception_variable.Action,
                exception_variable.Previous_Reason,
                otherReason,
                null,
                exception_variable.Invoice_Number);

            return Json(exception_variable);
        }

        public IActionResult GetCurrentPageSize([FromQuery(Name ="Reference")]string myref)
        {
            string[] myarr = dataProtector.Unprotect(myref).Split("|");

            return Json(myarr);
        }

        private static string ifValueNull(string a)
        {
            if (a == null)
            {
                a = "";
            }

            return a;
        }


        public int GetClientContactLimit()
        {
            var contactsLimit = contacts.GetContactsLimit();

            if (contactsLimit == null)
            {
                return DefaultContactLimit;
            }

            return contactsLimit.Limit;
        }

        private const int DefaultContactLimit = 5;


    }
}
