﻿using Assets.GAER.Physics;
using UnityEngine;
using GAER;
using JetBrains.Annotations;

public class PhysicsTester {

    static GameObject createDropObject(Vector3 dropPoint)
    {

        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.AddComponent<StopSphereFromFalling>();

        sphere.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        sphere.transform.localPosition = dropPoint;
        Rigidbody rb = sphere.AddComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = 2;

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
        public float ballRestHeight;

        public BallDropResults(float ballTravelled, float objRotation, float ballRestHeight)
        {
            this.ballTravelled = ballTravelled;
            this.objRotation = objRotation;
            this.ballRestHeight = ballRestHeight;
        }

        public override string ToString()
        {
            return string.Format("ballTravel: {0}\nChairRotation: {1}", ballTravelled, objRotation);
        }
    }

    public static BallDropExperiment StartBallDropExperiment(GameObject obj)
    {
        Transform objTrans = obj.transform;
        GameObject dropObj = createDropObject(objTrans.position + new Vector3(TestExperiment.Width/2, TestExperiment.Height+5, TestExperiment.Length/2));
        BallDropExperiment bs = new BallDropExperiment(dropObj, obj);
        dropObj.GetComponent<Rigidbody>().useGravity = true;
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

        float ballHeight = newBallPosition.y;

        float objRotationDegrees = Quaternion.Angle(newObjRotation, oldObjRotation);

        return new BallDropResults(ballTravelDistance, objRotationDegrees, ballHeight);
    }

}
