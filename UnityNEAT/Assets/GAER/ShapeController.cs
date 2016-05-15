using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System;
using System.Diagnostics;
using System.Linq;
using GAER;
using MarchingCubesProject;

public class ShapeController : UnitController
{
    public Material m_material;
    GameObject m_mesh;
    private static readonly int Width = TestExperiment.Width;
    private static readonly int Height = TestExperiment.Height;
    private static readonly int Length = TestExperiment.Length;
    private readonly float[,,] _voxels = new float[Width, Height, Length];
    private int _numVoxels;
    private float _threshold = 0.5f;
    private Stopwatch sw = new Stopwatch();
    public int ChildCount;
    private PhysicsTester.BallDropExperiment _ballDropExperiment =null;
    private Color _baseColor;
    public float rotationTerm;
    public float ballTravelTerm;
    public float ballRestTerm;
    public float fitnessCost;

    // Use this for initialization
    void Start()
    {
        MarchingCubes.SetTarget(_threshold);
        MarchingCubes.SetWindingOrder(0,1,2);
        MarchingCubes.SetModeToCubes();
        _baseColor = m_material.color;
    }

    void FixedUpdate() {    }

    public override void Activate(IBlackBox box)
    {
        Box = box;
        sw.Start();
        _numVoxels = 0;
        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Length; z++)
                {
                    box.ResetState();
                    box.InputSignalArray[0] = x;
                    box.InputSignalArray[1] = y;
                    box.InputSignalArray[2] = z;
                    box.Activate();
                    _voxels[x, y, z] = (float)box.OutputSignalArray[0];
                    if (_voxels[x, y, z] > _threshold)
                        _numVoxels++;
                }
            }
        }
        print(String.Format("Time taken to generate voxels: {0}ms", sw.ElapsedMilliseconds));

        var stopWatch = new Stopwatch();
        stopWatch.Start();
        Geometry.FindLargestComponent(_voxels, _threshold, gameObject);
        stopWatch.Stop();
        ChildCount = transform.childCount;
        _ballDropExperiment = PhysicsTester.StartBallDropExperiment(gameObject);
        print(String.Format("It took {0}ms to find largest component.", stopWatch.ElapsedMilliseconds));


    }

    public override float GetFitness()
    {
        sw.Stop();
        print(String.Format("It took {0}ms from activation to fitness evaluation.", sw.ElapsedMilliseconds));

        PhysicsTester.BallDropResults bdResults = PhysicsTester.MeassureBallDropExperiment(_ballDropExperiment);

        print("Balldrop: " + bdResults);
        //pot function of rotation, scaled to two times the possible value of ChildCount       
        rotationTerm = bdResults.objRotation/180.0f*Height*Width*Length*3.2f;
        //print("rot angle: " + bdResults.objRotation);
        //print("rotation term: " + rotationTerm);

        //exponential function of ball travel distance
        ballTravelTerm = Mathf.Min(1000, Mathf.Pow(3, 2*bdResults.ballTravelled));
        //print("ball travel term: " + ballTravelTerm);

        float deltaMiddle= bdResults.ballRestHeight - ((TestExperiment.Height + 2f) / 2);
        ballRestTerm = Mathf.Pow(2,-(deltaMiddle*2));
        print("ball rest term: " + ballRestTerm);

        float headHeightTerm = -(bdResults.headOverHips/6.75f)*1000;
        //print("material cost term: " + ChildCount);

        //Less is better
        fitnessCost =  ChildCount + rotationTerm + ballTravelTerm + ballRestTerm + headHeightTerm;
        print("fitnesscost: " + fitnessCost);

	    float maxRotation = Height*Length*Width*3.2f;
	    float maxBallTravel = 1000;
	    float maxBallRest = Mathf.Pow(2,(TestExperiment.Height+2f));
	    float maxChildCount = Height * Length * Width;
        float maxHead = 1000;

	    if (rotationTerm > maxRotation) {
		    throw new SystemException("rotation term above assumed max");
	    }
	    if (ballTravelTerm > maxBallTravel) {
		    throw new SystemException("ball travel term above assumed max");
	    }
	    if (ballRestTerm > maxBallRest) {
		    throw new SystemException("ball rest term above assumed max");
	    }
	    if (ChildCount > maxChildCount) {
		    throw new SystemException("child count term above assumed max");
	    }
        if (headHeightTerm > maxHead)
        {
            throw new SystemException("head height term above assumed max");
        }

            return (ChildCount == 0) ? 0 : (maxChildCount + maxRotation + maxBallTravel + maxBallRest) - fitnessCost;
        }

    public override void Stop()
    {
        if(_ballDropExperiment != null)
        {
            Destroy(_ballDropExperiment.toDestroy);
        }
    }

    protected override void OnMouseDown()
    {
        base.OnMouseDown();
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = Color.black;
        }
    }

    public override void DeSelect()
    {
        foreach (var r in GetComponentsInChildren<Renderer>())
        {
            r.material.color = _baseColor;
        }
    }
}
