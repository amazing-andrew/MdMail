using MarkdownSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate.Tests
{
    [TestClass]
    public class MarkDownTest 
    {
        public string RunMarkdown(string s)
        {
            MarkdownOptions options = new MarkdownOptions();
            options.AutoHyperlink = true;

            Markdown md = new Markdown(options);
            return md.Transform(s);
        }


        [TestMethod]
        public void AutoLink()
        {
            string test = "check out http://google.com.";
            string result = RunMarkdown(test);

            Assert.IsTrue(result.Contains("<a"));
        }
    }
}
