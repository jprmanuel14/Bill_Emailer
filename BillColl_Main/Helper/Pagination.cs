using BillColl_Main.Class;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BillColl_Main.Helper
{
    public static class Pagination
    {
        public static PagedData<T> PagedResult<T>(this List<T> list, int PageNumber, int PageSize) where T : class
        {
            var result = new PagedData<T>();
            //result.Data = list.Skip(PageSize * (PageNumber - 1)).Take(PageSize).ToList();
            result.Data = list.ToList();
            //result.TotalPages = Convert.ToInt32(Math.Ceiling((double)list.Count() / PageSize));
            result.TotalPages = (PageSize);
            result.CurrentPage = PageNumber;
            result.TotalData = list.Count();
            return result;
        }

    }
}
