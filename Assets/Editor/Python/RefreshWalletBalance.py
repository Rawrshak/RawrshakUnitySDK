
import arweave
import sys

from arweave.utils import ar_to_winston
import clr
from arweave.arweave_lib import ArweaveTransactionException

# This is a template for making callbacks into our C# code
clr.AddReference('Rawrshak')
import UnityEngine
from Rawrshak import ArweaveSettings
from Rawrshak import AssetBundleManager

arweaveSettings = None
settingsManager = None

assetBundleManager = UnityEngine.Object.FindObjectOfType(AssetBundleManager)

if assetBundleManager == None:
    print("Error: No Asset Bundle Manager Found.")
    quit()

arweaveSettings = assetBundleManager.mArweaveSettings

if arweaveSettings == None:
    assetBundleManager.AddErrorHelpbox("No Arweave Settings Found.")
    quit()

try:
    wallet = arweave.Wallet(arweaveSettings.arweaveWalletFile)
    wallet.api_url = arweaveSettings.arweaveGatewayUri
    print(ar_to_winston(wallet.balance))
    arweaveSettings.walletBalance = ar_to_winston(wallet.balance)
except ArweaveTransactionException as ae:
    print(ae.message)
    assetBundleManager.AddErrorHelpbox(ae.message)
except Exception as e:
    # print("Error ", e.__class__, ", [Cause]: ", e.msg)
    # print(e.__class__)
    # print(sys.exc_info())
    assetBundleManager.AddErrorHelpbox(str(sys.exc_info()[0]) + "\n" + str(sys.exc_info()[1]))
