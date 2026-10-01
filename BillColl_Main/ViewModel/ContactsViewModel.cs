using BillColl_Main.CustomAttribute;
using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.ViewModel
{
    public class ContactsViewModel
    {
        public string eId { get; set; }
        public string eClientCode { get; set; }
        [Required(ErrorMessage = "Contact type is required!")]
        public bool IsEngagement { get; set; }
        [RequiredIfTrue(nameof(IsEngagement), ErrorMessage = "Name is required!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Email is required!")]
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        [RequiredIfTrue(nameof(IsEngagement), ErrorMessage = "Designation is required!")]
        public string Designation { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Salutation { get; set; }
        public string GroupName { get; set; }
        public List<Group> Groups { get; set; }

        public int pageNumber { get; set; }
        public int pageSize { get; set; }
        public string SearchText { get; set; }
    }
}
