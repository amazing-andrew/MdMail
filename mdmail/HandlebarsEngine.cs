using Jurassic;
using Jurassic.Library;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate
{
    public class HandlebarsEngine
    {
        private Jurassic.ScriptEngine scriptEngine;

        private Jurassic.Library.ObjectInstance handlebarsObj;
        private Jurassic.Library.FunctionInstance compileFunction;
        private Jurassic.Library.FunctionInstance preCompileFunction;


        private ScriptSource FindHandlebarsJsFile()
        {
            if (File.Exists("handlebars.js"))
                return new FileScriptSource("handlebars.js");

            if (File.Exists("script/handlebars.js"))
                return new FileScriptSource("script/handlebars.js");

            if (File.Exists("scripts/handlebars.js"))
                return new FileScriptSource("scripts/handlebars.js");


            //can't find file, use embeded one
            return new StringScriptSource(Resources.handlebars);
        }

        public HandlebarsEngine()
        {
            scriptEngine = new Jurassic.ScriptEngine();

            var handlebarsSource = FindHandlebarsJsFile();

            scriptEngine.Execute(handlebarsSource);

            handlebarsObj = (ObjectInstance)scriptEngine.Evaluate("Handlebars");
            compileFunction = (FunctionInstance) scriptEngine.Evaluate("Handlebars.compile");
            preCompileFunction = (FunctionInstance) scriptEngine.Evaluate("Handlebars.precompile");
        }

        public Func<object, string> Compile(string templateSource)
        {
            try
            {
                var templateFunction = (FunctionInstance)compileFunction.Call(handlebarsObj, templateSource);
                HandlebarsTemplate t = new HandlebarsTemplate(scriptEngine, templateFunction);

                return new Func<object, string>(obj =>
                {
                    return t.Execute(obj);
                });
            }
            catch (JavaScriptException ex)
            {
                throw new HandlebarsException("Could not execute template\r\n\r\n{0}".Fmt(ex.Message), ex);
            }
        }

        public string PreCompile(string templateSource)
        {
            var r = preCompileFunction.Call(handlebarsObj, templateSource);
            var f = r as string;
            return r.ToString(); ;
        }
    }

    public class HandlebarsTemplate
    {
        Jurassic.ScriptEngine engine;
        Jurassic.Library.FunctionInstance templateFunction;

        public HandlebarsTemplate(Jurassic.ScriptEngine engine, Jurassic.Library.FunctionInstance func)
        {
            this.engine = engine;
            this.templateFunction = func;
        }

        public string Execute(object data)
        {
            try
            {
                var jsonText = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                var jsonObject = JSONObject.Parse(engine, jsonText);
                var results = templateFunction.Call(templateFunction, jsonObject);

                return results.ToString();
            }
            catch (JavaScriptException ex)
            {
                throw new HandlebarsException("Could not execute template\r\n\r\n{0}".Fmt(ex.Message), ex);
            }
        }
    }

    [Serializable]
    public class HandlebarsException : ApplicationException
    {
        public HandlebarsException() { }
        public HandlebarsException(string message) : base(message) { }
        public HandlebarsException(string message, Exception inner) : base(message, inner) { }
        protected HandlebarsException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
