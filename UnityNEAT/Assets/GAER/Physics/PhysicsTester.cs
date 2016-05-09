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

    public struct BallDropExperiment
    {
        public GameObject ball;
        public GameObject obj;
        public Transform ballTransform;
        public Transform objTransform;

        public BallDropExperiment(GameObject ball, GameObject obj)
        {
            this.ball = ball;
            this.obj = obj;

            this.ballTransform = Transform.Instantiate(ball.transform);
            this.objTransform = Transform.Instantiate(obj.transform);
        }
    }

    public struct BallDropResults
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
        GameObject dropObj = createDropObject(objTrans.position + new Vector3(0, 5, 0), 5, 5, 5);
        BallDropExperiment bs = new BallDropExperiment(dropObj, obj);
        dropObj.GetComponent<Rigidbody>().useGravity = true;
        return bs;
    }

    public static BallDropResults MeassureBallDropExperiment(BallDropExperiment exp)
    {
        Transform newBallTransform = Transform.Instantiate(exp.ball.transform);
        Transform newObjTransform = Transform.Instantiate(exp.obj.transform);

        Transform oldBallTransform = exp.ballTransform;
        Transform oldObjTransform = exp.objTransform;

        //ball travel distanve --- we ignore changes in height
        float x = oldBallTransform.position.x;
        float z = oldBallTransform.position.z;
        float _x = newBallTransform.position.x;
        float _z = newBallTransform.position.z;
        float ballTravelDistance = Mathf.Sqrt(Mathf.Pow(x - _x,2) + Mathf.Pow(z -_z,2));

        float objRotationDegrees = Quaternion.Angle(newObjTransform.rotation, oldObjTransform.rotation);

        return new BallDropResults(ballTravelDistance, objRotationDegrees);
    }

}
