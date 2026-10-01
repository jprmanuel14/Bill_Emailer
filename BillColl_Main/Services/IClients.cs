using BillColl_Main.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IClients
    {
        IEnumerable<Clients> GetClients(string clientname="", string clientContact = "", string clientEmail = "", string partner = ""
            , string clientContactSort = "", string clientEmailSort = "", string partnerSort = "", string sortH = "");

        IEnumerable<Clients> GetClientsAll(string clientname = "", string clientContact = "", string clientEmail = "", string partner = ""
         , string clientContactSort = "", string clientEmailSort = "", string partnerSort = "", string sortH = "");

        Clients GetClient(string clientcode);     
        //IEnumerable<ClientContact> GetClientContacts(string searchstring);
        //IEnumerable<Partners> GetPartners(string searchstring);
        

    }
}
