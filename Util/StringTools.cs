﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HlidacStatu.Util
{
    public static class StringTools
    {


        public static List<Tuple<string, bool>> SplitStringToPartsWithQuotes(string query, char quoteDelimiter)
        {
            //split newquery into part based on ", mark "xyz" parts
            //string , bool = true ...> part withint ""
            List<Tuple<string, bool>> textParts = new List<Tuple<string, bool>>();
            int[] found = CharacterPositionsInString(query, quoteDelimiter);
            if (found.Length > 0 && found.Length % 2 == 0)
            {
                int start = 0;
                bool withIn = false;
                foreach (var idx in found)
                {
                    int startIdx = start;
                    int endIdx = idx;
                    if (withIn)
                        endIdx++;
                    textParts.Add(new Tuple<string, bool>(query.Substring(startIdx, endIdx - startIdx), withIn));
                    start = endIdx;
                    withIn = !withIn;
                }
                if (start < query.Length)
                    textParts.Add(new Tuple<string, bool>(query.Substring(start), withIn));
            }
            else
            {
                textParts.Add(new Tuple<string, bool>(query, false));
            }
            return textParts;
        }

        public static int[] CharacterPositionsInString(string text, char lookingFor)
        {
            if (string.IsNullOrEmpty(text))
                return new int[] { };

            char[] textarray = text.ToCharArray();
            List<int> found = new List<int>();
            for (int i = 0; i < text.Length; i++)
            {
                if (textarray[i].Equals(lookingFor))
                {
                    found.Add(i);
                }
            }
            return found.ToArray();
        }



        public static string[] SplitWithSeparators(this string s, char[] separators, StringSplitOptions splitOptions = StringSplitOptions.None)
        {
            return SplitWithSeparators(s, separators.Select(m => m.ToString()).ToArray(), splitOptions);
        }

        public static string[] SplitWithSeparators(this string s, string[] separators,
            StringSplitOptions splitOptions = StringSplitOptions.None,
            StringComparison comparisonType = StringComparison.Ordinal)
        {
            if (string.IsNullOrEmpty(s))
                return new string[] { };

            List<string> parts = new List<string>();
            int prevStart = 0;
            for (int i = 0; i < s.Length; i++)
            {
                foreach (var sep in separators)
                {
                    if (i + sep.Length <= s.Length)
                    {
                        if (s.Substring(i, sep.Length).Equals(sep, comparisonType))
                        {
                            int partLen = i - prevStart;
                            if (partLen > 0)
                                parts.Add(s.Substring(prevStart, partLen));
                            parts.Add(sep);
                            i = i + sep.Length;
                            prevStart = i;
                            break;
                        }
                    }
                }
            }
            if (prevStart < s.Length)
                parts.Add(s.Substring(prevStart));

            if (splitOptions == StringSplitOptions.RemoveEmptyEntries)
                return parts.Where(m => !string.IsNullOrEmpty(m)).ToArray();

            return parts.ToArray();
        }

    }
}
