#include "Attrib.h"

namespace raytracer {

	Attrib::Attrib(void)
	{
	}

	Attrib::Attrib(float _kd, float _ks, float _wg, float _ka, float _r, float _g, float _b, float _kt, float _eta, float _kr) :
	kdcR(_kd), kscR(_ks), wg(_wg), kdcG(_ka), r(_r), g(_g), b(_b), krcR(_kt), eta(_eta), kr(_kr)
	{
	}


	Attrib::~Attrib(void)
	{
	}
}