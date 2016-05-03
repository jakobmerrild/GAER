using System.Collections.Generic;
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

    public class ModelRen
    {
        public struct comp
        {
            private int x;
            private int y;
            private GameObject component;

            public comp(int x, int y, GameObject component)
            {
                this.x = x;
                this.y = y;
                this.component = component;
            }
        }

        private readonly float[,,] model;

        public ModelRen(float[,,] model)
        {
            this.model = model;
        }

        public GameObject Cubify()
        {
            var par = new GameObject("CubifiedModel");
            return par;
        }

 /*       public List<comp> SliceModel(int layerIndex)
        {
            var components = new List<comp>();

            for (int x = 0; x < model.GetLength(0); x++)
            {
                for (int z = 0; z < model.GetLength(2); z++)
                {
                    int startX, startZ, endX, endZ;
                    if (model[x, layerIndex, z] == 1)
                    {
                        startX = x; endX = x;
                        startZ = z; endZ = z;
                        while (model[endX, layerIndex, endZ] == 1.0f) endX++;
                        for (int dX = startX; dX <= endX; endZ++)
                        {
                            //check if adjacent row of same size exists
                            while (model[dX, layerIndex, endZ] == 1.0f) dX++;
                            if(dX < endX) break;
                        }
                        GameObject go = new GameObject();
                        go.transform.localScale = new Vector3(endX-startX, 1, endZ-startZ);
                        components.Add( new comp(startX, startZ, go) );
                    }
                }
            }

            return components;
        }*/

        public void PreProcessSlice(int y_index)
        {
            int[,] connected = new int[model.GetLength(0), model.GetLength(2)];

            int cmp = 1;
            for (int x = 0; x < connected.GetLength(0); x++)
            {
                for (int z = 0; z < connected.GetLength(1); z++)
                {
                    if (model[x, y_index, z] == 1)
                    {
                        connected[x, z] = cmp;

                        //Adjacent right
                        if (model[x + 1, y_index, z] == 1) connected[x + 1, z] = cmp;
                        else cmp++;

                        //Adjacent below
                        if (model[x, y_index, z + 1] == 1) connected[x, z + 1] = cmp;
                        else cmp++;
                    }
                }
            }

        }

    }

}
