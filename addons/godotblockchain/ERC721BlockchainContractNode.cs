using System;
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using GodotBlockchain.addons.godotblockchain.utils;

namespace GodotBlockchain.addons.godotblockchain;

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
		if (string.Equals(metadata.CurrencyAddress.ToLower(), TokenUtils.THIRDWEB_CHAIN_NATIVE_TOKEN.ToLower(),
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
    
}