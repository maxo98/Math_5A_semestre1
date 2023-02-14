#include "pch.h"

#include "Neat.h"
#include "Hyperneat.h"

#include <vector>
#include <map>

#define DLL_EXPORT __declspec(dllexport)

template<typename type>
std::vector<type> wrapperArrayToVector(const type* data, const size_t length)
{
	std::vector<type> newVector = std::vector<type>(length);

	for (int i = 0; i < length; i++)
	{
		newVector[i] = data[i];
	}

	return newVector;
}

struct DataSet {
	int bones;
	std::vector<std::pair<std::vector<float>, std::vector<bool>>> vertices;
};

extern "C"
{
	DLL_EXPORT NeatParameters* CreateNeatParamInstance();
	DLL_EXPORT HyperneatParameters* CreateHyperNeatParamInstance();
	DLL_EXPORT Hyperneat* CreateHyperNeatInstance(unsigned int popSize, NeatParameters* neatParams, HyperneatParameters* hyperneatParams);
	DLL_EXPORT DataSet* InitDataSet(int numberBones);
	DLL_EXPORT void SetNewVertex(DataSet* dataset, float* position, int length, bool* linkedBones, int lengthLink);
	DLL_EXPORT void ApplyBackProp(Hyperneat* instance);
	DLL_EXPORT void DeleteInstance(void* instance);
	DLL_EXPORT void DeleteArrayInstance(void* instance);
}

void SetNewVertex(DataSet* dataset, float* position, int length, bool* linkedBones, int lengthLink)
{
	std::vector<float> lPosition = wrapperArrayToVector<float>(position, length);
	std::vector<bool> lLinkedBones = wrapperArrayToVector<bool>(linkedBones, lengthLink);

	std::pair<std::vector<float>, std::vector<bool>> newPair = std::make_pair(lPosition, lLinkedBones);
	dataset->vertices.push_back(newPair);
}

void SetBones(DataSet* dataset, int sizeBones)
{
	dataset->bones = sizeBones;
}

DataSet* InitDataSet(int numberBones)
{
	DataSet* newDataSet = new DataSet();
	newDataSet->bones = numberBones;

	return newDataSet;
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

void DeleteArrayInstance(void* instance)
{
	delete[] instance;
}
