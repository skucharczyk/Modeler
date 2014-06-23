#include <iostream>
#include <Windows.h>
#include <WindowsX.h>
#include <stdio.h>
#include "resource.h"
#include "Scene.h"

using namespace std;
using namespace raytracer;

HWND hWnd;
HDC hDC;
Scene *scene = NULL;
BITMAPINFOHEADER *bminfoheader;
BITMAPINFO *bminfo;
BYTE *buffer;
long buffer_size;
int xres, yres;

LRESULT CALLBACK WindowProc(HWND hWnd,
							UINT message,
							WPARAM wParam,
							LPARAM lParam);


int WINAPI WinMain(HINSTANCE hInstance,
				   HINSTANCE hPrevInstance,
				   LPSTR liCmdLine,
				   int nShowCmd)
{
	// struktura trzyma informacje dotyczace klasy okna
	WNDCLASSEX wc;

	// Szybkie zerowanie pamieci
	ZeroMemory(&wc, sizeof(WNDCLASSEX));

	// Informacje dotycz¹ce okna
	wc.cbSize = sizeof(WNDCLASSEX);
	wc.style = CS_OWNDC | CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = WindowProc;
	wc.hInstance = hInstance;
	wc.hCursor = LoadCursor(NULL, IDC_ARROW);
	wc.lpszClassName = "WindowClass";
	wc.lpszMenuName = MAKEINTRESOURCE(IDR_MAINFRAME);

	// Rejestracja klasy okna
	RegisterClassEx(&wc);
	
	string strBrs, strCam, strLgt, strAtr, strVer, strTmp, strPth, fileName;

	strBrs = "..\\Raytracer\\Sceny\\ulica\\ulica.brs";
	strCam = "..\\Raytracer\\Sceny\\ulica\\v1\\v1.cam";
	strLgt = "..\\Raytracer\\Sceny\\ulica\\v1\\v1.lgt";
	strAtr = "..\\Raytracer\\Sceny\\ulica\\v1\\v1.atr";
	strPth = "C:\\Users\\Lycha\\Documents\\Visual Studio 2010\\Projects\\Modeler\\Release\\scena-sw-lampa.scn";
	strPth = liCmdLine;
	scene = new Scene();
	//scene->readGeometry(strBrs);
	//scene->readCamera(strCam);
	//scene->readLighting(strLgt);
	//scene->readAttributesAlt(strAtr);
	//bool rendered = false;
	bool rendered = !scene->loadFromSceneLoader(strPth);
	if (!rendered) {
		rendered = scene->trianSize < 1;
	}
	remove(liCmdLine);
	xres = scene->camera.xResolution;
	yres = scene->camera.yResolution;

	bminfoheader = (BITMAPINFOHEADER *)malloc(sizeof(BITMAPINFOHEADER));
	memset(bminfoheader, 0, sizeof(BITMAPINFOHEADER));
	bminfoheader->biSize = sizeof(BITMAPINFOHEADER);
	bminfoheader->biWidth = xres;
	bminfoheader->biHeight = yres;
	bminfoheader->biPlanes = 1;
	bminfoheader->biBitCount = 24;
	bminfoheader->biCompression = BI_RGB;
	bminfoheader->biSizeImage = 0;
	bminfoheader->biXPelsPerMeter = 0x0ec4;
	bminfoheader->biYPelsPerMeter = 0x0ec4;     
	bminfoheader->biClrUsed = 0;
	bminfoheader->biClrImportant = 0;
	bminfo = (BITMAPINFO *)malloc(sizeof(BITMAPINFO));
	memset(bminfo, 0, sizeof(BITMAPINFO));
	bminfo->bmiHeader = *bminfoheader;

	int padding = 0;
	int scanlinebytes = 3 * xres;
	while ((scanlinebytes+padding)%4 != 0) padding++;
	int psw = scanlinebytes + padding;
	buffer_size = yres * psw;
	
	buffer = new BYTE[buffer_size];
	memset(buffer, 0, buffer_size);

	int upPointX;
	int upPointY;

	upPointX = GetSystemMetrics(SM_CXFULLSCREEN);
	upPointY = GetSystemMetrics(SM_CYFULLSCREEN);

	upPointX /= 2;
	upPointY /= 2;

	upPointX -= xres / 2;
	upPointY -= yres / 2;

	hWnd = CreateWindowEx(NULL,
						  "WindowClass",
						  "Raytracer",
						  WS_OVERLAPPEDWINDOW,
						  upPointX, upPointY,
						  xres+18, yres+59,
						  NULL,
						  NULL,
						  hInstance,
						  NULL);
	// Wyswietl okno
	ShowWindow(hWnd, nShowCmd);

	hDC = GetDC(hWnd);

	MSG msg;
	bool initialized = rendered;
	while (TRUE) {
		if (!initialized) {
			SendMessage(hWnd, WM_PAINT, 0, 0);
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
			initialized = true;
			scene->voxelize();
		}
		while (!rendered) {
			rendered = scene->renderTile(buffer, psw);
			HACCEL haccel = 0;
			InvalidateRect(hWnd,NULL,TRUE);
			SendMessage(hWnd, WM_PAINT, 0, 0);
			while (PeekMessage(&msg, NULL, 0, 0, PM_REMOVE)) {
				TranslateMessage(&msg);
				DispatchMessage(&msg);
			}
		}
		while (GetMessage(&msg, NULL, 0, 0)) {
			TranslateMessage(&msg);
			DispatchMessage(&msg);
		}
		/*if (msg.message == WM_QUIT) {
			break;
		}*/
	}

	return msg.wParam;
}

