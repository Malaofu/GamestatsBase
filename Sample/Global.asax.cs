﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Sample
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
        }

        void Application_BeginRequest(object sender, EventArgs e)
        {
            String pathInfo, query;
            String targetUrl = RewriteUrl(Request.Url.PathAndQuery, out pathInfo, out query);

            if (targetUrl != null)
            {
                Context.RewritePath(targetUrl, pathInfo, query, false);
            }
        }

        public static String RewriteUrl(String url, out String pathInfo, out String query)
        {
            int q = url.IndexOf('?');
            String path;
            pathInfo = "";

            if (q < 0)
            {
                path = url;
                query = "";
            }
            else
            {
                path = url.Substring(0, q);
                query = url.Substring(q + 1);
            }

            // Since our handler is ashx, not ASP classic, we need to rewrite
            // the .asp file extension so it will execute.
            if (path.Length < 4) return null;
            if (path.Substring(path.Length - 4).ToLowerInvariant() != ".asp") return null;
            return path.Substring(0, path.Length - 4) + ".ashx";
        }
    }
}
