﻿using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System.Collections.Generic;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System;
using System.Xml;
using System.IO;
using GAER;
using SharpNeat.Core;

public class Optimizer : MonoBehaviour {

    const int NUM_INPUTS = 3;
    const int NUM_OUTPUTS = 1;

    public int Trials;
    public float TrialDuration;
    public float StoppingFitness;
    private bool _eaRunning;
    private string popFileSavePath, champFileSavePath;

    ISimpleExperiment experiment;
    private NeatEvolutionAlgorithm<NeatGenome> _ea;

    public GameObject Unit;

    Dictionary<IBlackBox, UnitController> ControllerMap = new Dictionary<IBlackBox, UnitController>();
    private DateTime startTime;
    private float timeLeft;
    private float accum;
    private int frames;
    private float updateInterval = 12;

    private uint Generation;
    private double Fitness;

    private int _xCounter, _zCounter;
    private int _xOffset = -200;
    private int _xLimit = 200;
    private int _xFactor = TestExperiment.Width+6 , _zFactor = TestExperiment.Length + 6;

    public bool Ready { get; private set; }

	// Use this for initialization
	void Start () {
        Utility.DebugLog = true;
        experiment = new TestExperiment();
        XmlDocument xmlConfig = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("experiment.config");
        xmlConfig.LoadXml(textAsset.text);
        experiment.SetOptimizer(this);

        experiment.Initialize("Chair Experiment", xmlConfig.DocumentElement, NUM_INPUTS, NUM_OUTPUTS);

        champFileSavePath = Application.persistentDataPath + string.Format("/{0}.champ.xml", "car");
        popFileSavePath = Application.persistentDataPath + string.Format("/{0}.pop.xml", "car");
	    StoppingFitness = TestExperiment.Height*TestExperiment.Length*TestExperiment.Width;
        print(champFileSavePath);
	}

    // Update is called once per frame
    void Update()
    {
        //  evaluationStartTime += Time.deltaTime;
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        if (timeLeft <= 0.0)
        {
            var fps = accum / frames;
            timeLeft = updateInterval;
            accum = 0.0f;
            frames = 0;
            //   print("FPS: " + fps);
            if (fps < 10)
            {
                Time.timeScale = Time.timeScale - 1;
                print("Lowering time scale to " + Time.timeScale);
            }
        }
    }

    public void StartEA()
    {
        var evoSpeed = 25;
        if (_ea == null)
        {
            Utility.DebugLog = true;
            Utility.Log("Starting PhotoTaxis experiment");
            // print("Loading: " + popFileLoadPath);
            _ea = experiment.CreateEvolutionAlgorithm(popFileSavePath);
            startTime = DateTime.Now;

            _ea.UpdateEvent += ea_UpdateEvent;
            _ea.PausedEvent += ea_PauseEvent;

            //   Time.fixedDeltaTime = 0.045f;
            Time.timeScale = evoSpeed;
            _ea.StartContinue();
            _eaRunning = true;
        }
        else if (_ea.RunState == RunState.Paused)
        {
            Time.timeScale = evoSpeed;
            _ea.StartContinue();
            _eaRunning = true;
        }
            

    }
    /// <summary>
    /// Method to be called by the "Pause EA" button.
    /// </summary>
    public void PauseEA()
    {
        if (_ea != null && _ea.RunState == RunState.Running)
        {
            _ea.RequestPause();
        }
    }

