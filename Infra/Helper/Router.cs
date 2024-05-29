using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infra.Helper
{
    public struct BaseUrl
    {
        public const string DmaExam = "https://localhost:7121/";
        
    }
    public struct Request
    {
        public const string DmaExam = "DmaExam";

    }

    public static class Router
    {

        public static string route(this string Route, string project)
        {
            string route = null;
            switch (project)
            {
                case Request.DmaExam:
                    route = $"{BaseUrl.DmaExam}{Route}";
                    break;

                default:
                    throw new ArgumentException("Invalid Route");
            }
            return route;
        }
    }
}