LRESULT CALLBACK WindowProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	switch(message)
	{
	case WM_PAINT:		
		if (!buffer) break;
		StretchDIBits(hDC, 0, 0, xres, yres, 0, 0, xres, yres, buffer, bminfo, DIB_RGB_COLORS, SRCCOPY );
		ValidateRect(hWnd, NULL);
		break;
	case WM_CLOSE:
		ReleaseDC(hWnd, hDC);
		DestroyWindow(hWnd);
		ExitProcess(0);
		break;
	case WM_COMMAND:
		switch (LOWORD(wParam))
		{
		case IDM_PLIK_ZAPISZ:
			OPENFILENAME saveFileDialog;
			char saveFileName[500];
			memset(saveFileName, 0, 500);
			ZeroMemory(&saveFileDialog, sizeof(OPENFILENAME));
			saveFileDialog.lStructSize = sizeof(OPENFILENAME);
			saveFileDialog.hwndOwner = hWnd;
			saveFileDialog.lpstrFile = saveFileName;
			saveFileDialog.lpstrFilter = "Bitmap file (*.bmp)\0*.bmp\0";
			saveFileDialog.nMaxFile = 500;
			saveFileDialog.Flags = OFN_EXPLORER | OFN_PATHMUSTEXIST | OFN_HIDEREADONLY |OFN_OVERWRITEPROMPT;
			saveFileDialog.lpstrDefExt = "bmp";

			if (GetSaveFileName(&saveFileDialog)) {
				Scene::saveBMP(buffer, xres, yres, buffer_size, saveFileName);
			}			
			break;
		case ID_PLIK_ZAKONCZ:
			SendMessage(hWnd, WM_CLOSE, 0, 0);
			break;
		}
	case WM_DESTROY:
		{
			PostQuitMessage(0);
			return 0;
		} break;
	}

	return DefWindowProc(hWnd, message, wParam, lParam);
}

//int main (int argc, char * const argv[]) {
//	string strBrs, strCam, strLgt, strAtr, strVer, strTmp, strPth, fileName;
//	string tn;
//	bool dof = false;
//	float f = 0., z_f = 0;
//	int a;
//	//cout << "Podaj sciezke do folderu zawierajacego plik *.brs:\n";
//	//getline(cin, strPth);
//	//cout << "Podaj nazwe pliku *.brs (bez rozszerzenia) oraz wersje sceny (np. v2) do wyrenderowania:\n";
//	//cin >> strTmp;
//	//cin >> strVer;
//
//	//strBrs = strPth;
//	//strBrs.append("\\\\").append(strTmp);
//	//strBrs.append(".brs");
//	//strCam = strPth;
//	//strCam.append("\\\\").append(strVer).append("\\\\").append(strVer).append(".cam");
//	//strLgt = strPth;
//	//strLgt.append("\\\\").append(strVer).append("\\\\").append(strVer).append(".lgt");
//	//strAtr = strPth;
//	//strAtr.append("\\\\").append(strVer).append("\\\\").append(strVer).append(".atr");
//
//	//cout << "Dodac efekt glebi ostrosci (t/n):\n";
//	//cin >> tn;
//
//	//dof = tn=="t";
//
//	//if (dof) {
//	//	cout << "Podaj wartosc f (ogniskowa) oraz z_f (odleglosc do punktu, w ktorym obraz ma byc ostry) dla efektu DoF:\n";
//	//	cin >> f;
//	//	cin >> z_f;
//	//}
//
//	strBrs = "C:\\Users\\Marcin\\Documents\\Visual Studio 2010\\Projects\\Modeler\\trunk\\Raytracer\\Sceny\\temple\\temple.brs";
//	strCam = "C:\\Users\\Marcin\\Documents\\Visual Studio 2010\\Projects\\Modeler\\trunk\\Raytracer\\Sceny\\temple\\v4\\v4.cam";
//	strLgt = "C:\\Users\\Marcin\\Documents\\Visual Studio 2010\\Projects\\Modeler\\trunk\\Raytracer\\Sceny\\temple\\v4\\v4.lgt";
//	strAtr = "C:\\Users\\Marcin\\Documents\\Visual Studio 2010\\Projects\\Modeler\\trunk\\Raytracer\\Sceny\\temple\\v4\\v4.atr";
//	
//	Scene scena = Scene();
//
//	//cin >> a;
//
//	scena.readGeometry(strBrs);
//	scena.readCamera(strCam);
//	scena.readLighting(strLgt);
//	scena.readAttributesAlt(strAtr);
//
//	scena.saveWithNewFormat("temple-v4.scn");
//
//	//cout << "Zakonczono wczytywanie\n";
//	//cout << "Rozpoczeto wokselizacje\n";
//	//scena.voxelize();
//	//cout << "Wokselizacja zakonczona\n";
//	//cout << "Rozpoczeto renderowanie\n";
//	////fileName = strTmp.append("-").append(strVer).append(".bmp");
//	//fileName = "wynik.bmp";
//	//scena.render(fileName, dof, f, z_f);
//
//	//cin >> a;
//
//    return 0;
//}
