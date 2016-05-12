using UnityEditor.VersionControl;
using UnityEngine;

public class PhysicsTester {

    static GameObject createDropObject(Vector3 dropPoint, float width, float height, float length)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = new Vector3(5, 5, 5);
        sphere.transform.localPosition = new Vector3(dropPoint.x + width / 2, dropPoint.y + height / 2 + 50, dropPoint.z + length / 2);
        sphere.AddComponent<Rigidbody>().useGravity=false;

        return sphere;
    }

    static GameObject createDropObject2(Vector3 dropPoint, float width, float height, float length)
    {
        GameObject sphere = GameObject.Instantiate(Resources.Load("EthanRagdoll")) as GameObject;
        sphere.transform.localScale = new Vector3(10, 10, 10);
        sphere.transform.localPosition = new Vector3(dropPoint.x + width / 2, dropPoint.y + height / 2 + 25, dropPoint.z + length / 2);

        return sphere;
    }

    public class BallDropExperiment
    {
        public GameObject ball;
        public GameObject obj;
        public Vector3 ballPosition;
        public Quaternion objRotation;

        public BallDropExperiment(GameObject ball, GameObject obj)
        {
            this.ball = ball;
            this.obj = obj;

            this.ballPosition = ball.transform.position;
            this.objRotation = obj.transform.rotation;
        }
    }

    public class BallDropResults
    {
        public float ballTravelled;
        public float objRotation;

        public BallDropResults(float ballTravelled, float objRotation)
        {
            this.ballTravelled = ballTravelled;
            this.objRotation = objRotation;
        }

        public override string ToString()
        {
            return string.Format("ballTravel: {0}\nChairRotation: {1}", ballTravelled, objRotation);
        }
    }

    public static BallDropExperiment StartBallDropExperiment(GameObject obj)
    {
        Transform objTrans = obj.transform;
        GameObject dropObj = createDropObject2(objTrans.position + new Vector3(0, 5, 0), 10, 0, 10);
        BallDropExperiment bs = new BallDropExperiment(dropObj, obj);
        //dropObj.GetComponent<Rigidbody>().useGravity = true;
        return bs;
    }

    public static BallDropResults MeassureBallDropExperiment(BallDropExperiment exp)
    {
        Vector3 newBallPosition = exp.ball.transform.position;
        Quaternion newObjRotation = exp.obj.transform.rotation;

        Vector3 oldBallPosition = exp.ballPosition;
        Quaternion oldObjRotation = exp.objRotation;

        //ball travel distanve --- we ignore changes in height
        float x = oldBallPosition.x;
        float z = oldBallPosition.z;
        float _x = newBallPosition.x;
        float _z = newBallPosition.z;
        float ballTravelDistance = Mathf.Sqrt(Mathf.Pow(x - _x,2) + Mathf.Pow(z -_z,2));

        float objRotationDegrees = Quaternion.Angle(newObjRotation, oldObjRotation);

        return new BallDropResults(ballTravelDistance, objRotationDegrees);
    }

}
