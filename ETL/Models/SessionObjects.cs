using System;
using System.Net.Http.Headers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ETL.Models
{
    public static class SessionObjects
    {
        private static IHttpContextAccessor _httpContextAccessor;
        public static Int32 UserId {
            get {return UserId; }
            set {
                UserId = value;
            }
        }

        public static DataModel DataModel {
            get { return DataModel; }
            set { DataModel = value; }
        }

        public static void InitSession()
        {
            DataModel = new DataModel();
        }

        public static bool setMapping(IHttpContextAccessor httpContextAccessor, int val)
        {
            _httpContextAccessor = httpContextAccessor;
            //UserId = Convert.ToInt32(_httpContextAccessor.HttpContext.Items["User"]);
            _httpContextAccessor.HttpContext.Items.Add("UserId", val++);
            
            return true;
        }
    }
}
