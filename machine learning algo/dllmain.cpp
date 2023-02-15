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
	DLL_EXPORT void SetNewVertex(DataSet* dataset, float* position, int length, int* linkedBones, int lengthLink);
	DLL_EXPORT void DeleteInstance(void* instance);
	DLL_EXPORT void DeleteArrayInstance(void* instance);
	DLL_EXPORT void DeleteInstance(void* instance);
	DLL_EXPORT void ApplyBackProp(Genome* gen, NeuralNetwork* network);
	DLL_EXPORT void SaveGenome(Genome* gen);
	DLL_EXPORT bool SetCompute(DataSet* dataset, NeuralNetwork* network, int idx);
	DLL_EXPORT float* GetVerticesBones(DataSet* dataset, int idx);
	DLL_EXPORT float* GetVertice(DataSet* dataset, int idx);
	DLL_EXPORT int GetInputFromGenome(Genome* genome);
	DLL_EXPORT int GetOutputFromGenome(Genome* genome);
	DLL_EXPORT float* GetLinkedBones(DataSet* dataset, int idx);
	DLL_EXPORT Genome* CreateGenome(int input, int output, int layer, int node);
	DLL_EXPORT NeuralNetwork* CreateNeuralNetwork(Genome* gen);
	DLL_EXPORT bool Train(DataSet* dataset, NeuralNetwork* network, int epoch, float lr);
}

float* GetVertice(DataSet* dataset, int idx)
{
	return dataset->vertices[idx].first.data();
}

float* GetVerticesBones(DataSet* dataset, int idx)
{
	return dataset->vertices[idx].second.data();
}

bool SetCompute(DataSet* dataset, NeuralNetwork* network, int idx)
{
	auto data = dataset->vertices[idx];
	std::vector<float> output;
	network->compute(dataset->vertices[idx].first, output);
	bool correct = true;

	for (int cpt = 0; cpt < output.size(); cpt++)
	{
		if (data.second[cpt] == 1.f)
		{
			if (output[cpt] <= 0)
			{
				correct = false;
			}
		}
		else {
			if (output[cpt] > 0)
			{
				correct = false;
			}
		}
	}

	return correct;
}

float* GetLinkedBones(DataSet* dataset, int idx)
{
	return dataset->vertices[idx].second.data();
}

void SetNewVertex(DataSet* dataset, float* position, int length, int* linkedBones, int lengthLink)
{
	std::pair<std::vector<float>, std::vector<float>> newPair;// = std::make_pair(lPosition, lLinkedBones);

	newPair.first.reserve(length);
	newPair.second.reserve(lengthLink);

	for (int i = 0; i < length; i++)
	{
		newPair.first.push_back(position[i]);
	}

	newPair.first.push_back(0.5f);

	for (int i = 0; i < lengthLink; i++)
	{
		newPair.second.push_back(linkedBones[i] ? 1.0f : -1.0f);
	}

	dataset->vertices.push_back(newPair);
}

bool Train(DataSet* dataset, NeuralNetwork* network, int epoch, float lr)
{
	for (int i = 0; i < epoch; i++)
	{
		int index = randInt(0, dataset->vertices.size() - 1);

		if (network->backprop(dataset->vertices[index].first, dataset->vertices[index].second, lr) == false)
		{
			return false;
		}
	}

	return true;
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
	auto seed = time(NULL);
	srand(seed);

	Activation* tanh = new TanhActivation();

	std::vector<Activation*> arrActiv;
	arrActiv.push_back(tanh);

	std::unordered_map<std::pair<unsigned int, unsigned int>, unsigned int> allConn;

	Genome* gen = new Genome(input, output, arrActiv);

	gen->fullyConnect(layer, node, tanh, tanh, allConn, xavierUniformInit, time(NULL));

	return gen;
}

int GetOutputFromGenome(Genome* genome)
{
	return genome->getOutput();
}

int GetInputFromGenome(Genome* genome)
{
	return genome->getInput();
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
