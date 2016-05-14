using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.GAER.Physics
{
    class StopSphereFromFalling : MonoBehaviour
    {
        void FixedUpdate()
        {
            if (transform.position.y < 0.0)
            {
                var rb = GetComponent<Rigidbody>();
                rb.useGravity = false;
                var x = rb.velocity.x;
                var z = rb.velocity.z;
                rb.velocity = new Vector3(x, 0, z);
                
            }
        }
    }
}
