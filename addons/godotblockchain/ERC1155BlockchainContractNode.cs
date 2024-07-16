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

    public string currencyAddress { get; private set; }
    public BigInteger maxClaimable { get; private set; }
    public byte[] merkleRoot { get; private set; }
    public BigInteger tokenPrice { get; private set; }
    public BigInteger walletLimit { get; private set; }
    public BigInteger supplyClaimed { get; private set; }

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

	// public async Task<BigInteger> BalanceOfBatch( string[] addresses, BigInteger[] tokenIds  )
	// {
	// 	balanceOf = await contract.ERC1155_BalanceOfBatch( addresses , tokenIds );
	// 	return balanceOf;
	// }

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
	
	public async Task<BigInteger> Allowance( string owner, string spender )
	{
		return await contract.ERC20_Allowance( owner, spender );
	}

	public async Task<ThirdwebTransactionReceipt> Transfer( string toAddress, BigInteger amount )
	{
		return await contract.ERC20_Transfer( BlockchainClientNode.Instance.smartWallet, toAddress, amount );
	}

    public async Task<byte[]> GetNFTImage( NFT nft )
    {
        return await ThirdwebExtensions.GetNFTImageBytes( nft, BlockchainClientNode.Instance.internalClient);
    }

}