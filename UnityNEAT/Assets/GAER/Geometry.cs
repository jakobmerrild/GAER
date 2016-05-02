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

        public static int[,,] FindComponents(float[,,] voxels, float threshold)
        {
            int[,,] labels = new int[voxels.GetLength(0), voxels.GetLength(1), voxels.GetLength(2)];
            int label = 1;
            for(int i = 0; i < voxels.GetLength(0); i++)
            {
                for (int j = 0; j < voxels.GetLength(1); j++)
                {
                    for (int k = 0; k < voxels.GetLength(2); k++)
                    {
                        label = GrassFire(voxels, i, j, k, labels, label, threshold);
                    }
                }
            }
            return labels;
        }

        private static int GrassFire(float[,,] voxels, int x, int y, int z, int[,,] labels, int label, float threshold)
        {
            //bounds check
            if (  x < 0 || voxels.GetLength(0) < x
               || y < 0 || voxels.GetLength(1) < y
               || z < 0 || voxels.GetLength(2) < z)
            {
                return -1;
            }

            int nextLabel = label;
            if (labels[x,y,z] == 0 && voxels[x,y,z] > threshold)
            {
                nextLabel++;
                labels[x, y, z] = label;
                foreach(int[] p in GeneratePositions(x, y, z)) {
                    GrassFire(voxels, p[0], p[1], p[2], labels, label, threshold);
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
	}
}
