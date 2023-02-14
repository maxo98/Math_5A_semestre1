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
	std::vector<std::pair<std::vector<float>, std::vector<float>>> vertices;
};

extern "C"
{
	DLL_EXPORT DataSet* InitDataSet(int numberBones);
	DLL_EXPORT void SetNewVertex(DataSet* dataset, float* position, int length, bool* linkedBones, int lengthLink);
	DLL_EXPORT void DeleteInstance(void* instance);
	DLL_EXPORT void DeleteArrayInstance(void* instance);
	DLL_EXPORT void DeleteInstance(void* instance);
	DLL_EXPORT void ApplyBackProp(Genome* gen, NeuralNetwork* network);
	DLL_EXPORT void SaveGenome(Genome* gen);
	DLL_EXPORT Genome* CreateGenome(int input, int output, int layer, int node);
	DLL_EXPORT NeuralNetwork* CreateNeuralNetwork(Genome* gen);
	DLL_EXPORT void Train(DataSet* dataset, NeuralNetwork* network, int epoch, float lr);
}

void SetNewVertex(DataSet* dataset, float* position, int length, bool* linkedBones, int lengthLink)
{
	std::vector<float> lPosition = wrapperArrayToVector<float>(position, length);
	std::vector<bool> lLinkedBones = wrapperArrayToVector<bool>(linkedBones, lengthLink);

	std::pair<std::vector<float>, std::vector<float>> newPair;// = std::make_pair(lPosition, lLinkedBones);

	for (int i = 0; i < lPosition.size(); i++)
	{
		newPair.first.push_back(lPosition[i]);
	}

	for (int i = 0; i < lPosition.size(); i++)
	{
		newPair.second.push_back(lLinkedBones[i] ? 1.0f : 0.0f);
	}

	dataset->vertices.push_back(newPair);
}

void Train(DataSet* dataset, NeuralNetwork* network, int epoch, float lr)
{
	for (int i = 0; i < epoch; i++)
	{
		int index = randInt(0, dataset->vertices.size() - 1);

		network->backprop(dataset->vertices[index].first, dataset->vertices[index].second, lr);
	}
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

void ApplyBackProp(Genome* gen, NeuralNetwork* network)
{
	network->applyBackprop(*gen);
}

void SaveGenome(Genome* gen)
{
	gen->saveCurrentGenome();
}

Genome* CreateGenome(int input, int output, int layer, int node)
{
	Activation* tanh = new TanhActivation();

	std::vector<Activation*> arrActiv;
	arrActiv.push_back(tanh);

	std::unordered_map<std::pair<unsigned int, unsigned int>, unsigned int> allConn;

	Genome* gen = new Genome(input, output, arrActiv);

	gen->fullyConnect(layer, node, tanh, tanh, allConn, xavierUniformInit, time(NULL));

	return gen;
}

NeuralNetwork* CreateNeuralNetwork(Genome* gen)
{
	NeuralNetwork* network = new NeuralNetwork();

	Neat::genomeToNetwork(*gen, *network);

	return network;
}

void DeleteInstance(void* instance)
{
	delete instance;
}

void DeleteArrayInstance(void* instance)
{
	delete[] instance;
}
