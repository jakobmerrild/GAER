using System;
using UnityEngine;
using System.Linq;

namespace MarchingCubesProject
{
    public class Example : MonoBehaviour
    {

        public Material m_material;

        GameObject[] m_mesh;

        //Model dimension size
        static int dimSize = 12;

        //Population size for the grid
        static int popSize = 16;

        //Margin between population members
        static int margin = 5;

        float[] results = null;

        // Use this for initialization
        void Start()
        {
            //Target is the value that represents the surface of mesh
            //For example the perlin noise has a range of -1 to 1 so the mid point is were we want the surface to cut through
            //The target value does not have to be the mid point it can be any value with in the range
            MarchingCubes.SetTarget(0.0f);

            //Winding order of triangles use 2,1,0 or 0,1,2
            //MarchingCubes.SetWindingOrder(2, 1, 0);
            MarchingCubes.SetWindingOrder(0, 1, 2);

            //Set the mode used to create the mesh
            //Cubes is faster and creates less verts, tetrahedrons is slower and creates more verts but better represents the mesh surface
            MarchingCubes.SetModeToCubes();
            //MarchingCubes.SetModeToTetrahedrons();

            float start = Time.realtimeSinceStartup;

            m_mesh = new GameObject[popSize];

            float[,,] vx = new float[dimSize,dimSize,dimSize];
            for (int i = 1; i < vx.GetLength(0)-1; i++)
            {
                for (int j = 1; j < vx.GetLength(1)-1; j++)
                {
                    for (int k = 1; k < vx.GetLength(2)-1; k++)
                    {
                        vx[i, j, k] = 1.0f;
                    }
                }
            }

            m_mesh[0] = genObject(vx);
            m_mesh[1] = genObject( ModelGen.GenerateSimpleChair(dimSize) );

            for (int i = 2; i < popSize; i++)
            {
                var voxels = ModelGen.GenerateRandomModel(dimSize);
                m_mesh[i] = new ModelRen( voxels ).Cubify();
            }

            
            //population, width, height, length
            StartCoroutine( Tester.Testpopulation(m_mesh, dimSize, dimSize, dimSize) );

            Debug.Log("Time take = " + (Time.realtimeSinceStartup - start) * 1000.0f);

        }

        private GameObject genObject(float[,,] voxels)
        {
            Mesh mesh = MarchingCubes.CreateMesh(voxels);

            //The diffuse shader wants uvs so just fill with a empty array, there not actually used
            mesh.uv = new Vector2[mesh.vertices.Length];
            mesh.RecalculateNormals();

            GameObject go = new GameObject("Mesh");
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            go.GetComponent<Renderer>().material = m_material;
            go.GetComponent<MeshFilter>().mesh = mesh;

            var mc = go.AddComponent<MeshCollider>();
            mc.convex = true;

            var rr = go.AddComponent<Rigidbody>();
            rr.useGravity = true;

            return go;
        }

        // Update is called once per frame
        void Update()
        {
            if (results != null)
            {
                Debug.Log("RESULTS " + results.Length);
                String formatted = results.Aggregate("Results:\n", (current, t) => String.Concat(current, t + "\n"));
                Debug.Log(formatted);
                results = null;
            }
        }

    }

}
