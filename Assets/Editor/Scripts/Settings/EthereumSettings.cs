
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using Rawrshak;

namespace Rawrshak
{
    public class EthereumSettings : ScriptableObject
    {
        public string ethereumGatewayUri;
        public EthereumNetwork networkId;
        public int defaultGasPrice;
        public int chainId;
        public int port;
        public bool askForPasswordAtEveryTransaction;
        public void Init()
        {
            ethereumGatewayUri = "http://localhost";
            networkId = EthereumNetwork.Localhost;
            defaultGasPrice = 20;
            chainId = 5777;
            port = 8545;
            askForPasswordAtEveryTransaction = true;
        }

        public string GetEthereumUrl()
        {
            return String.Format("{0}:{1}", ethereumGatewayUri, port);
        }
    }
}
