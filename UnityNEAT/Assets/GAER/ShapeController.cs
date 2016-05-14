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
        //exponential function of rotation, scaled to two times the possible value of ChildCount
        rotationTerm = (Mathf.Pow(2, bdResults.objRotation) / (Mathf.Pow(2,180)) * Height*Length*Width/2);
        print("rot angle: " + bdResults.objRotation);
        print("rotation term: " + rotationTerm);

        //exponential function of ball travel distance
        ballTravelTerm = Mathf.Pow(2, bdResults.ballTravelled);
        print("ball travel term: " + ballTravelTerm);

        float deltaMiddle= bdResults.ballRestHeight - ((TestExperiment.Height + 1.5f) / 2);
        ballRestTerm = -Mathf.Pow(2,deltaMiddle+2);
        print("ball rest term: " + ballRestTerm);

        print("material cost term: " + ChildCount);

        //Less is better
        fitnessCost =  ChildCount + rotationTerm + ballTravelTerm + ballRestTerm;
        print("fitnesscost: " + fitnessCost);


        return float.MaxValue/3.0f - fitnessCost;
    }

    public override void Stop()
    {
        if(_ballDropExperiment != null)
        {
            Destroy(_ballDropExperiment.ball);
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
