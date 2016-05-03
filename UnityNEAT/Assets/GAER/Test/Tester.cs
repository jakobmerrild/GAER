using System;
using UnityEngine;
using System.Collections;

public class Tester {

    //margin between population members
    readonly static int margin = 5;

    public static IEnumerator Testpopulation(GameObject[] population, float width, float height, float length)
    {
        Vector3[] initPos = new Vector3[population.Length];

        //used for placement of pop members
        float mod = Mathf.Sqrt(population.Length);
        Debug.Log("GridSize " + mod);

        //Surface to place objects on
        GameObject plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.position = new Vector3(0, -(height / 2), 0);
        plane.transform.localScale = new Vector3(width * mod, 1, height * mod);
        plane.GetComponent<MeshCollider>().material = new PhysicMaterial();
        
        int iter = 0;
        for (int i = 0; i < population.Length; i++)
        {
            if (i != 0 && i % mod == 0) iter++;

            //Center mesh and 
            population[i].transform.localPosition = new Vector3(    -width / 2 + ((width + margin) * (i % mod)),
                                                                    -height / 2,
                                                                    -length / 2 + ((length + margin) * iter)
                                                       );

            initPos[i] = population[i].transform.localEulerAngles;

            createDropObject(population[i].transform.position, width, height, length);
        }

        yield return new WaitForSeconds(5);

        float[] fit = new float[population.Length];
        String formatted = "Results:\n";
        for (int i = 0; i < population.Length; i++)
        {
            fit[i] = evaluate(initPos[i], population[i].transform.localEulerAngles);
            formatted = String.Concat(formatted, fit[i]+"\n");
        }
        Debug.Log(formatted);

    }

    static GameObject createDropObject(Vector3 dropPoint, float width, float height, float length)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(5, 5, 5);
        sphere.transform.localPosition = new Vector3(dropPoint.x + width / 2, dropPoint.y + height / 2 + 50, dropPoint.z + length / 2);
        sphere.AddComponent<Rigidbody>();

        return sphere;
    }

    static float evaluate(Vector3 before, Vector3 after)
    {
        return Mathf.Abs(
                (before.x - after.x) +
                (before.y - after.y) +
                (before.z - after.z)
            );
    }
}
