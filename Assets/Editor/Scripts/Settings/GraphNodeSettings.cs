using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

namespace Rawrshak
{  
    public class GraphNodeSettings : ScriptableObject
    {
        public string graphNodeUri;

        public void Init()
        {
            graphNodeUri = "http://localhost:8000/subgraphs/name/gcbsumid/";
        }
    }
}