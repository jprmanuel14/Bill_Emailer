using BillColl_Main.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Models
{
    public class Contacts: CommonContactInfoBase
    {
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Salutation { get; set; }

        [NotMapped]
        public string Group_Description { get; set; }

        public string GroupType { get; set; }
        public string Los { get; set; }
    }
}
