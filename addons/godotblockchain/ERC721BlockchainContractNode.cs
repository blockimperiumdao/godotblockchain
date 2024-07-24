using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using System.Collections.Generic;

// Rest of the code
[GlobalClass,Tool]
public partial class ERC721BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC721BlockchainContractInitializedEventHandler();

	public string tokenName;
	public string symbol;
	public BigInteger totalSupply;
	public BigInteger balanceOf;

    public string currencyAddress { get; private set; }
    public ThirdwebContract currencyContract { get; private set; }
    public BigInteger currencyDecimals { get; private set; }
    public BigInteger maxClaimable { get; private set; }
    public byte[] merkleRoot { get; private set; }
    public BigInteger tokenPrice { get; private set; }
    public BigInteger adjustedTokenPrice { get; private set; }
    public BigInteger walletLimit { get; private set; }
    public BigInteger supplyClaimed { get; private set; }

	public override void _Ready()
	{
		if (contractResource == null)
		{
			Log("ERC721 contractResource is null");
			return;
		}
		else
		{
			Log("ERC721 initializing contract");
			Initialize();
		}
	}

    public new async void Initialize()
    {
		contract = await ThirdwebContract.Create(
			client: BlockchainClientNode.Instance.internalClient,
			address: contractResource.contractAddress,
			chain: contractResource.chainId
		);

        FetchMetadata();

		// emit a signal so systems will know that we are ready
		//
		EmitSignal(SignalName.ERC721BlockchainContractInitialized);
    }   

	public void Log( string message )
	{
		//EmitSignal(SignalName.ClientLogMessage, "ERC721BlockchainContractNode: " + message );
		BlockchainLogManager.Instance.EmitLog("ERC721BlockchainContractNode: " + message);
	} 
    // get the metadata of the token based on the current claim conditions
    public async void FetchMetadata( )
    {
        var claimConditions = await contract.DropERC721_GetActiveClaimCondition( );

        currencyAddress = claimConditions.Currency;
        maxClaimable = claimConditions.MaxClaimableSupply;
        merkleRoot = claimConditions.MerkleRoot;
        tokenPrice = claimConditions.PricePerToken;
        walletLimit = claimConditions.QuantityLimitPerWallet;
        supplyClaimed = claimConditions.SupplyClaimed;

        var currencyContract = await ThirdwebContract.Create(
            client: BlockchainClientNode.Instance.internalClient,
            address: currencyAddress,
            chain: contractResource.chainId
        );

		currencyDecimals = await currencyContract.ERC20_Decimals();
        adjustedTokenPrice = tokenPrice / BigInteger.Pow(10, (int)currencyDecimals);

        Log("Claim conditions: " + currencyAddress + " MaxClaimable: " + maxClaimable + " TokenPrice: " + adjustedTokenPrice + " WalletLimit: " + walletLimit + " SupplyClaimed: " + supplyClaimed );
    }

    public async Task<string> TokenName()
    {
        tokenName = await contract.ERC721_Name();
        return tokenName;
    }

    public async Task<string> Symbol()
    {
        symbol = await contract.ERC721_Symbol();
        return symbol;
    }    

	public async Task<BigInteger> TotalSupply(  )
	{
		totalSupply = await contract.ERC721_TotalSupply();

		return totalSupply;
	}

	public async Task<BigInteger> BalanceOf( )
	{
		balanceOf = await contract.ERC721_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress() );
		return balanceOf;
	}

	public async Task<BigInteger> BalanceOf( string address )
	{
		balanceOf = await contract.ERC721_BalanceOf( address );
		return balanceOf;
	}

    public async Task<string> OwnerOf( BigInteger tokenId )
    {
        return await contract.ERC721_OwnerOf( tokenId );
    }

    public async Task<NFT> GetNFT( BigInteger tokenId )
    {
        return await contract.ERC721_GetNFT( tokenId );
    }

    // returns all of the NFTs owned by the current wallet for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs()
    {
        return await contract.ERC721_GetOwnedNFTs( await BlockchainClientNode.Instance.smartWallet.GetAddress());    
    }

    // returns all of the NFTs owned by the passed wallet address for this particular contract
    public async Task<List<NFT>> GetOwnedNFTs( string walletAddress )
    {
        return await contract.ERC721_GetOwnedNFTs( walletAddress );    
    }  

	public async Task<ThirdwebTransactionReceipt> Claim( BigInteger quantity )
	{
		return await contract.DropERC721_Claim( BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), quantity );
	}	

    public async Task<ThirdwebTransactionReceipt> Transfer( string fromAddress, string toAddress, BigInteger tokenId )
    {
        return await contract.ERC721_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, tokenId );
    }


	public async Task<ThirdwebTransactionReceipt> SafeTransferFrom( string fromAddress, string toAddress, BigInteger amount )
	{
		return await contract.ERC721_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, amount );
	}

    public async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient);
    }

    public async Task<Sprite2D> GetNFTAsSprite2D( BigInteger nftId  )
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

        return await GetNFTAsSprite2D( nft );
    }

    public async Task<ImageTexture> GetNFTAsTexture( BigInteger nftId )
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

        return await GetNFTAsTexture( nft );
    }    

    public async Task<StandardMaterial3D> GetNFTAsStandardMaterial3D( BigInteger nftId )
    {
        NFT nft = await contract.ERC721_GetNFT( nftId );

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



}