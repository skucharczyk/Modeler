using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Modeler.FileSystem
{
    static class File
    {
        public static List<string> ReadFileLines(string file)
        {
            string[] text = System.IO.File.ReadAllLines(file);

            bool commStart = false;

            List<string> output = new List<string>();

            for(int i = 0; i < text.Length; ++i)
            {
                if(IsLineEmpty(text[i]) == false)
                {
                    List<bool> commLines = new List<bool>();

                    for(int j = 0; j < text[i].Length - 1; ++j)
                    {
                        if(commStart == false)
                        {
                            if(text[i][j] == '/' && text[i][j + 1] == '/')
                            {
                                string strLine = new string(text[i].ToCharArray(), 0, j);
                                if(strLine.Length > 0)
                                {
                                    output.Add(strLine);
                                }

                                commLines.Clear();
                                for(int k = 0; k < text[i].Length; ++k)
                                {
                                    commLines.Add(true);
                                }

                                break;
                            }
                            else if(text[i][j] == '/' && text[i][j + 1] == '*')
                            {
                                commStart = true;
                                commLines.Add(true);
                                commLines.Add(true);

                                ++j;
                            }
                            else
                            {
                                commLines.Add(false);
                            }
                        }
                        else
                        {
                            if(text[i][j] == '*' && text[i][j + 1] == '/')
                            {
                                commLines.Add(true);
                                commLines.Add(true);
                                commStart = false;

                                ++j;
                            }
                            else
                            {
                                commLines.Add(true);
                            }
                        }
                    }

                    if(commLines.Count < text[i].Length)
                    {
                        commLines.Add(commStart);
                    }

                    StringBuilder line = new StringBuilder();

                    for(int j = 0; j < text[i].Length; ++j)
                    {
                        if(commLines[j] == false)
                        {
                            line.Append(text[i][j]);
                        }
                    }

                    if(line.Length > 0)
                    {
                        output.Add(line.ToString());
                    }
                }
            }

            return output;
        }

        public static void SaveFileLines(string file, List<string> text)
        {
            string[] toWrite = new string[text.Count];
            text.CopyTo(toWrite);

            System.IO.File.WriteAllLines(file, toWrite);
        }

        public static string GetAttribute(string line, uint position)
        {
            char[] chars = line.ToCharArray();

            string att = "";
	        int i = 0;
	
	        while(i < line.Length && att.Length == 0)
	        {
		        while(i < line.Length && (Char.IsWhiteSpace(line[i]) == true))
		        {
			        ++i;
		        }
		        int start = i;
                while(i < line.Length && (Char.IsWhiteSpace(line[i]) == false))
		        {
			        ++i;
		        }
		        if(position == 0)
		        {
                    att = new string(chars, start, i - start);
		        }
		        else
		        {
			        --position;
		        }
	        }

	        return att;
        }

        public static string[] GetAttributes(string line)
        {
            List<string> atts = new List<string>();
            char[] chars = line.ToCharArray();

            int i = 0;
            string att = "";

            while(i < line.Length)
            {
                while(i < line.Length && (Char.IsWhiteSpace(line[i]) == true))
                {
                    ++i;
                }
                int start = i;
                while(i < line.Length && (Char.IsWhiteSpace(line[i]) == false))
                {
                    ++i;
                }

                att = new string(chars, start, i - start);

                atts.Add(att);
                att = "";
            }

            return atts.ToArray();
        }

        public static string CutFirstString(string line)
        {
            int i = 0;
            string att = "";

            if(line.Length > 0)
            {
                while(i < line.Length && (Char.IsWhiteSpace(line[i]) == true))
                {
                    ++i;
                }
                while(i < line.Length && (Char.IsWhiteSpace(line[i]) == false))
                {
                    ++i;
                }
                while(i < line.Length && (Char.IsWhiteSpace(line[i]) == true))
                {
                    ++i;
                }

                att = new string(line.ToCharArray(), i, line.Length - i);
            }

            return att;
        }

        private static bool IsLineEmpty(string line)
        {
            for(int i = 0; i < line.Length; ++i)
            {
                if(Char.IsWhiteSpace(line[i]) == false)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
