using System;
using System.Collections.Generic;
using UnityEngine;

namespace GAER {

	public class Geometry {

		public static float MaterialCost3d(float[,,] voxels, float unitCost, float threshold) {
			int count = 0;
			for(int i = 0; i < voxels.GetLength(0); i++) {
				for(int j = 0; j < voxels.GetLength(1); j++) {
					for(int k = 0; k < voxels.GetLength(2); k++) {
						if (voxels[i,j,k] > threshold){
							count++;
						}
					}
				}
			}
			return count*unitCost;
		}

	    public static GameObject FindLargestComponent(float[,,] voxels, float threshold, GameObject parent)
	    {
	        int label;
	        var labels = FindComponentsProbably(voxels, threshold, out label);
	        float xSum = 0, ySum = 0, zSum = 0; //for gravity?
	        if (label <= 0)
	        {
	            return parent;
	        }
	        for (int x = 0; x < voxels.GetLength(0); x++)
	        {
	            for (int y = 0; y < voxels.GetLength(1); y++)
	            {
	                for (int z = 0; z < voxels.GetLength(2); z++)
	                {
	                    if (labels[x, y, z] == label)
	                    {
	                        var positions = GeneratePositions(x, y, z);
	                        bool surrounded = true;
	                        foreach (var position in positions)
	                        {
	                            int _x = position[0];
	                            int _y = position[1];
                                int _z = position[2];
	                            if (_x < 0 || labels.GetLength(0) <= _x
	                                || _y < 0 || labels.GetLength(1) <= _y
	                                || _z < 0 || labels.GetLength(2) <= _z)
	                            {
	                                surrounded = false;
	                                break;
	                            }
                                if (labels[x, y, z] != label)
                                {
                                    surrounded = false;
                                    break;
                                }
                                
	                        }
	                        if (!surrounded)
	                        {
                                var cube = GameObject.CreatePrimitive((PrimitiveType.Cube));
                                cube.transform.parent = parent.transform;
                                cube.transform.localPosition = new Vector3(x, y, z);
                                cube.GetComponent<Renderer>().sharedMaterial = parent.GetComponent<ShapeController>().m_material;
                            }
                            
	                    }
	                }
	            }
	        }
	        return parent;
	    }

		public static int[,,] FindComponents(float[,,] voxels, float threshold, out int maxSizeLabel)
		{
			int[,,] labels = new int[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];
			int label = 1;
		    int maxSize = 0;
		    maxSizeLabel = 0;
			for(int i = 0; i < voxels.GetLength(0); i++)
			{
				for (int j = 0; j < voxels.GetLength(1); j++)
				{
					for (int k = 0; k < voxels.GetLength(2); k++)
					{
					    int size;
						label = GrassFire(voxels, i, j, k, labels, label, threshold, out size);
					    if (size > maxSize)
					    {
					        maxSize = size;
					        maxSizeLabel = label-1; //this is ugly as fuck
					    }
					}
				}
			}
			return labels;
		}

		public static int[,,] FindComponentsProbably(float[,,] voxels, float threshold, out int maxSizeLabel){
			int[,,] labels = new int[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];
			int label = 1;
		    int maxSize = 0;
		    maxSizeLabel = 0;
			int xrank = voxels.GetLength(0);
			int zrank = voxels.GetLength(2);
			for (int x = 0; x<2;x++){
				for (int z = 0; z<2;z++){
					for (int y = 0; y<voxels.GetLength(1); y++)
                    {
                        int size;
                        label = GrassFire(voxels, xrank/4 + xrank/2*x, y, zrank/4 + zrank/2*z, labels, label, threshold, out size);
                        if (size > maxSize)
                        {
                            maxSize = size;
                            maxSizeLabel = label - 1;
                        }
					}
				}
			}
			return labels;

		}

		private static int GrassFire(float[,,] voxels, int x, int y, int z, int[,,] labels, int label, float threshold, out int size)
		{
			Stack<int[]> s = new Stack<int[]>();
			s.Push(new int[] { x, y, z });
		    size = 0;
			while (s.Count > 0)
			{
				int[] currentPoint = s.Pop();
				x = currentPoint[0];
				y = currentPoint[1];
				z = currentPoint[2];

				//bounds check
				if (x < 0 || voxels.GetLength(0) <= x
						|| y < 0 || voxels.GetLength(1) <= y
						|| z < 0 || voxels.GetLength(2) <= z)
				{
					continue;
				}

				if (labels[x, y, z] == 0 && voxels[x, y, z] > threshold)
				{
				    ++size;		
					labels[x, y, z] = label;
					foreach (int[] p in GeneratePositions(x, y, z))
					{
						s.Push(p);
					}
				}
			}

		    if (size > 0)
		    {
		        return label + 1;

		    }
		    return label;
		}

		private static int[][] GeneratePositions(int x, int y, int z)
		{
			int[][] res = new int[26][];
			int r = 0;
			for(int i = -1;i <= 1; i++)
			{
				for(int j = -1; j <= 1; j++)
				{
					for (int k = -1; k <= 1; k++)
					{
						if(i==0 && j == 0 && k == 0)
						{
							continue;
						}
						res[r++] = new int[] {x + i, y + j, z + k};
					}
				}
			}
			return res;
		}
	}
}