    private ulong _updateCounter;
    private const uint Intervals = 10;
    /// <summary>
    /// Callback method for the update event on the EA.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ea_UpdateEvent(object sender, EventArgs e)
    {
        _updateCounter++;
        if (_updateCounter%Intervals == 0)
        {
            _ea.Stop();
        }
           
        //Utility.Log(string.Format("gen={0:N0} bestFitness={1:N6}", _ea.CurrentGeneration, _ea.Statistics._maxFitness));

        Fitness = _ea.Statistics._maxFitness;
        Generation = _ea.CurrentGeneration;
      

    //    Utility.Log(string.Format("Moving average: {0}, N: {1}", _ea.Statistics._bestFitnessMA.Mean, _ea.Statistics._bestFitnessMA.Length));

    
    }
    /// <summary>
    /// Callback method for the pause event on the EA.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    void ea_PauseEvent(object sender, EventArgs e)
    {
        Time.timeScale = 1;
        Utility.Log("Done ea'ing (and neat'ing)");

        XmlWriterSettings _xwSettings = new XmlWriterSettings();
        _xwSettings.Indent = true;
        // Save genomes to xml file.        
        DirectoryInfo dirInf = new DirectoryInfo(Application.persistentDataPath);
        if (!dirInf.Exists)
        {
            Debug.Log("Creating subdirectory");
            dirInf.Create();
        }
        using (XmlWriter xw = XmlWriter.Create(popFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, _ea.GenomeList);
        }
        // Also save the best genome

        using (XmlWriter xw = XmlWriter.Create(champFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, new NeatGenome[] { _ea.CurrentChampGenome });
        }
        DateTime endTime = DateTime.Now;
        Utility.Log("Total time elapsed: " + (endTime - startTime));

        //System.IO.StreamReader stream = new System.IO.StreamReader(popFileSavePath);
       

      
        _eaRunning = false;        
        
    }
    /// <summary>
    /// Method to be called by the Stop EA button
    /// </summary>
    public void StopEA()
    {

        if (_ea != null && _ea.RunState == SharpNeat.Core.RunState.Running)
        {
            _ea.Stop();
        }
    }

    private int _counter;

    public void Evaluate(IBlackBox box)
    {
        int xPos, zPos;
        lock(this)
        {
            xPos = _xCounter * _xFactor + _xOffset;
            if (xPos < _xLimit)
            {
                _xCounter++;
            }
                
            else
            {
                _xCounter = 0;
                _zCounter++;
            }
            zPos = _zCounter * _zFactor;
            _counter++;
            if (_counter == experiment.DefaultPopulationSize)
            {
                _xCounter = 0;
                _zCounter = 0;
                _counter = 0;
            }
                
        }
 
        GameObject obj = Instantiate(Unit, new Vector3(xPos, 0, zPos), Unit.transform.rotation) as GameObject;
        UnitController controller = obj.GetComponent<UnitController>();

        ControllerMap.Add(box, controller);

        controller.Activate(box);
    }


    public void StopEvaluation(IBlackBox box)
    {
        UnitController ct = ControllerMap[box];
        ct.Stop();
        Destroy(ct.gameObject);
        ControllerMap.Remove(box);
    }

    public void RunBest()
    {
        Time.timeScale = 1;

        NeatGenome genome = null;


        // Try to load the genome from the XML document.
        try
        {
            using (XmlReader xr = XmlReader.Create(champFileSavePath))
                genome = NeatGenomeXmlIO.ReadCompleteGenomeList(xr, false, (NeatGenomeFactory)experiment.CreateGenomeFactory())[0];


        }
        catch (Exception)
        {
            // print(champFileLoadPath + " Error loading genome from file!\nLoading aborted.\n"
            //						  + e1.Message + "\nJoe: " + champFileLoadPath);
            return;
        }

        // Get a genome decoder that can convert genomes to phenomes.
        var genomeDecoder = experiment.CreateGenomeDecoder();

        // Decode the genome into a phenome (neural network).
        var phenome = genomeDecoder.Decode(genome);

        GameObject obj = Instantiate(Unit, Unit.transform.position, Unit.transform.rotation) as GameObject;
        UnitController controller = obj.GetComponent<UnitController>();

        ControllerMap.Add(phenome, controller);

        controller.Activate(phenome);
    }

    public float GetFitness(IBlackBox box)
    {
        if (ControllerMap.ContainsKey(box))
        {
            return ControllerMap[box].GetFitness();
        }
        return 0;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 100, 40), "Start EA"))
        {
            StartEA();
        }
        if (GUI.Button(new Rect(10, 60, 100, 40), "Pause EA"))
        {
            PauseEA();
        }
        if (GUI.Button(new Rect(10, 110, 100, 40), "Stop EA"))
        {
            StopEA();
        }

        if (GUI.Button(new Rect(10, 160, 100, 40), "Run best"))
        {
            RunBest();
        }
        GUI.Button(new Rect(10, Screen.height - 70, 100, 60), string.Format("Generation: {0}\nFitness: {1:0.00}", Generation, Fitness));
    }


}
