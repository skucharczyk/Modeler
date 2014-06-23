#include "File.h"

void ReadFileLines(string file, vector<string>& output)
{
	vector<string> text;

	std::ifstream fileObj(file.c_str());

	std::string line;
	while(getline(fileObj, line))
	{
		text.push_back(line);
	}

	bool commStart = false;

	for(unsigned i = 0; i < text.size(); ++i)
    {
        if(IsLineEmpty(text[i]) == false)
        {
            vector<bool> commLines;

            for(unsigned j = 0; j < text[i].length() - 1; ++j)
            {
                if(commStart == false)
                {
                    if(text[i][j] == '/' && text[i][j + 1] == '/')
                    {
                        string strLine = string(text[i], 0, j);
                        if(strLine.length() > 0)
                        {
							output.push_back(strLine);
                        }

                        commLines.clear();
                        for(unsigned k = 0; k < text[i].length(); ++k)
                        {
							commLines.push_back(true);
                        }

                        break;
                    }
                    else if(text[i][j] == '/' && text[i][j + 1] == '*')
                    {
                        commStart = true;
						commLines.push_back(true);
						commLines.push_back(true);

                        ++j;
                    }
                    else
                    {
						commLines.push_back(false);
                    }
                }
                else
                {
                    if(text[i][j] == '*' && text[i][j + 1] == '/')
                    {
						commLines.push_back(true);
						commLines.push_back(true);
                        commStart = false;

                        ++j;
                    }
                    else
                    {
						commLines.push_back(true);
                    }
                }
            }

			if(commLines.size() < text[i].length())
            {
				commLines.push_back(commStart);
            }

            string line;

            for(unsigned j = 0; j < text[i].length(); ++j)
            {
                if(commLines[j] == false)
                {
					line.push_back(text[i][j]);
                }
            }

            if(line.length() > 0)
            {
				output.push_back(line);
            }
        }
    }
}

std::string GetAttribute(std::string& text, unsigned partNum)
{
	std::string part;
	unsigned i = 0;
	
	while(i < text.length() && part.empty())
	{
		while(i < text.length() && (IsWhiteSpace(text[i])) == true)
		{
			++i;
		}
		unsigned start = i;
		while(i < text.length() && (IsWhiteSpace(text[i]) == false))
		{
			++i;
		}
		if(partNum == 0)
		{
			part = std::string(text, start, i-start);
		}
		else
		{
			--partNum;
		}
	}

	return part;
}

void GetAttributes(std::string& text, std::vector<std::string>& outParts)
{
    unsigned i = 0;
	std::string att;

    while(i < text.length())
    {
        while(i < text.length() && (IsWhiteSpace(text[i])) == true)
        {
            ++i;
        }
        int start = i;
        while(i < text.length() && (IsWhiteSpace(text[i]) == false))
        {
            ++i;
        }

		att = std::string(text, start, i - start);

		outParts.push_back(att);
        att = "";
    }
}

void SaveFileLines(string file, vector<string>& text)
{
	ofstream fileBuffer;
	fileBuffer.open(file);

	for (unsigned i = 0; i < text.size(); ++i) {
		fileBuffer << text[i] << "\n";
	}

	fileBuffer.close();
}

string CutFirstString(string line)
{
    unsigned i = 0;
    string att = "";

	if(line.length() > 0)
    {
        while(i < line.length() && (IsWhiteSpace(line[i]) == true))
        {
            ++i;
        }
        while(i < line.length() && (IsWhiteSpace(line[i]) == false))
        {
            ++i;
        }
        while(i < line.length() && (IsWhiteSpace(line[i]) == true))
        {
            ++i;
        }

        att = string(line, i, line.length() - i);
    }

    return att;
}

bool IsLineEmpty(string line)
{
	for(unsigned i = 0; i < line.length(); ++i)
    {
        if(IsWhiteSpace(line[i]) == false)
        {
            return false;
        }
    }

    return true;
}

bool IsWhiteSpace(char c)
{
	return c == ' ' || c == '\t' || c == '\n' || c == '\v' || c == '\f' || c == '\r';
}