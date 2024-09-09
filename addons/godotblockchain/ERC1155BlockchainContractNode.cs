using System;
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using System.Collections.Generic;
using GodotBlockchain.addons.godotblockchain.utils;

namespace GodotBlockchain.addons.godotblockchain;

[GlobalClass,Tool]
public partial class ERC1155BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC1155BlockchainContractInitializedEventHandler();


	public class ERC1155TokenMetadata
	{
		public BigInteger TotalSupply { get; internal set; }
		public BigInteger BalanceOf { get; internal set; }
		
		public string CurrencyAddress { get; internal set; }
		public BigInteger MaxClaimable { get; internal set; }
		public byte[] MerkleRoot { get; internal set; }
		public BigInteger TokenPrice { get; internal set; }
		public BigInteger WalletLimit { get; internal set; }
		public BigInteger SupplyClaimed { get; internal set; }	
		public string CurrencyIcon { get; internal set; }
		
		public string CurrencySymbol { get; internal set; }

		public ThirdwebContract ThirdwebCurrencyContract { get; internal set; }
		public BigInteger CurrencyDecimals { get; internal set; } 
		
		public Drop_ClaimCondition ClaimConditions { get; internal set; }
		
		public ThirdwebContract CurrencyContract { get; internal set; }
	}
	
	
	public override void _Ready()
	{
		if (ContractResource == null)
		{
			Log("ERC1155 contractResource is null");
			return;
		}
		else
		{
			Log("ERC1155 initializing contract " + ContractResource.contractAddress);
			Initialize();
		}

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
    
    // fills the node with the metadata from the Blockchain based on the active (i.e. currently use) claim condition
    public async Task<ERC1155TokenMetadata> FetchMetadataForToken(BigInteger tokenId)
    {
	    Log("Getting claim conditions for " + tokenId );

	    if (InternalThirdwebContract == null) return null;
	    var claimConditions = await InternalThirdwebContract.DropERC1155_GetActiveClaimCondition( tokenId );

	    ERC1155TokenMetadata metadata = new ERC1155TokenMetadata
	    {
		    CurrencyAddress = claimConditions.Currency,
		    MaxClaimable = claimConditions.MaxClaimableSupply,
		    MerkleRoot = claimConditions.MerkleRoot,
		    TokenPrice = claimConditions.PricePerToken,
		    WalletLimit = claimConditions.QuantityLimitPerWallet,
		    SupplyClaimed = claimConditions.SupplyClaimed,
		    ClaimConditions = claimConditions,
		    BalanceOf = await InternalThirdwebContract.ERC1155_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress(), tokenId ),
		    TotalSupply = await InternalThirdwebContract.ERC1155_TotalSupply( tokenId )
	    };

	    // check to see if the currency address is the native currency of the chain
	    // if it is, then we need to get the native currency information
	    if (string.Equals(metadata.CurrencyAddress.ToLower(), 
			TokenUtils.THIRDWEB_CHAIN_NATIVE_TOKEN.ToLower(),
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

	public async Task<ThirdwebTransactionReceipt> Claim( BigInteger tokenId, BigInteger quantity )
	{
		return await InternalThirdwebContract.DropERC1155_Claim( BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), tokenId, quantity );
	}	

	public async Task<ThirdwebTransactionReceipt> Transfer( string fromAddress, string toAddress, BigInteger tokenId, BigInteger amount )
	{
		return await InternalThirdwebContract.ERC1155_SafeTransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, tokenId, amount, new byte[] {} );
	}



}