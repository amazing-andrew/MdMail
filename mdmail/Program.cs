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


            Models.Mail mail = new Models.Mail();
            mail.To = options.To;
            mail.CC = options.CC;
            mail.BCC = options.BCC;
            mail.Subject = options.Subject;


            //Applies the handlebars templating
            ApplyTemplate(options.FilePath, options.data, mail);


            //CONVERT TEMPLATE TEXT FROM MARKDOWN TO HTML
            mail.Body = Markdown(mail.Body);

            //Apply pre-mail styles to the html body
            ApplyPreMailStyles(options.Style, mail);

            DisplayInOutlook(mail);
        }

        private static void ApplyPreMailStyles(string stylesheetFilePath, Models.Mail mail)
        {
            if (stylesheetFilePath == null || !File.Exists(stylesheetFilePath))
                return;


            string cssText = File.ReadAllText(stylesheetFilePath);
            string css = "<style type=\"text/css\">{0}</style>\r\n".Fmt(cssText);
            mail.Body = css + mail.Body;

            var result = PreMailer.Net.PreMailer.MoveCssInline(mail.Body);
            mail.Body = result.Html;
        }

        private static void ApplyTemplate(string templateFilePath, object templateData, Models.Mail mail)
        {
            //GET TEMPLATE TEXT
            // -----------------------------------------------------
            string templateSource = File.ReadAllText(templateFilePath);

            //GET HANDLEBARS TEMPLATE
            // -----------------------------------------------------
            HandlebarsEngine handlebars = new HandlebarsEngine();
            var bodyTemplate = handlebars.Compile(templateSource);
            mail.Body = bodyTemplate(templateData);


            //DETECT SUBJECT, TO, CC, BCC fields from template
            // -----------------------------------------------------
            Regex detection = new Regex(
                  "^\r\n(\r\nSubject:(?<SUBJECT>.*?)\\n\r\n|\r\nTo:(?<TO>.*?)\\n\r\n|\r\nCC"+
                  ":(?<CC>.*?)\\n\r\n|\r\nBCC:(?<BCC>.*?)\\n\r\n)*",
                RegexOptions.IgnoreCase
                | RegexOptions.CultureInvariant
                | RegexOptions.IgnorePatternWhitespace
                );


            if (mail.Body != null)
            {
                //normalize newlines
                mail.Body = mail.Body.Replace("\r\n", "\n");

                //run detection
                var match = detection.Match(mail.Body);

                if (match.Success)
                {
                    if (mail.To == null && match.Groups["TO"] != null)
                        mail.To = match.Groups["TO"].Value.NullSafeTrim().ToNullIfWhiteSpace();

                    if (mail.CC == null && match.Groups["CC"] != null)
                        mail.CC = match.Groups["CC"].Value.NullSafeTrim().ToNullIfWhiteSpace();

                    if (mail.BCC == null && match.Groups["BCC"] != null)
                        mail.BCC = match.Groups["BCC"].Value.NullSafeTrim().ToNullIfWhiteSpace();

                    if (mail.Subject == null && match.Groups["SUBJECT"] != null)
                        mail.Subject = match.Groups["SUBJECT"].Value.NullSafeTrim().ToNullIfWhiteSpace();

                    //remove matches from original body
                    mail.Body = detection.Replace(mail.Body, "");
                }

                //re-introduce \r\n from \n
                mail.Body = mail.Body.Replace("\n", "\r\n");                
            }
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

        private static void DisplayInOutlook(Models.Mail m)
        {
            NetOffice.OutlookApi.Application outlook = new NetOffice.OutlookApi.Application();

            

            var outlookMailItem = outlook.CreateItem(NetOffice.OutlookApi.Enums.OlItemType.olMailItem) as NetOffice.OutlookApi.MailItem;

            if (m.To != null)
                outlookMailItem.To = m.To;

            if (m.CC != null)
                outlookMailItem.CC = m.CC;

            if (m.BCC != null)
                outlookMailItem.BCC = m.BCC;

            if (m.Subject != null)
                outlookMailItem.Subject = m.Subject;

            outlookMailItem.Display(false); //display first b4 we set the body to obtain the default signature

            if (m.Body != null)
                outlookMailItem.HTMLBody = m.Body + outlookMailItem.HTMLBody;
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
