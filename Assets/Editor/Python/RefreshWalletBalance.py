
import arweave
import sys

from arweave.utils import ar_to_winston
import clr
from arweave.arweave_lib import ArweaveTransactionException

# This is a template for making callbacks into our C# code
clr.AddReference('Rawrshak')
import UnityEngine
from Rawrshak import AssetBundleMenu

all_asset_bundle_menus = UnityEngine.Resources.FindObjectsOfTypeAll(AssetBundleMenu)

if all_asset_bundle_menus.Length == 0:
    print("Upload Manager was not found.")
    quit()

menu = all_asset_bundle_menus[0]
uploadConfig = menu.mUploadManager.mConfig

if uploadConfig == None:
    menu.AddErrorHelpbox("No Upload Config Found.")
    print("No Upload Config found.")
    quit()

try:
    wallet = arweave.Wallet(uploadConfig.walletFile)
    wallet.api_url = uploadConfig.gatewayUri

    print("Balance: " + str(wallet.balance))
    uploadConfig.walletBalance = str(wallet.balance)
    
    menu.ClearHelpbox()
except ArweaveTransactionException as ae:
    print(ae.message)
    menu.AddErrorHelpbox(ae.message)
except Exception as e:
    # print("Error ", e.__class__, ", [Cause]: ", e.msg)
    # print(e.__class__)
    # print(sys.exc_info())
    menu.AddErrorHelpbox(str(sys.exc_info()[0]) + "\n" + str(sys.exc_info()[1]))
