using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IRemarks
    {
        ClientRemarks AddRemarks(ClientRemarks clientRemarks);
        ClientRemarks UpdateRemarks(ClientRemarks clientRemarks);
        ClientRemarks GetRemarks(string ClientCode, DateTime StatementDate);

    }
}
