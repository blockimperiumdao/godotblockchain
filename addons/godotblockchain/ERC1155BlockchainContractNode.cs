using System;
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using System.Collections.Generic;
using BIGConnect.addons.godotblockchain.utils;

namespace BIGConnect.addons.godotblockchain;

[GlobalClass,Tool]
public partial class ERC1155BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC1155BlockchainContractInitializedEventHandler();


	public class ERC1155TokenMetadata
	{
		public BigInteger _totalSupply;
		public BigInteger _balanceOf;
		
		public string CurrencyAddress { get; set; }
		public BigInteger MaxClaimable { get; set; }
		public byte[] MerkleRoot { get; set; }
		public BigInteger TokenPrice { get; set; }
		public BigInteger WalletLimit { get; set; }
		public BigInteger SupplyClaimed { get; set; }	
		public string ContractAddress { get; set; }
		public string CurrencyIcon { get; set; }
		
		public string CurrencySymbol { get; set; }

		public ThirdwebContract ThirdwebCurrencyContract { get; set; }
		public BigInteger CurrencyDecimals { get; set; } 
		
		public Drop_ClaimCondition ClaimConditions { get; set; }
		
	}
	
	
	public override void _Ready()
	{
		AddToGroup("Blockchain", true);
	}

    public new async void Initialize()
    {
		InternalThirdwebContract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: ContractResource.contractAddress,
			chain: ContractResource.chainId
		);


		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.ERC1155BlockchainContractInitialized);

		AddToGroup("Blockchain", true);
    }   

	public void Log( string message )
	{
		BlockchainLogManager.Instance.EmitLog("ERC1155BlockchainContractNode: " + message);
	} 
	
	public async Task<List<BigInteger>> BalanceOfBatch( string[] addresses, BigInteger[] tokenIds  )
	{
		return await InternalThirdwebContract.ERC1155_BalanceOfBatch( addresses , tokenIds );
	}	
	
	public async Task<BigInteger> TotalSupply( BigInteger tokenId  )
	{
		return await InternalThirdwebContract.ERC1155_TotalSupply( tokenId );
	}	
	
    // returns all of the NFTs owned by the current wallet for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs()
    {
        return await InternalThirdwebContract.ERC1155_GetOwnedNFTs( await BlockchainClientNode.Instance.smartWallet.GetAddress());    
    }

    // returns all of the NFTs owned by the passed wallet address for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs( string walletAddress )
    {
        return await InternalThirdwebContract.ERC1155_GetOwnedNFTs( walletAddress );    
    }    



	// public async Task<BigInteger> TotalSupply(  )
	// {
	// 	_totalSupply = await InternalThirdwebContract.ERC1155_TotalSupply();
	//
	// 	return _totalSupply;
	// }


	
    // fills the node with the metadata from the Blockchain based on the active (i.e. currently use) claim condition
    public async Task<ERC1155TokenMetadata> FetchMetadataForToken(BigInteger tokenId)
    {
	    Log("Getting claim conditions");

	    if (InternalThirdwebContract != null)
	    {
		    Log("Contract is not null");
	    }
	    else
	    {
		    Log("Contract is null");
	    }
	    var claimConditions = await InternalThirdwebContract.DropERC1155_GetActiveClaimCondition( tokenId );

	    ERC1155TokenMetadata metadata = new ERC1155TokenMetadata
	    {
		    CurrencyAddress = claimConditions.Currency,
		    MaxClaimable = claimConditions.MaxClaimableSupply,
		    MerkleRoot = claimConditions.MerkleRoot,
		    TokenPrice = claimConditions.PricePerToken,
		    WalletLimit = claimConditions.QuantityLimitPerWallet,
		    SupplyClaimed = claimConditions.SupplyClaimed,
		    ClaimConditions = claimConditions
	    };
	    
	    metadata._balanceOf = await InternalThirdwebContract.ERC1155_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress(), tokenId );	
	    metadata._totalSupply = await InternalThirdwebContract.ERC1155_TotalSupply( tokenId );

	    // check to see if the currency address is the native currency of the chain
	    // if it is, then we need to get the native currency information
	    if (string.Equals(metadata.CurrencyAddress.ToLower(), "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE".ToLower(),
		        StringComparison.Ordinal))
	    {
		    var chainData = await Utils.FetchThirdwebChainDataAsync(BlockchainClientNode.Instance.internalClient,
			    ContractResource.chainId);

		    metadata.CurrencySymbol = chainData.NativeCurrency.Symbol;
		    metadata.CurrencyDecimals = chainData.NativeCurrency.Decimals;
		    metadata.CurrencyIcon = chainData.Icon.Url;
	    }
	    else
	    {
		    // get information about the currency required to claim the token
		    metadata.ThirdwebCurrencyContract = await ThirdwebContract.Create(
			    client: BlockchainClientNode.Instance.internalClient,
			    address: metadata.CurrencyAddress,
			    chain: ContractResource.chainId
		    );
		    metadata.CurrencySymbol = await metadata.ThirdwebCurrencyContract.ERC20_Symbol();

		    metadata.CurrencyDecimals = await metadata.ThirdwebCurrencyContract.ERC20_Decimals();
	    }

	    Log("Claim conditions: " + metadata.CurrencyAddress + " MaxClaimable: " + metadata.MaxClaimable + " TokenPrice: " + metadata.TokenPrice + "("+ metadata.CurrencySymbol +") WalletLimit: " + metadata.WalletLimit + " SupplyClaimed: " + metadata.SupplyClaimed );
	
	    return metadata;
    }       

	public async Task<ThirdwebTransactionReceipt> Claim( BigInteger tokenId, BigInteger quantity )
	{
		return await InternalThirdwebContract.DropERC1155_Claim( BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), tokenId, quantity );
	}	

	public async Task<ThirdwebTransactionReceipt> Transfer( string fromAddress, string toAddress, BigInteger tokenId, BigInteger amount )
	{
		return await InternalThirdwebContract.ERC1155_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, tokenId, amount, new byte[] {} );
	}

    public async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient);
    }

  public async Task<Sprite2D> GetNFTAsSprite2D( BigInteger nftId  )
    {
        NFT nft = await InternalThirdwebContract.ERC1155_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public async Task<ImageTexture> GetNFTAsTexture( BigInteger nftId )
    {
        NFT nft = await InternalThirdwebContract.ERC1155_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( BigInteger nftId )
    {
        NFT nft = await InternalThirdwebContract.ERC1155_GetNFT( nftId );

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
        NFT nft = await InternalThirdwebContract.ERC1155_GetNFT( nftId );
        
        return await GetNFTAsAudioStreamMP3(nft);
    }

    public async Task<byte[]> GetNFTAsByteArray(int nftId)
    {
        NFT nft = await InternalThirdwebContract.ERC1155_GetNFT( nftId );

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