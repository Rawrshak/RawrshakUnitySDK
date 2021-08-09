import arweave
import sys

import clr
from arweave.arweave_lib import Wallet, Transaction, ArweaveTransactionException
from arweave.transaction_uploader import TransactionUploaderException, get_uploader

# This is a template for making callbacks into our C# code
clr.AddReference('Rawrshak')
import UnityEngine
from Rawrshak import UploadManager

# Note:
# SIGPIPE Fails in transaction_uploader so I went into transaction_uploader.py
# and replaced SIGPIPE with a try catch
# Library\PythonInstall\Lib\site-packages\arweave\transaction_uploader.py

# try:
#     from signal import signal, SIGPIPE, SIG_DFL
#     signal(SIGPIPE, SIG_DFL)
# except ImportError:  # If SIGPIPE is not available (win32),
#     pass 

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

    filename = menu.mUploadManager.bundleForUpload.mName
    fileLocation = menu.mUploadManager.bundleForUpload.mFileLocation

    print(filename)
    print(fileLocation)
    print("Wallet API: " + wallet.api_url)
    
    #####################################################
    # This Works                                        #
    #####################################################
    # with open(fileLocation + ".meta", 'rb') as fileHandler:
    #     bytes_data = fileHandler.read()
        
    #     print("Creating a transaction..")
    #     transaction = Transaction(wallet, data=bytes_data)
    #     transaction.add_tag('Content-Type', 'application/octet-stream')
    #     print("Signing..")
    #     print("Transaction URL: " + transaction.api_url)
    #     transaction.sign()
    #     transaction.send()

    #     arweaveSettings.bundleForUpload.mTransactionId = transaction.id
    #     arweaveSettings.bundleForUpload.mUri = transaction.api_url + "/" + transaction.id

    #     print("TX ID: " + transaction.id)
    #     status = transaction.get_status()
    #     print("Status: " + status)
    #####################################################

    with open(fileLocation, "rb", buffering=0) as fileHandler:
        tx = arweave.Transaction(wallet, file_handler=fileHandler, file_path=fileLocation)
        # tx.api_url = arweaveSettings.arweaveGatewayUri
        tx.add_tag('Content-Type', 'application/octet-stream')
        tx.sign()
        
        uploader = get_uploader(tx, fileHandler)

        while not uploader.is_complete:
            uploader.upload_chunk()

            print("{}% complete, {}/{}".format(
                uploader.pct_complete, uploader.uploaded_chunks, uploader.total_chunks
            ))

        status = tx.get_status()
        if status == "PENDING":
            menu.mUploadManager.bundleForUpload.mStatus = "Uploading"
        else:
            menu.mUploadManager.bundleForUpload.mStatus = "Uploaded"
            menu.mUploadManager.bundleForUpload.mNumOfConfirmations = status['number_of_confirmations']
        
        menu.mUploadManager.bundleForUpload.mTransactionId = tx.id
        menu.mUploadManager.bundleForUpload.mUri = tx.api_url + "/" + tx.id

except TransactionUploaderException as tue:
    print(tue.args)
    menu.AddErrorHelpbox(tue.args)
except ArweaveTransactionException as ae:
    print(ae.args)
    menu.AddErrorHelpbox(ae.args)
except Exception as e:
    print(e.args)
    menu.AddErrorHelpbox(e.args)
    # assetBundleManager.AddErrorHelpbox(str(sys.exc_info()[0]) + "\n" + str(sys.exc_info()[1]))

