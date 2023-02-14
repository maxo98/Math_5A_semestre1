#include "pch.h"

#include "Neat.h"
#include "Hyperneat.h"

extern "C"
{
	NeatParameters* CreateNeatParamInstance();
	HyperneatParameters* CreateHyperNeatParamInstance();
	Hyperneat* CreateHyperNeatInstance(unsigned int popSize, NeatParameters* neatParams, HyperneatParameters* hyperneatParams);
	void ApplyBackProp(Hyperneat* instance);
	void DeleteInstance(void* instance);
}

void ApplyBackProp(Hyperneat* instance)
{
	instance->applyBackprop();
}

HyperneatParameters* CreateHyperNeatParamInstance()
{
	HyperneatParameters* newHyperneatParameters = new HyperneatParameters();

	return newHyperneatParameters;
}

Hyperneat* CreateHyperNeatInstance(unsigned int popSize, NeatParameters* neatParams, HyperneatParameters* hyperneatParams)
{
	return new Hyperneat(popSize, *neatParams, *hyperneatParams, Neat::INIT::ONE);
}

NeatParameters* CreateNeatParamInstance()
{
	NeatParameters* newNeatParameters = new NeatParameters();

	return newNeatParameters;
}

void DeleteInstance(void* instance)
{
	delete instance;
}