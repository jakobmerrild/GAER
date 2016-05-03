using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking.Match;
using Object = UnityEngine.Object;

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


		public static List<GameObject> FindComponents(float[,,] voxels, float threshold)
		{
		    ConnectedComponent result = new ConnectedComponent();
			int[,,] labels = new int[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];
			int label = 1;
			for(int i = 0; i < voxels.GetLength(0); i++)
			{
				for (int j = 0; j < voxels.GetLength(1); j++)
				{
					for (int k = 0; k < voxels.GetLength(2); k++)
					{
					    ConnectedComponent conComp;
						label = GrassFire(voxels, i, j, k, labels, label, threshold, out conComp);
					    if (conComp.Size > result.Size)
					    {
                            result.Components.ForEach(Object.Destroy);
                            result = conComp;
                        }
					        
					}
				}
			}
			return result.Components;
		}

		public static int[,,] FindComponentsProbably(float[,,] voxels, float threshold){
			int[,,] labels = new int[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];
			int label = 1;
			int xrank = voxels.GetLength(0);
			int zrank = voxels.GetLength(2);
			for (int x = 0; x<2;x++){
				for (int z = 0; z<2;z++){
					for (int y = 0; y<voxels.GetLength(1); y++)
					{
					    ConnectedComponent conComp;
						label = GrassFire(voxels, xrank/4 + xrank/2*x, y, zrank/4 + zrank/2*z, labels, label, threshold, out conComp);
					}
				}
			}
			return labels;

		}

		private static int GrassFire(float[,,] voxels, int x, int y, int z, int[,,] labels, int label, float threshold, out ConnectedComponent result)
		{
            result = new ConnectedComponent {Label = label};
			Stack<int[]> s = new Stack<int[]>();
			s.Push(new int[] { x, y, z });
			int nextLabel = label;

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
				    var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    cube.transform.position = new Vector3(x, y, z);
				    cube.AddComponent<Rigidbody>();
				    if (result.Parent == null)
				    {
				        result.Parent = cube;
				    }
				    else
				    {
                        cube.AddComponent<FixedJoint>().connectedBody = result.Parent.GetComponent<Rigidbody>();
                    }
                    result.Components.Add(cube);
                    result.Size++;

                    nextLabel++;
					labels[x, y, z] = label;
					foreach (int[] p in GeneratePositions(x, y, z))
					{
						s.Push(p);
					}
				}
			}
			return nextLabel;
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

	    private class ConnectedComponent
	    {
	        public int Size { get; set; }
	        public int Label { get; set; }
	        public GameObject Parent { get; set; }
	        public List<GameObject> Components { get; private set; }

	        public ConnectedComponent()
	        {
	            Components = new List<GameObject>();
	        }
	    }
	}
}
