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

	private string _tokenName;
	private string _currencySymbol;
	private BigInteger _totalSupply;
	private BigInteger _balanceOf;

    public string ContractAddress { get; private set; }
    public string CurrencyAddress { get; private set; }
    private string CurrencyIcon { get; set; }
    public ThirdwebContract ThirdwebCurrencyContract { get; private set; }
    public BigInteger CurrencyDecimals { get; private set; }
    public BigInteger MaxClaimable { get; private set; }
    public byte[] MerkleRoot { get; private set; }
    public BigInteger TokenPrice { get; private set; }
    public BigInteger WalletLimit { get; private set; }
    public BigInteger SupplyClaimed { get; private set; }

	public override void _Ready()
	{
		if (ContractResource == null)
		{
			Log("ERC721 contractResource is null");
			return;
		}
		else
		{
			Log("ERC721 initializing contract");
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

        ContractAddress = ContractResource.contractAddress;

        if ( InternalThirdwebContract != null )
        {
            await FetchMetadata();

            // emit a signal so systems will know that we are ready
            //
            EmitSignal(SignalName.ERC721BlockchainContractInitialized);
        }
    }   

	public void Log( string message )
	{
		//EmitSignal(SignalName.ClientLogMessage, "ERC721BlockchainContractNode: " + message );
		BlockchainLogManager.Instance.EmitLog("ERC721BlockchainContractNode: " + message);
	} 
	
	// fills the node with the metadata from the Blockchain based on the active (i.e. currently use) claim condition
	public async Task<Drop_ClaimCondition> FetchMetadata()
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

		Log("Setting metadata");
		CurrencyAddress = claimConditions.Currency;
		MaxClaimable = claimConditions.MaxClaimableSupply;
		MerkleRoot = claimConditions.MerkleRoot;
		TokenPrice = claimConditions.PricePerToken;
		WalletLimit = claimConditions.QuantityLimitPerWallet;
		SupplyClaimed = claimConditions.SupplyClaimed;
		
		// check to see if the currency address is the native currency of the chain
		// if it is, then we need to get the native currency information
		if (string.Equals(CurrencyAddress.ToLower(), "0xEeeeeEeeeEeEeeEeEeEeeEEEeeeeEeeeeeeeEEeE".ToLower(),
			    StringComparison.Ordinal))
		{
			var chainData = await Utils.FetchThirdwebChainDataAsync(BlockchainClientNode.Instance.internalClient,
				ContractResource.chainId);

			_currencySymbol = chainData.NativeCurrency.Symbol;
			CurrencyDecimals = chainData.NativeCurrency.Decimals;
			CurrencyIcon = chainData.Icon.Url;
		}
		else
		{
			// get information about the currency required to claim the token
			ThirdwebCurrencyContract = await ThirdwebContract.Create(
				client: BlockchainClientNode.Instance.internalClient,
				address: CurrencyAddress,
				chain: ContractResource.chainId
			);
			_currencySymbol = await ThirdwebCurrencyContract.ERC20_Symbol();

			CurrencyDecimals = await ThirdwebCurrencyContract.ERC20_Decimals();
		}

		Log("Claim conditions: " + CurrencyAddress + " MaxClaimable: " + MaxClaimable + " TokenPrice: " + TokenPrice + "("+ _currencySymbol +") WalletLimit: " + WalletLimit + " SupplyClaimed: " + SupplyClaimed );
	
		return claimConditions;
	}    
    

    public async Task<string> TokenName()
    {
        _tokenName = await InternalThirdwebContract.ERC721_Name();
        return _tokenName;
    }

    public async Task<string> Symbol()
    {
        _currencySymbol = await InternalThirdwebContract.ERC721_Symbol();
        return _currencySymbol;
    }    

	public async Task<BigInteger> TotalSupply(  )
	{
		_totalSupply = await InternalThirdwebContract.ERC721_TotalSupply();

		return _totalSupply;
	}

	public async Task<BigInteger> BalanceOf( )
	{
		_balanceOf = await InternalThirdwebContract.ERC721_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress() );
		return _balanceOf;
	}

	public async Task<BigInteger> BalanceOf( string address )
	{
		_balanceOf = await InternalThirdwebContract.ERC721_BalanceOf( address );
		return _balanceOf;
	}

    public async Task<string> OwnerOf( BigInteger tokenId )
    {
        return await InternalThirdwebContract.ERC721_OwnerOf( tokenId );
    }

    public async Task<NFT> GetNFT( BigInteger tokenId )
    {
        return await InternalThirdwebContract.ERC721_GetNFT( tokenId );
    }

    // returns all of the NFTs owned by the current wallet for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs()
    {
        return await InternalThirdwebContract.ERC721_GetOwnedNFTs( await BlockchainClientNode.Instance.smartWallet.GetAddress());    
    }

    // returns all of the NFTs owned by the passed wallet address for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs( string walletAddress )
    {
        return await InternalThirdwebContract.ERC721_GetOwnedNFTs( walletAddress );    
    }  

	public async Task<ThirdwebTransactionReceipt> Claim( BigInteger quantity )
	{
		return await InternalThirdwebContract.DropERC721_Claim( BlockchainClientNode.Instance.smartWallet, 
                                                await BlockchainClientNode.Instance.smartWallet.GetAddress(), 
                                                quantity );
	}	

    public async Task<ThirdwebTransactionReceipt> Transfer( string fromAddress, string toAddress, BigInteger tokenId )
    {
        return await InternalThirdwebContract.ERC721_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, tokenId );
    }


	public async Task<ThirdwebTransactionReceipt> SafeTransferFrom( string fromAddress, string toAddress, BigInteger amount )
	{
		return await InternalThirdwebContract.ERC721_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, amount );
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
        byte[] downloadedData = await ThirdwebStorage.Download<byte[]>(BlockchainClientNode.Instance.internalClient, 
                                                                        nft.Metadata.AnimationUrl);		

        Log("Received bytes for audio: " + downloadedData.Length);

        return downloadedData;
    } 

}