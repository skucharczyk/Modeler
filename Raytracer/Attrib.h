#pragma once
#include <iostream>

//////////////////////////////////////////////////////////////////////////
// Współczynniki oznaczają:
// kd  - współczynnik rozpraszania powierzchni
// ks  - współczynnik odbicia lustrzanego
// wg  - współczynnik połyskliwości
// ka  - współczynnik rozpraszania światła otaczającego
// r   - składowa czerowna
// g   - składowa zielona
// b   - składowa niebieska
// kt  - współczynnik przeźroczystości
// eta - stosunek prędkości światla w próżni do prędkości światła w danym ośrodku
// kr  -
//////////////////////////////////////////////////////////////////////////
namespace raytracer {
	class Attrib
	{
	public:
		float kdcR, kdcG, kdcB;
		float kscR, kscG, kscB;
		float krcR, krcG, krcB;
		float kacR, kacG, kacB;
		float r, g, b;
		float wg, eta;
		float kr;
		Attrib(float, float, float, float, float, float, float, float, float, float);
		Attrib(void);
		~Attrib(void);/*
		friend std::ostream& operator<<(std::ostream& os, const Attrib& a) 
		{
			return os << a.kd << " " << a.ks << " " << a.wg << " " << a.ka << "\n"
				<< a.r << " " << a.g << " " << a.b << "\n"
				<< a.kt << " " << a.eta << " " << a.kr << "\n";
		};*/
	};
}
