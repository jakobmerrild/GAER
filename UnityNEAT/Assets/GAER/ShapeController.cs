using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System;
using System.Collections.Generic;
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
    private List<GameObject> _components;
    private int _numVoxels;
    // Use this for initialization
    void Start()
    {
        MarchingCubes.SetTarget(0.5f);
        MarchingCubes.SetWindingOrder(0,1,2);
        MarchingCubes.SetModeToCubes();
    }

    void FixedUpdate() {    }

    public override void Activate(IBlackBox box)
    {
        _numVoxels = 0;
        for (int x = 1; x < Width-1; x++)
        {
            for (int y = 1; y < Height-1; y++)
            {
                for (int z = 1; z < Length-1; z++)
                {
                    box.ResetState();
                    box.InputSignalArray[0] = x;
                    box.InputSignalArray[1] = y;
                    box.InputSignalArray[2] = z;
                    box.Activate();
                    _voxels[x, y, z] = (float)box.OutputSignalArray[0];
                    if (_voxels[x, y, z] > 0.5f)
                        _numVoxels++;
                }
            }
        }

        //Mesh mesh = MarchingCubes.CreateMesh(_voxels);

        //mesh.uv = new Vector2[mesh.vertices.Length];
        //mesh.RecalculateNormals();

        //m_mesh = new GameObject("Mesh");
        //m_mesh.AddComponent<MeshFilter>();
        //m_mesh.AddComponent<MeshRenderer>();
        //m_mesh.GetComponent<Renderer>().material = m_material;
        //m_mesh.GetComponent<MeshFilter>().mesh = mesh;
        ////Center mesh
        //m_mesh.transform.position = transform.position;
        _components = Geometry.FindComponents(_voxels, 0.5f);
    }

    public override float GetFitness()
    {
        int[,,] labels = Geometry.FindComponentsProbably(_voxels, 0.5f);
        int componentCount = 0;
        for (int x = 1; x < Width - 1; x++)
        {
            for (int y = 1; y < Height - 1; y++)
            {
                for (int z = 1; z < Length - 1; z++)
                {
                    componentCount = Math.Max(componentCount, labels[x, y, z]);
                }
            }
        }
        return componentCount/(_numVoxels+1);
    }

    public override void Stop()
    {
        _components.ForEach(Destroy);        
    }


}



