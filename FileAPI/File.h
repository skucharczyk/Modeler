#ifndef FILEAPI_H
#define FILEAPI_H

#include <vector>
#include <string>
#include <fstream>
#include <sstream>

using namespace std;

void ReadFileLines(string file, vector<string>& output);
void SaveFileLines(string file, vector<string>& text);
std::string GetAttribute(std::string& text, unsigned partNum);
void GetAttributes(std::string& text, std::vector<std::string>& outParts);
string CutFirstString(string line);
bool IsLineEmpty(string line);
bool IsWhiteSpace(char c);

template<typename T> 
inline T StringToNum(std::string string)
{
	T num;
	stringstream ss;
	ss << string;
	ss >> num;
	return num;
}

#endif