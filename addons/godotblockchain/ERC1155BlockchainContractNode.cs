using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using System.Collections.Generic;


[GlobalClass,Tool]
public partial class ERC1155BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC1155BlockchainContractInitializedEventHandler();

	public BigInteger totalSupply;
	public BigInteger balanceOf;
	public List<BigInteger> balancesOf;

    public string currencyAddress { get; private set; }
    public BigInteger maxClaimable { get; private set; }
    public byte[] merkleRoot { get; private set; }
    public BigInteger tokenPrice { get; private set; }
    public BigInteger walletLimit { get; private set; }
    public BigInteger supplyClaimed { get; private set; }

	public override void _Ready()
	{
		AddToGroup("Blockchain", true);

	}

    public new async void Initialize()
    {
		contract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: contractResource.contractAddress,
			chain: contractResource.chainId
		);

		//TODO - we should pull back the metadata for all the tokens
		FetchMetadata(0);

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.ERC1155BlockchainContractInitialized);

		AddToGroup("Blockchain", true);
    }   

	public void Log( string message )
	{
		//EmitSignal(SignalName.ClientLogMessage, "ERC1155BlockchainContractNode: " + message );
		BlockchainLogManager.Instance.EmitLog("ERC1155BlockchainContractNode: " + message);
	} 

	public async Task<BigInteger> BalanceOf( BigInteger tokenId  )
	{
		balanceOf = await contract.ERC1155_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress(), tokenId );
		return balanceOf;
	}

    // returns all of the NFTs owned by the current wallet for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs()
    {
        return await contract.ERC1155_GetOwnedNFTs( await BlockchainClientNode.Instance.smartWallet.GetAddress());    
    }

    // returns all of the NFTs owned by the passed wallet address for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs( string walletAddress )
    {
        return await contract.ERC1155_GetOwnedNFTs( walletAddress );    
    }    

	public async Task<List<BigInteger>> BalanceOfBatch( string[] addresses, BigInteger[] tokenIds  )
	{
		balancesOf = await contract.ERC1155_BalanceOfBatch( addresses , tokenIds );
		return balancesOf;
	}

	public async Task<BigInteger> TotalSupply(  )
	{
		totalSupply = await contract.ERC1155_TotalSupply();

		return totalSupply;
	}

	public async Task<BigInteger> TotalSupply( BigInteger tokenId  )
	{
		totalSupply = await contract.ERC1155_TotalSupply( tokenId );

		return totalSupply;
	}


    // get the metadata of the token based on the current claim conditions
    public async void FetchMetadata( BigInteger tokenId )
    {
        var claimConditions = await contract.DropERC1155_GetActiveClaimCondition( tokenId );

        currencyAddress = claimConditions.Currency;
        maxClaimable = claimConditions.MaxClaimableSupply;
        merkleRoot = claimConditions.MerkleRoot;
        tokenPrice = claimConditions.PricePerToken;
        walletLimit = claimConditions.QuantityLimitPerWallet;
        supplyClaimed = claimConditions.SupplyClaimed;
    }

	public async Task<ThirdwebTransactionReceipt> Claim( BigInteger tokenId, BigInteger quantity )
	{
		return await contract.DropERC1155_Claim( BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), tokenId, quantity );
	}	

	public async Task<ThirdwebTransactionReceipt> Transfer( string fromAddress, string toAddress, BigInteger tokenId, BigInteger amount )
	{
		return await contract.ERC1155_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, tokenId, amount, new byte[] {} );
	}

    public async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient);
    }

  public async Task<Sprite2D> GetNFTAsSprite2D( BigInteger nftId  )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public async Task<ImageTexture> GetNFTAsTexture( BigInteger nftId )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( BigInteger nftId )
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsStandardMaterial3D( nft );
    }

    public async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( NFT nft )
    {
        ImageTexture texture = await GetNFTAsTexture( nft );

        StandardMaterial3D material = new StandardMaterial3D();
        material.AlbedoTexture = texture;

        return material;
    }

    public async Task<Sprite2D> GetNFTAsSprite2D( NFT nft )
    {
        ImageTexture texture = await GetNFTAsTexture( nft );
        
        Sprite2D sprite = new Sprite2D();
        sprite.Texture = texture;

        return sprite;        
    }

    public async Task<ImageTexture> GetNFTAsTexture( NFT nft )
    {
        byte[] nftImageBytes = await nft.GetNFTImageBytes(BlockchainClientNode.Instance.internalClient);

        StreamPeerBuffer stream = new StreamPeerBuffer();
        stream.DataArray = nftImageBytes;

        Image image = new Image();

        if ( TokenUtils.GetFileType(nftImageBytes) == TokenUtils.PNG )
        {
            image.LoadPngFromBuffer(nftImageBytes);
        }
        else if ( TokenUtils.GetFileType(nftImageBytes) == TokenUtils.JPEG )
        {
            image.LoadJpgFromBuffer(nftImageBytes);
        }
        else
        {
            Log("Unknown or unsupported media type for NFT image - returning empty sprite");
            return new ImageTexture();
        }

        ImageTexture texture = new ImageTexture();
        texture.SetImage(image);

        return texture;
    }

    public async Task<AudioStreamMP3> GetNFTAsAudioStreamMP3(NFT nft)
    {
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		

        Log("Getting NFT as audio stream: " + nft.Metadata.Name);
        Log("Getting NFT as audio stream: " + nft.Metadata.Description);
        Log("Getting NFT as audio stream URL: " + nft.Metadata.AnimationUrl);

        Log("Received bytes for audio: " + downloadedData.Length);

        var audioStream = new AudioStreamMP3();
        audioStream.Data = downloadedData;

        return audioStream;
    }

    public async Task<AudioStreamMP3> GetNFTAsAudioStreamMP3(int nftId)
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );
        
        return await GetNFTAsAudioStreamMP3(nft);
    }

    public async Task<byte[]> GetNFTAsByteArray(int nftId)
    {
        NFT nft = await contract.ERC1155_GetNFT( nftId );

        return await GetNFTAsByteArray(nft);
    }

    public async Task<byte[]> GetNFTAsByteArray(NFT nft)
    {
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		

        Log("Received bytes for audio: " + downloadedData.Length);

        return downloadedData;
    } 

}