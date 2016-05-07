using UnityEngine;

namespace MarchingCubesProject
{
    public class ModelGen
    {

        //not 100% random
        public static float[,,] GenerateRandomModel(int dimSize)
        {
            float[,,] model = new float[dimSize, dimSize, dimSize];

            System.Random gen = new System.Random();

            int x, y, z, tmp;
            tmp = 0;
            for (x = 1; x < model.GetLength(0) - 1; x++)
            {
                for (y = 5; y < model.GetLength(1) - 1; y++)
                {
                    for (z = 1; z < model.GetLength(2) - 1; z++)
                    {
                        model[x, y, z] = gen.Next(2);
                    }
                }
            }

            return model;
        }

        //not 100% random
        public static float[,,] GenerateSimpleChair(int dimSize)
        {
            float[,,] model = new float[dimSize, dimSize, dimSize];

            System.Random gen = new System.Random();

            int x, y, z, tmp;
            tmp = 0;
            for (x = 1; x < model.GetLength(0) - 1; x++)
            {
                int legSize = dimSize / 4;
                for (y = 1; y < model.GetLength(1) - 1; y++)
                {
                    for (z = 1; z < model.GetLength(2) - 1; z++)
                    {
                        if (y > 5 && y < 7) model[x, y, z] = 1;
                        if (x < legSize || x > (model.GetLength(0) - 1) - legSize) model[x, y, z] = 1;
                    }
                }
            }

            return model;
        }
    }

    public class ModelRen : MonoBehaviour
    {

        private readonly float[,,] model;

        public ModelRen(float[,,] model)
        {
            this.model = model;
        }

        public GameObject Cubify()
        {
            var par = new GameObject("CubifiedModel");
            var block = GameObject.CreatePrimitive(PrimitiveType.Cube);

            //constants
            var cubeWidth = 1; //x
            var cubeHeight = 1; //y
            var cubeLength = 1; //z

            var cubes = new GameObject[model.GetLength(0), model.GetLength(1), model.GetLength(2)];

            for (int i = 0; i < model.GetLength(0); i++)
            {
                for (int j = 0; j < model.GetLength(1); j++)
                {
                    for (int k = 0; k < model.GetLength(2); k++)
                    {
                        if(model[i,j,k] == 0) continue;
                        cubes[i, j, k] = (GameObject) Instantiate(block,
                            new Vector3(par.transform.position.x + i*cubeWidth,
                                par.transform.position.y + j*cubeHeight,
                                par.transform.position.z + k*cubeLength
                                ),
                            Quaternion.identity
                            );
                        cubes[i, j, k].gameObject.AddComponent<Rigidbody>();
                        cubes[i, j, k].gameObject.AddComponent<BoxCollider>();
                        cubes[i, j, k].gameObject.transform.parent = par.transform;
                    }
                }
            }
            Destroy(block);

            for (int i = 0; i < model.GetLength(0); i++)
            {
                for (int j = 0; j < model.GetLength(1)-1; j++)
                {
                    for (int k = 0; k < model.GetLength(2)-1; k++)
                    {
                        if (cubes[i, j, k] == null) continue;

                        //check neighbours
                        if (cubes[i + 1, j, k] != null)
                        {
                            cubes[i, j, k].gameObject.AddComponent<FixedJoint>().connectedBody = cubes[i + 1, j, k].GetComponent<Rigidbody>();
                        }

                        if (cubes[i, j + 1, k] != null)
                        {
                            cubes[i, j, k].gameObject.AddComponent<FixedJoint>().connectedBody = cubes[i, j + 1, k].GetComponent<Rigidbody>();
                        }

                        if (cubes[i, j, k + 1] != null)
                        {
                            cubes[i, j, k].gameObject.AddComponent<FixedJoint>().connectedBody = cubes[i, j, k + 1].GetComponent<Rigidbody>();
                        }
                    }
                }

            }
            return par;
        }

    }

}
