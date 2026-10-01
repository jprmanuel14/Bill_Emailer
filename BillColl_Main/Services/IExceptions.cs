using BillColl_Main.Class;
using BillColl_Main.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Services
{
    public interface IExceptions
    {
        IEnumerable<ExceptionsList> GetExceptions(string clientName = "", string dateSentPrev = "", string dateSortPrev = "");
        IEnumerable<ExceptionsList> GetExceptionsAll(string clientName = "", string dateSentPrev = "", string dateSortPrev = "");

        Exceptions RemoveExceptions(int Id);
        Exceptions AddExceptions(Exceptions exceptions);

        Exceptions GetException(string ClientCode);

        ExceptionDetail GetExceptionById(int id);

        bool UpdateExceptionReason(int id, string reason, string otherReason);

        IEnumerable<Reason> GetReasons();
        IEnumerable<Reason> GetDeletionReasons();
    }
}
