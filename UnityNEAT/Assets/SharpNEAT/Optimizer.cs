//#define ROTATECAMERA
using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System.Collections.Generic;
using SharpNeat.EvolutionAlgorithms;
using SharpNeat.Genomes.Neat;
using System;
using System.Xml;
using System.IO;
using System.Linq;
using GAER;
using SharpNeat.Core;


public class Optimizer : MonoBehaviour {

    const int NUM_INPUTS = 3;
    const int NUM_OUTPUTS = 1;

    public int Trials;
    public float TrialDuration;
    public float StoppingFitness;
    private bool _eaRunning;
    private string popFileSavePath, champFileSavePath, bestFileSavePath;

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

    private int _xCounter, _zCounter, _counter; //Counters used when calculating the position of objects.
    private int _xOffset = -200;
    private int _xLimit = 200;
    private int _xFactor = TestExperiment.Width+6 , _zFactor = TestExperiment.Length + 6;

    //Store _numBestPhenomes phenomes everytime the EA pauses. See ea_PauseEvent
    private const int NumBestPhenomes = 10;
    private List<IBlackBox> _bestPhenomes;
    private List<NeatGenome> _bestGenomes;
    private Dictionary<IBlackBox, NeatGenome> _phenomesMap = new Dictionary<IBlackBox, NeatGenome>();

    private bool _loadOldPopulation;
    public ShapeController SelectedController;
    public uint SelectedGenomeId { get; private set; }
    public uint SelectedGeneration { get; private set; }
    public int TimeSinceSelection { get { return (int) _ea.CurrentGeneration - (int) SelectedGeneration; }}
    public const int DecayTime = 10;
    public const float SelectionBoost = 1e5f;

    #region Unity methods
    // Use this for initialization
    void Start () {
        Utility.DebugLog = true;
        experiment = new TestExperiment();
        XmlDocument xmlConfig = new XmlDocument();
        TextAsset textAsset = (TextAsset)Resources.Load("experiment.config");
        xmlConfig.LoadXml(textAsset.text);
        experiment.SetOptimizer(this);

        experiment.Initialize("Chair Experiment", xmlConfig.DocumentElement, NUM_INPUTS, NUM_OUTPUTS);

        champFileSavePath = Application.persistentDataPath + string.Format("/{0}.champ.xml", "chair");
        popFileSavePath = Application.persistentDataPath + string.Format("/{0}.pop.xml", "chair");
        bestFileSavePath = Application.persistentDataPath + string.Format("/{0}.best.{1}.xml", "chair", NumBestPhenomes);
	    StoppingFitness = TestExperiment.Height*TestExperiment.Length*TestExperiment.Width;
        print(champFileSavePath);
        var camera = GameObject.FindGameObjectWithTag("MainCamera");
        camera.GetComponent<GhostFreeRoamCamera>().allowMovement = false;
        camera.GetComponent<GhostFreeRoamCamera>().allowRotation = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        //var rng = new System.Random();
        //float[,,] voxels = new float[10,10,10];
        //for (int x = 0; x < 10; x++)
        //{
        //    voxels[0, 0, x] = 1;
        //    voxels[0, x, 0] = 1;
        //    voxels[x, 0, 0] = 1;
        //    for (int y = 0; y < 10; y++)
        //    {
        //        for (int z = 0; z < 10; z++)
        //        {
        //            //if((x < 1 && y < 1 && z < 1) || (x > 8 && y > 8 && z > 8))
        //            //    voxels[x, y, z] = 1.0f;
        //        }
        //    }
        //}
        //GameObject parent = new GameObject();
        //var children = Geometry.FindLargestComponent(voxels, 0.5f);
        //parent.AddComponent<Rigidbody>();
        //foreach (var child in children)
        //{
        //    child.transform.parent = parent.transform;
        //}
        //var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //sphere.AddComponent<Rigidbody>();
        //sphere.GetComponent<Rigidbody>().mass = 100;
        //sphere.transform.position = new Vector3(1, 20, 1);
        //sphere.transform.localScale = new Vector3(10, 10, 10);


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
#endregion

    #region Listener methods for subscribing to EA events.
    //Fields used to automatically request the EA to pause at certain intervals.
    private ulong _updateCounter;
    private const uint Intervals = 3; //Adjust this up to make the auto pause happen more rarely, and down for more frequently.
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
        // save the _numBestPhenomes best phenomes and evaluate them(show them on screen.)
        
        var decoder = experiment.CreateGenomeDecoder();
        _bestGenomes = _ea.GenomeList.OrderByDescending(x => x.EvaluationInfo.Fitness).Take(NumBestPhenomes).ToList();
        
        _bestPhenomes = _bestGenomes.Select(x => decoder.Decode(x)).ToList();
        _phenomesMap = _bestPhenomes.Select((k, i) => new {k, v = _bestGenomes[i]}).ToDictionary(x => x.k,x => x.v);
        _bestPhenomes.ForEach(Evaluate);
        ResetCounters(); //Hack to reset the counters used to calculate the position of the objects.
        using (XmlWriter xw = XmlWriter.Create(bestFileSavePath, _xwSettings))
        {
            experiment.SavePopulation(xw, _bestGenomes);
        }
        DateTime endTime = DateTime.Now;
        Utility.Log("Total time elapsed: " + (endTime - startTime));

        var camera = GameObject.FindGameObjectWithTag("MainCamera");
#if (ROTATECAMERA)
        camera.transform.Rotate(Vector3.up, 180.0f);
#endif
        //Unlock camera movement.
        camera.GetComponent<GhostFreeRoamCamera>().allowMovement = true;
        camera.GetComponent<GhostFreeRoamCamera>().allowRotation = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;


        //System.IO.StreamReader stream = new System.IO.StreamReader(popFileSavePath);
        _eaRunning = false;        
        
    }
#endregion

