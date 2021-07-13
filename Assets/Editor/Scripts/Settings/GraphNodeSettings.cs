using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class GraphNodeSettings : ScriptableObject
{
    public string graphNodeUri;

    public void Init()
    {
        graphNodeUri = "http://localhost:8000/subgraphs/name/gcbsumid/";
    }
}
