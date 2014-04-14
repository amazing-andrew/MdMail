using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmailTemplate.Wrapping
{
    public enum WordTokenType
    {
        None,
        Text,
        Space,
    }

    public class WordToken
    {
        public string Text { get; set; }
        public WordTokenType Type { get; set; }
    }

    public class WordReader : IDisposable
    {
        protected internal enum WordReaderState
        {
            Start,
            Text,
            Space,
            Finished
        }

        public bool BreakOnHyphens { get; set; }


        private WordReaderState _currentState = WordReaderState.Start;
        private WordToken _current;
        private string _text;
        private WordTokenType _type;

        private readonly TextReader _reader;

        private bool _isEndOfFile;
        private char[] _chars;
        private int _charPos;

        private int _charsUsed;
        private int _lineStartPos;


        protected WordReaderState CurrentState
        {
            get { return _currentState; }
        }


        public WordToken Current { get { return _current;  } }
        public string Text { get { return _text; } }
        public WordTokenType Type { get { return _type; } }

        public WordReader(TextReader reader)
        {
            _reader = reader;
            _currentState = WordReaderState.Start;
            _type = WordTokenType.None;
            _text = null;
            _chars = new char[1025];
        }


        public bool Read()
        {
            if (!ReadInternal())
            {
                SetToken(WordTokenType.None);
                return false;
            }

            return true;
        }

        private bool ReadInternal()
        {
            while (true)
            {
                switch (_currentState)
                {
                    case WordReaderState.Start:
                    case WordReaderState.Text:
                    case WordReaderState.Space:
                        return ReadNextChunk();

                    case WordReaderState.Finished:
                        return false;
                        
                    default:
                        break;
                }
            }
        }

        private bool ReadNextChunk()
        {
            ShiftBufferIfNeeded();

            if (_isEndOfFile)
                return false;

            char currentChar = _chars[_charPos];

            if (currentChar == '\0')
                ReadData(false);

            if (IsBreakChar(currentChar))
            {
                string spaces = ReadSpaces();
                this.SetToken(spaces, WordTokenType.Space);
                return true;
            }
            else
            {
                string text = ReadText();
                this.SetToken(text, WordTokenType.Text);
                return true;
            }
        }


        private string ReadText()
        {
            int initialPosition = _charPos;
            int endPosition;

            while(true)
            {
                char currentChar = _chars[_charPos];

                if (currentChar == '\0') // need to read more data
                {
                    if (ReadData(true) == 0)
                    {
                        break;
                    }
                }

                bool isBreakChar = IsBreakChar(currentChar);

                if (BreakOnHyphens && IsHyphenChar(currentChar))
                {
                    string hyphens = ReadHyphens();
                    break;
                }
                

                if (!isBreakChar)
                    _charPos++;
                else
                    break;
            }
            
            //go back 1
            endPosition = _charPos;
            return new string(_chars, initialPosition, endPosition - initialPosition);
        }

        private int ReadData(bool append)
        {
            return ReadData(append, 0);
        }

        private int ReadData(bool append, int charsRequired)
        {
            if (_isEndOfFile)
                return 0;

            // char buffer is full
            if (_charsUsed + charsRequired >= _chars.Length - 1)
            {
                if (append)
                {
                    // copy to new array either double the size of the current or big enough to fit required content
                    int newArrayLength = Math.Max(_chars.Length * 2, _charsUsed + charsRequired + 1);

                    // increase the size of the buffer
                    char[] dst = new char[newArrayLength];

                    BlockCopyChars(_chars, 0, dst, 0, _chars.Length);

                    _chars = dst;
                }
                else
                {
                    int remainingCharCount = _charsUsed - _charPos;

                    if (remainingCharCount + charsRequired + 1 >= _chars.Length)
                    {
                        // the remaining count plus the required is bigger than the current buffer size
                        char[] dst = new char[remainingCharCount + charsRequired + 1];

                        if (remainingCharCount > 0)
                            BlockCopyChars(_chars, _charPos, dst, 0, remainingCharCount);

                        _chars = dst;
                    }
                    else
                    {
                        // copy any remaining data to the beginning of the buffer if needed and reset positions
                        if (remainingCharCount > 0)
                            BlockCopyChars(_chars, _charPos, _chars, 0, remainingCharCount);
                    }

                    _lineStartPos -= _charPos;
                    _charPos = 0;
                    _charsUsed = remainingCharCount;
                }
            }

            int attemptCharReadCount = _chars.Length - _charsUsed - 1;

            int charsRead = _reader.Read(_chars, _charsUsed, attemptCharReadCount);

            _charsUsed += charsRead;

            if (charsRead == 0)
                _isEndOfFile = true;

            _chars[_charsUsed] = '\0'; //make sure we mark the end of our buffer
            return charsRead;
        }

        private string ReadSpaces()
        {
            int initialPosition = _charPos;
            int endPosition;

            while (true)
            {
                char currentChar = _chars[_charPos];

                if (currentChar == '\0') // need to read more data
                {
                    if (ReadData(true) == 0)
                    {
                        break;
                    }
                }

                bool isBreakChar = IsBreakChar(currentChar);

                if (isBreakChar)
                    _charPos++;
                else
                    break;
            }

            //go back 1
            endPosition = _charPos;
            return new string(_chars, initialPosition, endPosition - initialPosition);
        }

        private string ReadHyphens()
        {
            int initialPosition = _charPos;
            int endPosition;

            while (true)
            {
                char currentChar = _chars[_charPos];

                if (currentChar == '\0') // need to read more data
                {
                    if (ReadData(true) == 0)
                    {
                        break;
                    }
                }

                bool isHyphen = IsHyphenChar(currentChar);

                if (isHyphen)
                    _charPos++;
                else
                    break;
            }

            //go back 1
            endPosition = _charPos;
            return new string(_chars, initialPosition, endPosition - initialPosition);
        }



        private void ShiftBufferIfNeeded()
        {
            // once in the last 10% of the buffer shift the remainling content to the start to avoid
            // unnessesarly increasing the buffer size when reading numbers/strings
            int length = _chars.Length;
            if (length - _charPos <= length * 0.1)
            {
                int count = _charsUsed - _charPos;
                if (count > 0)
                    BlockCopyChars(_chars, _charPos, _chars, 0, count);

                _lineStartPos -= _charPos;
                _charPos = 0;
                _charsUsed = count;
                _chars[_charsUsed] = '\0';
            }
        }

        private void BlockCopyChars(char[] src, int srcOffset, char[] dest, int destOffset, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Buffer.BlockCopy(src, srcOffset, dest, destOffset, count);
            }
        }

        private bool IsBreakChar(char c)
        {
            return char.IsWhiteSpace(c);
        }
        private bool IsHyphenChar(char c)
        {
            return c == '-';
        }



        private void SetToken(WordTokenType type)
        {
            _text = null;
            _type = type;
            _current = new WordToken() { Text = null, Type = type };
        }
        private void SetToken(string text, WordTokenType type)
        {
            _text = text;
            _type = type;
            _current = new WordToken() { Text = text, Type = type };
        }

        public void Dispose()
        {
            this._reader.Dispose();
        }
    }
}