    #region UnityNEAT methods
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
                ResetCounters();
            }
                
        }
 
        GameObject obj = Instantiate(Unit, new Vector3(xPos, 0, zPos), Unit.transform.rotation) as GameObject;
        UnitController controller = obj.GetComponent<UnitController>();
        controller.MouseDownEvent += (sender, args) =>
        {
            var shapeController = sender as ShapeController;
            if (SelectedController != null)
            {
                SelectedController.DeSelect();
            }
            if (shapeController != null)
            {
                SelectedController = shapeController;
            }
        };
        ControllerMap.Add(box, controller);

        controller.Activate(box);
    }

    public void StopEvaluation(IBlackBox box)
    {
        UnitController ct = ControllerMap[box];
        ct.Stop();
        Destroy(ct.gameObject);
        //ControllerMap.Remove(box);
    }

    public float GetFitness(IBlackBox box)
    {
        if(ControllerMap.ContainsKey(box))
        {
            return ControllerMap[box].GetFitness();
        }
        return 0;
    }

    #endregion

    #region Utility methods
    private void ResetCounters()
    {
        _xCounter = 0;
        _zCounter = 0;
        _counter = 0;
    }
#endregion

    #region GUI methods
    /// <summary>
    /// Method to create some buttons on the GUI.
    /// </summary>
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
        _loadOldPopulation = GUI.Toggle(new Rect(10, 210, 200, 40), _loadOldPopulation, "Load old population.");

        GUI.Button(new Rect(10, Screen.height - 70, 100, 60), string.Format("Generation: {0}\nFitness: {1:0.00}", Generation, Fitness - float.MaxValue));
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

    public void StartEA()
    {

        var camera = GameObject.FindGameObjectWithTag("MainCamera");
#if (ROTATECAMERA)
        camera.transform.Rotate(Vector3.up, 180.0f);
#endif
        //lock camera movement
        camera.GetComponent<GhostFreeRoamCamera>().allowMovement = false;
        camera.GetComponent<GhostFreeRoamCamera>().allowRotation = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        var evoSpeed = 100;

        if (_ea == null)
        {
            Utility.DebugLog = true;
            Utility.Log("Starting PhotoTaxis experiment");
            // print("Loading: " + popFileLoadPath);
            if (_loadOldPopulation)
            {
                _ea = experiment.CreateEvolutionAlgorithm(popFileSavePath);
            }
            else
            {
                _ea = experiment.CreateEvolutionAlgorithm();
            }

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
            if (SelectedController != null)
            {
                var selectedPhenome = SelectedController.Box;
                var selectedGenome = _phenomesMap[selectedPhenome];
                selectedGenome.EvaluationInfo.SetFitness(float.MaxValue);
                SelectedGenomeId = selectedGenome.Id;
                SelectedGeneration = _ea.CurrentGeneration;
            }
            Time.timeScale = evoSpeed;
            _ea.StartContinue();
            _bestPhenomes.ForEach(StopEvaluation);
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
    #endregion
}
