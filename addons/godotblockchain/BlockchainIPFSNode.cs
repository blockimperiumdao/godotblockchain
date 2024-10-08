using Godot;
using Thirdweb;



namespace GodotBlockchain.addons.godotblockchain;

[GlobalClass,Tool]

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

	public override void _Ready()
	{
		AddToGroup("Blockchain", true);
	}

	public void Initialize()
	{
		// initially try to perform a download
		//Download();
	}

	public async void Upload()
	{
		if ( uploadDataBytes != null )
		{
			IPFSUploadResult result = await ThirdwebStorage.UploadRaw(BlockchainClientNode.Instance.internalClient, uploadDataBytes);
			ipfsURI = result.IpfsHash;

			BlockchainLogManager.Instance.EmitLog("Uploaded to IPFS: " + ipfsURI);
			BlockchainLogManager.Instance.EmitLog("Preview available at" + result.PreviewUrl);

			EmitSignal(SignalName.BlockchainIPFSUploadCompleted);			
		}
		else if (localFilePath == "")
		{
			BlockchainLogManager.Instance.EmitLog("Set the localPath of this node");
		}
		else
		{
			IPFSUploadResult result = await ThirdwebStorage.Upload(BlockchainClientNode.Instance.internalClient, localFilePath);
			ipfsURI = result.IpfsHash;

			BlockchainLogManager.Instance.EmitLog("Uploaded to IPFS: " + ipfsURI);
			BlockchainLogManager.Instance.EmitLog("Preview available at" + result.PreviewUrl);

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
				BlockchainLogManager.Instance.EmitLog("Set the ipfsURI of this node or create a BlockchainIPFSResource and assign its upfsURI property");
			}
			else
			{
				downloadedData = await ThirdwebStorage.Download<string>(BlockchainClientNode.Instance.internalClient, ipfsURI);		
			}		
		}
		else 
		{
			if ( ipfsContractResource.ipfsURI == "" )
			{
				BlockchainLogManager.Instance.EmitLog("Create a BlockchainIPFSResource and assign its upfsURI property");
			}
			else
			{
				ipfsURI = ipfsContractResource.ipfsURI;
				downloadedData = await ThirdwebStorage.Download<string>(BlockchainClientNode.Instance.internalClient, ipfsURI);		
			}
		}

		// emit a signal so systems will know that we are done downloading
		EmitSignal(SignalName.BlockchainIPFSDownloadCompleted);
	}     

}
