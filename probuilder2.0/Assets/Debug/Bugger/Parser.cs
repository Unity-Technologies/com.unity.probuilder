using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/**
 * Part of lwjson library
 * Written by LLLF
 * Released to the public domain
 */
// namespace Engine.JSON
// {
    class Parser
    {
        /*
         * sample json
         * { "string" : value, ... } - object
         * value can be
         *      - [] an array
         *      - {} an object
         *      - string, int, true, false, null
         * [ value, ... ] - array
         *
         */

        private String json;
        private int index;

        private enum TOKEN
        {
            NONE, //nothing is ahead
            OPENING_BRACE, // {
            CLOSING_BRACE, // }
            OPENING_BRACKET, // [
            CLOSING_BRACKET, // ]
            COMMA, // ,
            COLON, // :
            OTHER //values
        }

        private char[] Tokens = new char[] { '{', '}', '[', ']', ',', ':' };

        public Parser(String json)
        {
            this.json = System.Text.RegularExpressions.Regex.Replace(json, @"\s", "");
        }

        public Dictionary<String, Object> Parse()
        {
            return ParseObject();
        }

        private Dictionary<String, Object> ParseObject()
        {
            Dictionary<String, Object> dict = new Dictionary<String, Object>();

            //read the first token {
            Expect(TOKEN.OPENING_BRACE);

            while (true)
            {
                switch (PeekNextToken())
                {
                    //we now only accept either } or a string
                    case TOKEN.CLOSING_BRACE:
                        index++;
                        return dict;
                    case TOKEN.COMMA:
                        index++; //move foward
                        continue;
                    default:
                        //it's a key pair
                        KeyValuePair<String, Object> pair = ParseDataPair();
                        dict.Add(pair.Key, pair.Value);
                        
                        //the problem here is that it doesnt neccessarly mean there there will be a comma here
                        //it could be the end of the object and no comma is present
                        //Expect(TOKEN.COMMA);
                        continue;
                }
            }

        }

        private void Expect(TOKEN token)
        {
            if (PeekNextToken() != token) throw new Exception(token + " expected");
            index++; //move index
        }

        private void Expect(char c)
        {
            if (json[index++] != c)
                throw new Exception(c + " expected");
        }

        private bool IsEnd()
        {
            return (index == json.Length);
        }

        private TOKEN PeekNextToken()
        {
            if (IsEnd())
                return TOKEN.NONE;

            //look at the next token
            int tokenPosition = json.IndexOfAny(Tokens, index);
            if (tokenPosition == -1)
            {
                return TOKEN.OTHER;
            }
            else
            {
                return GetTokenFromChar(json[index]);
            }
        }

        private char PeekChar()
        {
            return json[index + 1];
        }

        private char CurrentChar()
        {
            return json[index];
        }

        private TOKEN GetTokenFromChar(char c)
        {
            switch (c)
            {
                case '{':
                    return TOKEN.OPENING_BRACE;
                case '}':
                    return TOKEN.CLOSING_BRACE;
                case '[':
                    return TOKEN.OPENING_BRACKET;
                case ']':
                    return TOKEN.CLOSING_BRACKET;
                case ',':
                    return TOKEN.COMMA;
                case ':':
                    return TOKEN.COLON;
                default:
                    return TOKEN.OTHER;
            }
        }

        private KeyValuePair<String, Object> ParseDataPair()
        {
            //we are expecting a string
            String key = ParseString();
            Expect(TOKEN.COLON);
            Object value = ParseValue();
            
            return new KeyValuePair<String,Object>(key, value);
        }

        private String ParseString()
        {
            //expect a "
            Expect('"');
            StringBuilder sb = new StringBuilder();
            for (; index < json.Length; index++)
            {
                if (json[index - 1] != '\\' && json[index] == '"')
                {
                    //finish
                    break;
                }
                else
                {
                    sb.Append(json[index]);
                }
            }

            Expect('"');

            return sb.ToString();
        }

        private Object ParseValue()
        {
            /*
             * We are expecting one of these
             *      - String - "
             *      - Number - check this as default case
             *      - Object - {
             *      - Array - [
             *      - True, false, null
             */
            switch (PeekNextToken())
            {
                case TOKEN.OTHER: //either ", number, true, false, null
                    char c = CurrentChar();
                    if (c == '"')
                    {
                        return ParseString();
                    }

                    if (c == 't' && json[index + 1] == 'r' && json[index + 2] == 'u' && json[index + 3] == 'e')
                    {
                        index += 4;
                        return true;
                    }
                    else if (c == 'f' && json[index + 1] == 'a' && json[index + 2] == 'l' && json[index + 3] == 's' && json[index + 4] == 'e')
                    {
                        index += 5;
                        return false;
                    }
                    else if (c == 'n' && json[index + 1] == 'u' && json[index + 2] == 'l' && json[index + 4] == 'l')
                    {
                        index += 4;
                        return null;
                    }
                    else
                    {
                        //try and read a number
                        return ParseNumber();
                    }
                case TOKEN.OPENING_BRACE:
                    return ParseObject();
                case TOKEN.OPENING_BRACKET:
                    return ParseArray();
                default:
                    throw new Exception("Expecting a value type");
            }

        }

        private List<Object> ParseArray()
        {
            Expect(TOKEN.OPENING_BRACKET);

            List<Object> list = new List<Object>();

            while (true)
            {
                switch (PeekNextToken())
                {
                    case TOKEN.CLOSING_BRACKET:
                        //and we are done
                        index++;
                        return list;
                    case TOKEN.COMMA:
                        index++;
                        continue;
                    default:
                        //read a data element
                        list.Add(ParseValue());
                        continue;
                }
            }
        }

        private double ParseNumber()
        {
            //keep reading until we hit , ] or }
            int end = json.IndexOfAny(new char[] { ',', ']', '}' }, index);
            String number = json.Substring(index, end - index);

            index += number.Length;
            return Double.Parse(number);
        }
    }
// }