using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate.Wrapping
{
    public class TextWrapper
    {
        public bool BreakOnLongWords { get; set; }
        public bool BreakOnHyphens { get; set; }
        public bool DropWhiteSpace { get; set; }
        public bool ReplaceWhiteSpace { get; set; }
        public string FirstLineIndent { get; set; }
        public string OtherLineIndent { get; set; }

        private int _Width;

        public int Width
        {
            get { return _Width; }
            set { 

                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException("Width", "Width must be a positive integer.");
                }

                _Width = value; 
            }
        }



        public TextWrapper()
	    {
            BreakOnLongWords = true;
            BreakOnHyphens = true;
            DropWhiteSpace = true;
            ReplaceWhiteSpace = true;
            Width = 70;
	    }

        public TextWrapper(int width) : this()
        {
            Width = width;
        }


        public IEnumerable<string> Wrap(string source)
        {
            return Wrap(new StringReader(source));
        }

        public IEnumerable<string> Wrap(TextReader reader)
        {
            int totalLineLength = this.Width;

            WordReader wr = new WordReader(reader);
            wr.BreakOnHyphens = this.BreakOnHyphens;
            StringBuilder sb = new StringBuilder();

            int currentLineLength = 0;

            //setup first line indentation
            if (FirstLineIndent != null)
            {
                currentLineLength = FirstLineIndent.Length;
                sb.Append(FirstLineIndent);
            }

            //decrease total line length based on our other-line indentation
            if (OtherLineIndent != null)
            {
                totalLineLength -= OtherLineIndent.Length;
            }


            while (wr.Read())
            {
                string text = wr.Text;

                if (text == null || text.Length == 0)
                    continue;

                //replace white spaces
                if (ReplaceWhiteSpace && wr.Type == WordTokenType.Space)
                    text = " ";


                var lineSplit = text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                for (int lineIndex = 0; lineIndex < lineSplit.Length; lineIndex++)
                {
                    string line = lineSplit[lineIndex];


                    ////////////////////////////////////
                    //Test for Long Words
                    ////////////////////////////////////
                    if (BreakOnLongWords && line.Length > totalLineLength)
                    {
                        //flush out string builder, so this can start on a new newline
                        if (sb.Length > 0)
                        {
                            yield return sb.ToString();
                            sb.Clear();
                            currentLineLength = 0;
                        }


                        //insert new lines until line is less than total
                        while (line.Length > totalLineLength)
                        {
                            int maxCount = Math.Min(totalLineLength, line.Length);
                            string subStr = line.Substring(0, maxCount);
                            line = line.Remove(0, maxCount);
                            yield return subStr;
                        }
                    }


                    ////////////////////////////////////
                    //DETECT NEWLINES
                    //start new line b/c we are over out limit, or we have newline in our text
                    ////////////////////////////////////

                    int testLength = currentLineLength + line.Length;
                    if (testLength > totalLineLength || lineIndex > 0) 
                    {
                        yield return sb.ToString();
                        sb.Clear();
                        currentLineLength = 0;

                        //when starting new lines, time spaces at start
                        if (DropWhiteSpace)
                            line = line.TrimStart();

                        //after the triming of spaces, input our indentation
                        if (OtherLineIndent != null)
                            line = OtherLineIndent + line;
                    }



                    /// Just append and move on to next
                    sb.Append(line);
                    currentLineLength += line.Length;
                }
            }

            yield return sb.ToString();
        }
    }
}
