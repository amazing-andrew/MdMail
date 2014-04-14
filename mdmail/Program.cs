using EmailTemplate.Wrapping;
using Jurassic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace EmailTemplate
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            ConsoleHelper con = new ConsoleHelper();

            ProgramOptions options = new ProgramOptions();
            options.Parse(args);

            if (options.ShouldShowHelp)
            {
                if (options.ErrorMessage != null)
                {
                    con.WriteLineInColor(ConsoleColor.Yellow, options.ErrorMessage);
                    con.WriteLine();
                    con.WriteLine();
                }

                con.WriteLine(options.GetHelpText());
                return;
            }


            //GET TEMPLATE TEXT
            string templateSource = File.ReadAllText(options.FilePath);

            //GET HANDLEBARS TEMPLATE
            HandlebarsEngine handlebars = new HandlebarsEngine();
            var bodyTemplate = handlebars.Compile(templateSource);
            var body = bodyTemplate(options.data);

            //DETECT SUBJECT
            string subject = options.Subject;
            if (subject == null)
            {
                DetectSubject(ref body, ref subject);
            }

            //CONVERT TEMPLATE TEXT FROM MARKDOWN TO HTML
            body = Markdown(body);


            if (options.Style != null)
            {
                if (File.Exists(options.Style))
                {
                    
                        string cssText = File.ReadAllText(options.Style);
                        string css = "<style type=\"text/css\">{0}</style>\r\n".Fmt(cssText);
                        body = css + body;
                        var result = PreMailer.Net.PreMailer.MoveCssInline(body);
                        body = result.Html;
                }
            }


            LoadEmail(options.To, options.CC, options.BCC, subject, body);
        }

        private static string Markdown(string body)
        {
            MarkdownSharp.MarkdownOptions mdoptions = new MarkdownSharp.MarkdownOptions();
            mdoptions.AutoHyperlink = true;
            MarkdownSharp.Markdown md = new MarkdownSharp.Markdown(mdoptions);
            body = md.Transform(body);
            return body;
        }

        private static void DetectSubject(ref string body, ref string subject)
        {
            //try to detect subject line in template
            Regex subjectRegex = new Regex(@"^Subject:(?<SUBJECT>.*?)\r\n");
            Match m = subjectRegex.Match(body);
            if (m.Success)
            {
                subject = m.Groups["SUBJECT"].Value.NullSafeTrim();
                body = subjectRegex.Replace(body, "");
            }
        }

        private static void LoadEmail(string to, string cc, string bcc, string subject, string body)
        {
            NetOffice.OutlookApi.Application outlook = new NetOffice.OutlookApi.Application();

            var mail = outlook.CreateItem(NetOffice.OutlookApi.Enums.OlItemType.olMailItem) as NetOffice.OutlookApi.MailItem;

            if (to != null)
                mail.To = to;

            if (cc != null)
                mail.CC = cc;

            if (bcc != null)
                mail.BCC = bcc;

            if (subject != null)
                mail.Subject = subject;

            if (body != null)
                mail.HTMLBody = body;

            mail.Display(false);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            ConsoleHelper c = new ConsoleHelper();
            c.WriteLineInColor(ConsoleColor.Red, ex.Message);
            Environment.Exit(-1);
        }
    }
}
