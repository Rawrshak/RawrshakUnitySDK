## RawrshakUnitySDK
Unity SDK for the Rawrshak Platform

# Versions in Use
Unity: 2020.3.12f1
Nethereum: 3.8.0, net461AOT
graphQL-client-unity: https://github.com/Gazuntype/graphQL-client-unity

Note: Asset bundles are backward compatible but not forward compatible.

# Todo:
1. Rawrshak Menu
    - Wallet Tab
        - Import private key or connect via WalletConnect
    - Settings Tab
        - Ability to set all Rawrshak Setting requirements
        - This includes connection settings:
            - Rawrshak Subgraph
            - Ethereum (mainnet, testnets, or subnets)
            - Arweave (connection and wallet)
    - Content Contract Tab
        - deploy a content contract
        - manage wallet roles
        - select content contract in use
    - Asset Viewer Tab
        - list all assets in content contract
        - set up new assets and deploy to content contract
        - upload metadata to permanent storage (Arweave)
        - Verify assets are loadable before uploading
    - AssetBundle Tab
        - Generate asset bundles
        - Upload asset bundles to arweave
        - Verify Asset bundles usable
2. Install the following:
    - Nethereum
    - Python
    - Arweave Python Client
    - GraphQL
    - WalletConnect


