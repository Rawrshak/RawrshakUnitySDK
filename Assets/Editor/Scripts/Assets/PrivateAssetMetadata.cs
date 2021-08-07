
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

namespace Rawrshak
{
    [Serializable]
    public class PrivateAssetMetadataCommon : ScriptableObject
    {
        public AssetType type;
        public AssetSubtype subtype;
    }
}