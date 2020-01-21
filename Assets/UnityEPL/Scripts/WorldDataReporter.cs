﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("UnityEPL/Reporters/World Data Reporter")]
public class WorldDataReporter : DataReporter
{

    public bool reportView = true;

    public bool isStatic = true;
    public bool doSpawnReport = true;

    public int framesPerReport = 60;

    private int offset;
    BoxCollider objectCollider;

    void Awake() {
        offset = (int)Random.Range(0, framesPerReport / 2);
    }

    void Update()
    {
        if (!isStatic) CheckTransformReport();
    }

    void BoxCheck()
    {
        if (reportView && GetComponent<BoxCollider>() == null)
        {
            reportView = false;
            throw new UnityException("You have selected enter/exit viewfield reporting for " + gameObject.name + " but there is no box collider on the object." +
                                      "  This feature uses collision detection to compare with camera bounds and other objects.  Please add a collider or " +
                                      "unselect viewfield enter/exit reporting.");
        }
        objectCollider = gameObject.GetComponent<BoxCollider>();
    }

    protected override void OnEnable() {
        base.OnEnable();
        BoxCheck();
        if(doSpawnReport)
            DoSpawnReport();
    }

    protected override void OnDisable() {
        if(doSpawnReport)
            DoDespawnReport();
    }

    // TODO: gather data in single function, use wrapper to set event type

    public void DoTransformReport(System.Collections.Generic.Dictionary<string, object> extraData)
    {
        System.Collections.Generic.Dictionary<string, object> transformDict = new System.Collections.Generic.Dictionary<string, object>(extraData);
        transformDict.Add("positionX", xform.position.x);
        transformDict.Add("positionY", xform.position.y);
        transformDict.Add("positionZ", xform.position.z);

        transformDict.Add("rotationX", xform.rotation.eulerAngles.x);
        transformDict.Add("rotationY", xform.rotation.eulerAngles.y);
        transformDict.Add("rotationZ", xform.rotation.eulerAngles.z);

        transformDict.Add("reportID", reportingID);
        transformDict.Add("objectName", gameObject.name);
        eventQueue.Enqueue(new DataPoint(gameObject.name + "Transform", RealWorldFrameDisplayTime(), transformDict));
    }

    public void DoTransformReport()
    {
        System.Collections.Generic.Dictionary<string, object> transformDict = new System.Collections.Generic.Dictionary<string, object>();
        transformDict.Add("positionX", xform.position.x);
        transformDict.Add("positionY", xform.position.y);
        transformDict.Add("positionZ", xform.position.z);

        transformDict.Add("rotationX", xform.rotation.eulerAngles.x);
        transformDict.Add("rotationY", xform.rotation.eulerAngles.y);
        transformDict.Add("rotationZ", xform.rotation.eulerAngles.z);

        transformDict.Add("reportID", reportingID);
        transformDict.Add("objectName", gameObject.name);
        eventQueue.Enqueue(new DataPoint(gameObject.name + "Transform", RealWorldFrameDisplayTime(), transformDict));
    }

    private void CheckTransformReport()
    {
        if ((Time.frameCount + offset) % framesPerReport == 0)
        {
            DoTransformReport();
        }
    }

    private void DoSpawnReport() {
        System.Collections.Generic.Dictionary<string, object> transformDict = new System.Collections.Generic.Dictionary<string, object>();
        transformDict.Add("positionX", xform.position.x);
        transformDict.Add("positionY", xform.position.y);
        transformDict.Add("positionZ", xform.position.z);

        transformDict.Add("rotationX", xform.rotation.eulerAngles.x);
        transformDict.Add("rotationY", xform.rotation.eulerAngles.y);
        transformDict.Add("rotationZ", xform.rotation.eulerAngles.z);

        transformDict.Add("reportID", reportingID);
        transformDict.Add("objectName", gameObject.name);
        eventQueue.Enqueue(new DataPoint(gameObject.name + "Spawn", RealWorldFrameDisplayTime(), transformDict));
    }
    
    private void DoDespawnReport() {
        System.Collections.Generic.Dictionary<string, object> transformDict = new System.Collections.Generic.Dictionary<string, object>();
        transformDict.Add("reportID", reportingID);
        transformDict.Add("objectName", gameObject.name);
        eventQueue.Enqueue(new DataPoint(gameObject.name + "Despawn", RealWorldFrameDisplayTime(), transformDict));
    }
}