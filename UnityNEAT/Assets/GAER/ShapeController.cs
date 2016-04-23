using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;
using System;
using MarchingCubesProject;

public class ShapeController : UnitController
{
    public Material m_material;
    GameObject m_mesh;
    private int width = 32;
    private int height = 32;
    private int length = 32;
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
        float[,,] voxels = new float[width, height, length];
        for (int x = 1; x < width-1; x++)
        {
            for (int y = 1; y < height-1; y++)
            {
                for (int z = 1; z < length-1; z++)
                {
                    box.ResetState();
                    box.InputSignalArray[0] = x;
                    box.InputSignalArray[1] = y;
                    box.InputSignalArray[2] = z;
                    box.Activate();
                    voxels[x, y, z] = (float)box.OutputSignalArray[0];
                    if (voxels[x, y, z] > 0.5f)
                        _numVoxels++;
                }
            }
        }

        Mesh mesh = MarchingCubes.CreateMesh(voxels);

        mesh.uv = new Vector2[mesh.vertices.Length];
        mesh.RecalculateNormals();

        m_mesh = new GameObject("Mesh");
        m_mesh.AddComponent<MeshFilter>();
        m_mesh.AddComponent<MeshRenderer>();
        m_mesh.GetComponent<Renderer>().material = m_material;
        m_mesh.GetComponent<MeshFilter>().mesh = mesh;
        //Center mesh
        m_mesh.transform.position = transform.position;
    }

    public override float GetFitness()
    {
        int target = length * height * width / 3;
        var fit = -Math.Abs(_numVoxels - target) + length*height*width;
        return fit;
    }

    public override void Stop()
    {
    }


}



