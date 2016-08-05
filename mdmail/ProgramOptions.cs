using EmailTemplate.Wrapping;
using Mono.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate
{
    public class ProgramOptions
    {
        //HELP DESCRIPTIONS TEXT
        public string ProgramName { get; private set; }
        public string ProgramVersion { get; private set; }
        public string ProgramDescription { get; private set; }
        public string Example { get; private set; }
        public string ErrorMessage { get; private set; }


        private List<string> UnusedArgs = new List<string>();
        private readonly OptionSet options;
        public Newtonsoft.Json.Linq.JObject data = new Newtonsoft.Json.Linq.JObject();



        //OPTIONS
        public bool ShouldShowHelp { get; set; }


        public string To { get; set; }
        public string CC { get; set; }
        public string BCC { get; set; }

        public string Subject { get; set; }
        public string FilePath { get; set; }

        public string Style { get; set; }



        public ProgramOptions ()
	    {
            string assemblyLocation = Assembly.GetEntryAssembly().Location;

            string processName = Path.GetFileNameWithoutExtension(assemblyLocation);

            this.ProgramName = "Email Template";
            this.ProgramVersion = "v" + System.Diagnostics.FileVersionInfo.GetVersionInfo(assemblyLocation).FileVersion;
            this.ProgramDescription = "Loads and displays an email template into outlook. Uses an markdown markup and data passed into handlebars.js to fill out the template. You can also supply a stylesheet to apply to the email.\r\n\r\nNOTE: You can set the \"To\", \"CC\", \"BCC\", and \"Subject\" fields from within the template by having first few lines start with \"To:\", \"CC:\", \"BCC:\", and/or \"Subject:\"";
            this.Example = "{0} \"markdown.md\" [list of properties]\r\n\r\nlist of properties: key=value (i.e. user.name=andrew)".Fmt(processName);


            //set defaults
            this.Style = "markdown.css";

            options = new OptionSet()
                .Add("sub|subject=", "sets the subject of the email", x => Subject = x)
                .Add("t|to=", "sets the to of the email", x => To = x)
                .Add("cc=", "sets the to of the email", x => CC = x)
                .Add("bcc=", "sets the from of the email", x => BCC = x)
                .Add("css|style=", "sets the file that contains the css style sheet for the template defaults to (markdown.css)", x => Style = x)
                .Add("?|help", "displays this help message", x => ShouldShowHelp = true);
	    }

        private void SetData(string key, string value)
        {
            var nameSplit = key.Split(".").Select(x => x.NullSafeTrim()).ToList();

            JObject parent = data;

            for (int i = 0; i < nameSplit.Count; i++)
            {
                string name = nameSplit[i];
                bool isLast = i == nameSplit.Count - 1;

                if (isLast)
                {
                    parent[name] = value;
                }
                else
                {
                    if (parent.GetValue(name) == null)
                        parent.Add(name, new JObject());
                    parent = parent[name] as JObject;
                }


            }
        }
        
        public void Parse(IEnumerable<string> args)
        {
            if (args.Count() == 0)
            {
                this.ShouldShowHelp = true;
                return;
            }
            
            UnusedArgs = options.Parse(args);

            //fill out file path first 
            if (UnusedArgs.Count > 0)
            {
                this.FilePath = UnusedArgs[0];
                UnusedArgs.RemoveAt(0);
            }

            string tempKey = null;
            while (UnusedArgs.Count > 0) {
                var unused = UnusedArgs[0];
                char[] kvSplitChars = new char[] { '=', ':' };
                bool isKeyValue = unused.IndexOfAny(kvSplitChars) > -1;

                if (isKeyValue) {
                    var key = unused.Substring(0, unused.IndexOfAny(kvSplitChars));
                    var value = unused.Remove(0, key.Length + 1);
                    SetData(key, value);
                }
                else {
                    if (tempKey == null) {
                        tempKey = unused;
                    }
                    else {
                        string tempValue = unused;
                        SetData(tempKey, tempValue);

                        tempKey = null;
                        tempValue = null;
                    }
                }

                UnusedArgs.RemoveAt(0);
            }
           
            //validate
            this.Validate();

            if (ErrorMessage != null)
                ShouldShowHelp = true;
        }

        public void Validate()
        {
            if (UnusedArgs.Count > 0)
            {
                ErrorMessage = "Unknown arguments: {0}".Fmt(UnusedArgs.JoinStrings(", "));
                return;
            }

            if (FilePath == null)
            {
                ErrorMessage = "File path is required.";
                return;
            }

            if (!File.Exists(FilePath))
            {
                ErrorMessage = "File \"{0}\" does not exist.".Fmt(FilePath);
                return;
            }
        }


        public string GetHelpText()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            TextWrapper wrapper = new TextWrapper();
            wrapper.Width = Math.Min(70, Console.BufferWidth - 1);
            wrapper.ReplaceWhiteSpace = false;

            string title = string.Format("{0}{1}",
                ProgramName ?? Assembly.GetExecutingAssembly().GetName().Name,
                " " + ProgramVersion).Trim();

            sb.AppendLine(title);
            sb.AppendLine();


            if (this.ProgramDescription != null)
            {
                string desc = wrapper.Wrap(ProgramDescription).JoinStrings("\r\n");
                sb.AppendLine(desc);
                sb.AppendLine();
            }

            if (this.Example != null)
            {
                string exmp = wrapper.Wrap(Example).JoinStrings("\r\n");

                sb.AppendLine();
                sb.AppendLine(exmp);
                sb.AppendLine();
            }

            options.WriteOptionDescriptions(writer);
            sb.AppendLine();

            return sb.ToString();
        }
    }
}
