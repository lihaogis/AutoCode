using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace Core
{
    public class HanderAdmin
    {
        string path;

        public string Path
        {
            get { return path; }
            set { path = value; }
        }

        public HanderAdmin(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// 创建一般处理程序
        /// </summary>
        public void CreateHanderAdmin(string table_name, string table_comment, DataSet ds)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(@"using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using Common;
using System.Text;
using System.Data;

namespace HanderAdmin
{");
            sb.Append("\r\n" + @"    /// <summary>");
            sb.Append("\r\n" + @"    /// " + table_comment + "");
            sb.Append("\r\n" + @"    /// <summary>");
            sb.Append("\r\n     public class "+table_name+" : IHttpHandler, System.Web.SessionState.IRequiresSessionState");
            sb.Append("\r\n" + @"     {
        public HttpContext context
        {
            set;
            get;
        }
        public void ProcessRequest(HttpContext context)
        {
            //在此处写入您的处理程序实现。
            this.context = context;
            string optionType = context.Request.PathInfo.Substring(1);");
            sb.Append("\r\n	        context.Response.ContentType =\"text/html\";//这里一定要html");
            sb.Append("\r\n" + @"            MethodInfo m = this.GetType().GetMethod(optionType);
            m.Invoke(this, null);
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}");
            StringBuilder sbb = new StringBuilder();
            sbb.Append("<%@ WebHandler Language=\"C#\" CodeBehind=\"Hand" + table_name + ".ashx.cs\" Class=\"DMGX.Hander.HandlerMain\" %>");
            File.WriteAllText(this.Path + "/" + "Hand" + table_name + ".ashx", sb.ToString());
            File.WriteAllText(this.Path + "/" + "Hand" + table_name + ".ashx.cs", sb.ToString());
        }//end CreateHanderAdmin 


    }
}
