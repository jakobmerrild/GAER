using Assets.GAER.Physics;
using UnityEngine;
using GAER;


public class PhysicsTester {

    static GameObject createDropObject(Vector3 dropPoint)
    {
        GameObject ragdoll = GameObject.Instantiate(Resources.Load("EthanRagdoll")) as GameObject;
        //ragdoll.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        ragdoll.transform.localPosition = new Vector3(dropPoint.x + TestExperiment.Width / 2, dropPoint.y + TestExperiment.Height+1, dropPoint.z + TestExperiment.Length / 2 + 4);
        ragdoll.AddComponent<StopSphereFromFalling>();
        var rb = ragdoll.GetComponents<Rigidbody>();

        foreach (var rib in rb)
        {
            rib.useGravity = false;
        }

        return ragdoll;
    }

    public class BallDropExperiment
    {
        public GameObject ball;
        public GameObject toDestroy;
        public GameObject obj;
        public GameObject head;
        public Vector3 ballPosition;
        public Vector3 headPosition;
        public Quaternion objRotation;

        public BallDropExperiment(GameObject ball, GameObject head, GameObject obj, GameObject toDestroy)
        {
            this.ball = ball;
            this.toDestroy = toDestroy;
            this.obj = obj;
            this.head = head;

            this.headPosition = head.transform.position;
            this.ballPosition = ball.transform.position;
            this.objRotation = obj.transform.rotation;
        }
    }

    public class BallDropResults
    {
        public float ballTravelled;
        public float objRotation;
        public float ballRestHeight;
        public float headOverHips;

        public BallDropResults(float ballTravelled, float headHeightOverHips, float objRotation, float ballRestHeight)
        {
            this.ballTravelled = ballTravelled;
            this.objRotation = objRotation;
            this.ballRestHeight = ballRestHeight;
            this.headOverHips = headHeightOverHips;
        }

        public override string ToString()
        {
            return string.Format("ballTravel: {0}\nChairRotation: {1}", ballTravelled, objRotation);
        }
    }

    public static BallDropExperiment StartBallDropExperiment(GameObject obj)
    {
        Transform objTrans = obj.transform;

        GameObject dropObj = createDropObject(objTrans.position);

        var hips = GameObject.Find("EthanSkeleton/EthanHips");
        var head = GameObject.Find("EthanSkeleton/EthanHips/EthanSpine/EthanSpine1/EthanSpine2/EthanNeck/EthanHead");
        BallDropExperiment bs = new BallDropExperiment(hips, head, obj, dropObj);

        var rb = dropObj.GetComponents<Rigidbody>();

        foreach (var rib in rb)
        {
            rib.useGravity = true;
        }
        return bs;
    }

    public static BallDropResults MeassureBallDropExperiment(BallDropExperiment exp)
    {
        Vector3 newBallPosition = exp.ball.transform.position;
        Quaternion newObjRotation = exp.obj.transform.rotation;
        Vector3 newHeadPosition = exp.head.transform.position;

        Vector3 oldBallPosition = exp.ballPosition;
        Quaternion oldObjRotation = exp.objRotation;        

        //ball travel distanve --- we ignore changes in height
        float x = oldBallPosition.x;
        float z = oldBallPosition.z;
        float _x = newBallPosition.x;
        float _z = newBallPosition.z;
        float ballTravelDistance = Mathf.Sqrt(Mathf.Pow(x - _x,2) + Mathf.Pow(z -_z,2));

        float headHeightOverHips = newHeadPosition.y - newBallPosition.y;

        float ballHeight = newBallPosition.y;

        float objRotationDegrees = Quaternion.Angle(newObjRotation, oldObjRotation);

        return new BallDropResults(ballTravelDistance, headHeightOverHips, objRotationDegrees, ballHeight);
    }

}
