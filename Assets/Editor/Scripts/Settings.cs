using UnityEngine;
using UnityEditor;

// Todo: Serialize and load from json file
[CreateAssetMenu(fileName = "RawrshakSettings", menuName = "ScriptableObjects/RawrshakSettings", order = 1)]
public class Settings : ScriptableObject {

    public enum EthereumNetwork {
        MAINNET,
        RINKBY,
        KOVAN,
        CUSTOM
    };

    public string assetBundleFolder;
    public string graphNodeUri;
    public string ethereumGatewayUri;
    public EthereumNetwork networkId;
    public int defaultGasPrice;
    public int chainId;
    public int port;
    public string arweaveGatewayUri;
    public string arweaveWalletFile;

    public void InitData() {
        ethereumGatewayUri = "http://127.0.0.1/";
        networkId = EthereumNetwork.CUSTOM;
        defaultGasPrice = 20;
        chainId = 5777;
        port = 7545;
        graphNodeUri = "http://localhost:8000/subgraphs/name/gcbsumid/";
        arweaveGatewayUri = "http://arweave.net";
        arweaveWalletFile = "/Asset/WalletFile";
        assetBundleFolder = "/Asset/AssetBundles";

        // networks = new RawrshakSettings.EthereumNetwork[5];
        // networks[0].networkIdName = "Mainnet";
        // networks[0].networkId = 1;
        // networks[0].chainId = 1;
        // networks[0].networkIdEnum = RawrshakSettings.NetworkId.MAINNET;
        
        // networks[1].networkIdName = "Ropsten";
        // networks[1].networkId = 3;
        // networks[1].chainId = 3;
        // networks[1].networkIdEnum = RawrshakSettings.NetworkId.ROPSTEN;
        
        // networks[2].networkIdName = "Rinkby";
        // networks[2].networkId = 4;
        // networks[2].chainId = 4;
        // networks[2].networkIdEnum = RawrshakSettings.NetworkId.RINKBY;
        
        // networks[3].networkIdName = "Kovan";
        // networks[3].networkId = 12;
        // networks[3].chainId = 12;
        // networks[3].networkIdEnum = RawrshakSettings.NetworkId.KOVAN;
        
        // networks[4].networkIdName = "Custom";
        // networks[4].networkId = 0;
        // networks[4].chainId = 0;
        // networks[4].networkIdEnum = RawrshakSettings.NetworkId.CUSTOM;
    }
}