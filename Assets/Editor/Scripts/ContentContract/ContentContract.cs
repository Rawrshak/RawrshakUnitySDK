
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

namespace Rawrshak
{
    public class ContentContract : ScriptableObject
    {
        public string mName;
        public string mSymbol;
        public string mContractAddress;
        public string mMetadataUri;
        public string mDescription;
        public string mDeveloperName;
        public string mDeveloperAddress;
        public string mContractDeploymentDate;
        public string mImageUri;
        public bool isDeployed;
        public List<ContractRoyalties> mContractRoyalties;
        public List<WalletWithRoles> mDevWallets;

        public void Init(string developerName, string developerAddress)
        {
            mName = "Content Contract Name";
            mSymbol = "CONT";
            mContractAddress = "0x...";
            mMetadataUri = "...";
            mDescription = "Content Contract Description";
            mDeveloperName = developerName;
            mDeveloperAddress = developerAddress;
            mContractDeploymentDate = DateTime.MinValue.ToString();
            mImageUri = "...";
            isDeployed = false;
            mContractRoyalties = new List<ContractRoyalties>();
            mDevWallets = new List<WalletWithRoles>();
        }
    }

    [Serializable]
    public class ContractRoyalties 
    {
        public string mAddress;
        public float mRoyalty;
    }

    [Serializable]
    public class WalletWithRoles
    {
        public string mAddress;
        [EnumFlag]
        public Role mRole;
    }
}