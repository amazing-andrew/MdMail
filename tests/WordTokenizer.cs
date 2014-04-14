using EmailTemplate.Wrapping;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate.Tests
{
    [TestClass]
    public class WordTokenizerTests
    {
        [TestMethod]
        public void MyTestMethod()
        {
            string test = "My special string. I really like it alot.";

            StringReader sr = new StringReader(test);

            WordReader wr = new WordReader(sr);

            while(wr.Read())
            {
                Console.WriteLine(
                    "{0}: {1}".Fmt(wr.Type, wr.Text)
                    );
            }
        }
    }
}
