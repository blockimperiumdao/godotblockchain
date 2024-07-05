#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;


public partial class BlockchainIPFSNode : Node
{
	[Signal]
	public delegate void BlockchainIPFSDownloadCompletedEventHandler();

	[Signal]
	public delegate void BlockchainIPFSUploadCompletedEventHandler();

	[Export]
	public BlockchainIPFSResource ipfsContractResource { get; internal set; }

	[Export]
	public string ipfsURI;

	[Export]
	public string localFilePath;

	public byte[] uploadDataBytes;

	private string downloadedData;

	public void Initialize()
	{
		// initially try to perform a download
		Download();
	}

	public async void Upload()
	{
		if ( uploadDataBytes != null )
		{
			IPFSUploadResult result = await ThirdwebStorage.UploadRaw(BlockchainManager.Instance.internalClient, uploadDataBytes);
			ipfsURI = result.IpfsHash;

			BlockchainManager.Instance.EmitLog("Uploaded to IPFS: " + ipfsURI);
			BlockchainManager.Instance.EmitLog("Preview available at" + result.PreviewUrl);

			EmitSignal(SignalName.BlockchainIPFSUploadCompleted);			
		}
		else if (localFilePath == "")
		{
			BlockchainManager.Instance.EmitLog("Set the localPath of this node");
		}
		else
		{
			IPFSUploadResult result = await ThirdwebStorage.Upload(BlockchainManager.Instance.internalClient, localFilePath);
			ipfsURI = result.IpfsHash;

			BlockchainManager.Instance.EmitLog("Uploaded to IPFS: " + ipfsURI);
			BlockchainManager.Instance.EmitLog("Preview available at" + result.PreviewUrl);

			// emit a signal so systems will know that we are done uploading
			EmitSignal(SignalName.BlockchainIPFSUploadCompleted);			
		}
	}
	
	public async void Download()
	{
		// this is a no-op
		if (( ipfsContractResource == null ) && ( ipfsURI == "" ))
		{
			return;
		}

		// if we don't have a contract resource, try to download from the ipfsURI
		//
		if ( ipfsContractResource == null )
		{
			if ( ipfsURI == "" )
			{
				BlockchainManager.Instance.EmitLog("Set the ipfsURI of this node or create a BlockchainIPFSResource and assign its upfsURI property");
			}
			else
			{
				downloadedData = await ThirdwebStorage.Download<string>(BlockchainManager.Instance.internalClient, ipfsURI);		
			}		
		}
		else 
		{
			if ( ipfsContractResource.ipfsURI == "" )
			{
				BlockchainManager.Instance.EmitLog("Create a BlockchainIPFSResource and assign its upfsURI property");
			}
			else
			{
				ipfsURI = ipfsContractResource.ipfsURI;
				downloadedData = await ThirdwebStorage.Download<string>(BlockchainManager.Instance.internalClient, ipfsURI);		
			}
		}

		// emit a signal so systems will know that we are done downloading
		EmitSignal(SignalName.BlockchainIPFSDownloadCompleted);
	}     

}

#endif
