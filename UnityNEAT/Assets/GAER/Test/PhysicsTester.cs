using UnityEngine;

public class PhysicsTester {

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
