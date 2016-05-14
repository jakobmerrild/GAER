using System;
using UnityEngine;
using System.Collections;
using SharpNeat.Phenomes;

public abstract class UnitController : MonoBehaviour
{

    public event EventHandler MouseDownEvent;

    public abstract void Activate(IBlackBox box);

    public abstract void Stop();

    public abstract float GetFitness();

    protected virtual void OnMouseDown()
    {
        if (MouseDownEvent != null)
        {
            MouseDownEvent(this, null);
        }
    }

    public abstract void DeSelect();
}
