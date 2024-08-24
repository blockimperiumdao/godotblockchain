using System;
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using System.Collections.Generic;

using BIGConnect.addons.godotblockchain.utils;

namespace BIGConnect.addons.godotblockchain;

// Rest of the code
[GlobalClass,Tool]
public partial class ERC721BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC721BlockchainContractInitializedEventHandler();

	public class ERC721TokenMetadata
	{
		public BigInteger TotalSupply { get; internal set; }
		public BigInteger BalanceOf { get; internal set; }

		public string CurrencySymbol { get; internal set; }
		public string TokenSymbol { get; internal set; }
		public string TokenName { get; internal set; }
		
		public string ContractAddress { get; internal set; }
		public string CurrencyAddress { get; internal set; }
		public string CurrencyIcon { get; internal set; }
		public ThirdwebContract ThirdwebCurrencyContract { get; internal set; }
		public BigInteger CurrencyDecimals { get; internal set; }
		public BigInteger MaxClaimable { get; internal set; }
		public byte[] MerkleRoot { get; internal set; }
		public BigInteger TokenPrice { get; internal set; }
		public BigInteger WalletLimit { get; internal set; }
		public BigInteger SupplyClaimed { get; internal set; }
		public Drop_ClaimCondition ClaimConditions { get; internal set; }
		public ThirdwebContract CurrencyContract { get; internal set; }
	}

	public override void _Ready()
	{
		if (ContractResource == null)
		{
			Log("ERC721 contractResource is null");
			return;
		}
		else
		{
			Log("ERC721 initializing contract " + ContractResource.contractAddress);
			Initialize();
		}

		AddToGroup("Blockchain", true);
	}

    public new async void Initialize()
    {
	    if ( ( ContractResource == null ) || ( BlockchainClientNode.Instance == null ) )
        {
            Log("contractResource or BlockchainClientNode is null");
            return;
        }

		InternalThirdwebContract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: ContractResource.contractAddress,
			chain: ContractResource.chainId
		);
		
        if (InternalThirdwebContract == null) return;

        // emit a signal so systems will know that we are ready
        //
        EmitSignal(SignalName.ERC721BlockchainContractInitialized);
    }   

	public void Log( string message )
	{
		//EmitSignal(SignalName.ClientLogMessage, "ERC721BlockchainContractNode: " + message );
		BlockchainLogManager.Instance.EmitLog("ERC721BlockchainContractNode: " + message);
	} 
	
	// fills the node with the metadata from the Blockchain based on the active (i.e. currently use) claim condition
	public async Task<ERC721TokenMetadata> FetchMetadata()
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
		var claimConditions = await InternalThirdwebContract.DropERC721_GetActiveClaimCondition( );

		ERC721TokenMetadata metadata = new ERC721TokenMetadata()
		{
			CurrencyAddress = claimConditions.Currency,
			MaxClaimable = claimConditions.MaxClaimableSupply,
			MerkleRoot = claimConditions.MerkleRoot,
			TokenPrice = claimConditions.PricePerToken,
			WalletLimit = claimConditions.QuantityLimitPerWallet,
			SupplyClaimed = claimConditions.SupplyClaimed,
			TokenName = await InternalThirdwebContract.ERC721_Name(),
			TokenSymbol = await InternalThirdwebContract.ERC721_Symbol(),
			TotalSupply =  await InternalThirdwebContract.ERC721_TotalSupply(),
			BalanceOf = await InternalThirdwebContract.ERC721_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress() ),
			ClaimConditions = claimConditions,
		};
		
		Log("Setting metadata");

		
		// check to see if the currency address is the native currency of the chain
		// if it is, then we need to get the native currency information
		if (string.Equals(metadata.CurrencyAddress.ToLower(), "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE".ToLower(),
			    StringComparison.Ordinal))
		{
			var chainData = await Utils.GetChainMetadata(BlockchainClientNode.Instance.internalClient,
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
			metadata.CurrencyContract = metadata.ThirdwebCurrencyContract;
		}

		Log("Claim conditions: " + metadata.CurrencyAddress + " MaxClaimable: " + metadata.MaxClaimable + " TokenPrice: " + metadata.TokenPrice + "("+ metadata.CurrencySymbol +") WalletLimit: " + metadata.WalletLimit + " SupplyClaimed: " + metadata.SupplyClaimed );
	
		return metadata;
	}    
	
	
	public async Task<BigInteger> BalanceOf( string address )
	{
		return await InternalThirdwebContract.ERC721_BalanceOf( address );
	}

    public async Task<string> OwnerOf( BigInteger tokenId )
    {
        return await InternalThirdwebContract.ERC721_OwnerOf( tokenId );
    }
    
    public async Task<ThirdwebTransactionReceipt> Claim( BigInteger quantity )
    {
	    return await InternalThirdwebContract.DropERC721_Claim( BlockchainClientNode.Instance.smartWallet, 
		    await BlockchainClientNode.Instance.smartWallet.GetAddress(), 
		    quantity );
    }	
    
    public async Task<ThirdwebTransactionReceipt> SafeTransferFrom( string fromAddress, string toAddress, BigInteger amount )
    {
	    return await InternalThirdwebContract.ERC721_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, amount );
    }

    /*
    public async Task<NFT> GetNFT( BigInteger tokenId )
    {
	    Log("Getting nft " + tokenId );
        return await InternalThirdwebContract.ERC721_GetNFT( tokenId );
    }

    // returns all of the NFTs owned by the current wallet for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs()
    {
	    var owner = await BlockchainClientNode.Instance.smartWallet.GetAddress();
	    Log("Getting nfts of owner " + owner );
        return await InternalThirdwebContract.ERC721_GetOwnedNFTs( owner );    
    }

    // returns all of the NFTs owned by the passed wallet address for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs( string walletAddress )
    {
        return await InternalThirdwebContract.ERC721_GetOwnedNFTs( walletAddress );    
    }  



    public async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient);
    }

    public async Task<Sprite2D> GetNFTAsSprite2D( BigInteger nftId  )
    {
        NFT nft = await InternalThirdwebContract.ERC721_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public async Task<ImageTexture> GetNFTAsTexture( BigInteger nftId )
    {
        NFT nft = await InternalThirdwebContract.ERC721_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( BigInteger nftId )
    {
        NFT nft = await InternalThirdwebContract.ERC721_GetNFT( nftId );

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
	    Log("Getting NFT as audio stream: " + nft.Metadata.Name);
	    Log("Getting NFT as audio stream: " + nft.Metadata.Description);
	    Log("Getting NFT as audio stream URL: " + nft.Metadata.AnimationUrl);	    
	    
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		
        
        Log("Received bytes for audio: " + downloadedData.Length);

        var audioStream = new AudioStreamMP3();
        audioStream.Data = downloadedData;

        return audioStream;
    }

    public async Task<AudioStreamMP3> GetNFTAsAudioStreamMP3(int nftId)
    {
        NFT nft = await InternalThirdwebContract.ERC721_GetNFT( nftId );
        
        return await GetNFTAsAudioStreamMP3(nft);
    }

    public async Task<byte[]> GetNFTAsByteArray(int nftId)
    {
        NFT nft = await InternalThirdwebContract.ERC721_GetNFT( nftId );

        return await GetNFTAsByteArray(nft);
    }

    public async Task<byte[]> GetNFTAsByteArray(NFT nft)
    {
	    Log("Getting NFT as bytearray: " + nft.Metadata.Name);
	    Log("Getting NFT as bytearray: " + nft.Metadata.Description);
	    Log("Getting NFT as audio stream URL: " + nft.Metadata.AnimationUrl);
	    
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		

        Log("Received bytes for audio: " + downloadedData.Length);

        return downloadedData;
    } 
    */

}