using UnityEngine;
using System.Collections;
using System.Xml;
using SharpNeat.Domains;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;

public interface ISimpleExperiment : INeatExperiment
{
    NeatEvolutionAlgorithm<NeatGenome> CreateEvolutionAlgorithm(string fileName);
    void Initialize(string name, XmlElement xmlConfig, int input, int output);
    void SetOptimizer(Optimizer optimizer);
}
