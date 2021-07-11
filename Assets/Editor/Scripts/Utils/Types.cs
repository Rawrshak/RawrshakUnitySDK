using UnityEngine;

namespace Rawrshak {

    public enum EthereumNetwork {
        Mainnet,
        Rinkby,
        Kovan,
        Localhost
    };
    public enum SupportedBuildTargets {
        StandaloneWindows,
        StandaloneWindows64,
        Android,
        iOS,
        WebGL
    };

    // [Flags]
    public enum Role
    {
        None = 0,
        Minter = 1 << 0,
        Burner = 1 << 1
    };
}