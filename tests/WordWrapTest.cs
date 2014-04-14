using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate.Tests
{
    [TestClass]
    public class WordWrapTest
    {




        //[TestMethod]
        //public void WordWrap()
        //{
        //    string testString = "I like pie.";

        //    string expected = "I like\r\npie.";
        //    string actual = ConsoleHelper.WordWrap(testString, 10, 0, 0);

        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod]
        //public void WordWrap2()
        //{
        //    string testString = "Would you like some of my new strawberry pie?";

        //    string expected = "Would you\r\nlike some\r\nof my new\r\nstrawberry\r\npie?";
        //    string actual = ConsoleHelper.WordWrap(testString, 10, 0, 0);

        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod]
        //public void WordWrap3()
        //{

        //    string testString = "I would like some pie soon please!";

        //    string expected = "  I would like      " + "\r\n" + "  some pie soon     " + "\r\n" + "  please!";

        //    string actual = ConsoleHelper.WordWrap(testString, 20, 2, 2);

        //    Assert.AreEqual(expected, actual);
        //}

        //[TestMethod]
        //public void WordWrap4()
        //{
        //    string testString = "JSON data";

        //    string expected = "JSON \r\ndata";

        //    string acutal = ConsoleHelper.WordWrap(testString, 5, 0, 0);

        //    Assert.AreEqual(expected, acutal);
        //}

        //[TestMethod]
        //public void WordWrap5()
        //{
        //    string testString = "JSON  data";

        //    string expected = "JSON \r\ndata";

        //    string acutal = ConsoleHelper.WordWrap(testString, 5, 0, 0);

        //    Assert.AreEqual(expected, acutal);
        //}

        //[TestMethod]
        //public void WordWrap6()
        //{
        //    string testString = "json data goes\r\n here";

        //    string expected = "json data \r\ngoes      \r\nhere";

        //    string acutal = ConsoleHelper.WordWrap(testString, 10, 0, 0);

        //    Assert.AreEqual(expected, acutal);
        //}

        //[TestMethod]
        //public void TestDesc()
        //{
        //    string testString = "Loads and displays an email template into outlook. Uses an HTML and optional JSON data file to load data into the template using Handlebars.js";

        //    string expected = "Loads and displays an email template into outlook. Uses an HTML and optional    \r\nJSON data file to load data into the template using Handlebars.js";

        //    string actual = ConsoleHelper.WordWrap(testString, 80, 0, 0);

        //    Assert.AreEqual(expected, actual);
        //}
    }
}
