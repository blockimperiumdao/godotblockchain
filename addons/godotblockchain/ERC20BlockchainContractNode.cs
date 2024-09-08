using System;
using System.Numerics;
using System.Threading.Tasks;
using BIGConnect.addons.godotblockchain.utils;
using Godot;
using Thirdweb;

namespace BIGConnect.addons.godotblockchain;

[GlobalClass,Tool]
public partial class ERC20BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC20BlockchainContractInitializedEventHandler();

	public class ERC20TokenMetadata
	{
		internal BigInteger BalanceOf { get; set; }
		public string CurrencyAddress { get; internal set; }
		public string CurrencyIcon { get; internal set; }
		public BigInteger CurrencyDecimals { get; internal set; }
		public BigInteger MaxClaimable { get; internal set; }
		public  byte[] MerkleRoot { get; internal set; }
		public BigInteger TokenPrice { get; internal set; }
		public BigInteger WalletLimit { get; internal set; }
		public BigInteger SupplyClaimed { get; internal set; }
		public string CurrencySymbol { get; internal set; }
		public BigInteger TotalSupply { get; internal set; }
		public string TokenName { get; internal set; }
		public string TokenSymbol { get; internal set; }
		public BigInteger TokenDecimals { get; internal set; }
		
		public Drop_ClaimCondition ClaimConditions { get; internal set; }
		
		public ThirdwebContract CurrencyContract { get; internal set; }
	}
	
	public ThirdwebContract ThirdwebCurrencyContract { get; internal set; }

	
	public override void _Ready()
	{
		if (ContractResource == null)
		{
			Log("contractResource is null");
			return;
		}
		else
		{
			Log("ERC20 initializing contract " + ContractResource.contractAddress);
			Initialize();
		}

		AddToGroup("Blockchain", true);
	}

	private new async void Initialize()
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

		// emit a signal so systems will know that we are ready
		//
		if (InternalThirdwebContract != null )
		{
			EmitSignal(SignalName.ERC20BlockchainContractInitialized);
		}		
	}   

	private void Log( string message )
	{
		BlockchainLogManager.Instance.EmitLog("ERC20BlockchainContractNode: " + message);
	} 

	// fills the node with the metadata from the Blockchain based on the active (i.e. currently use) claim condition
	public async Task<ERC20TokenMetadata> FetchMetadata()
	{
		Log("Getting claim conditions");

		if (InternalThirdwebContract == null)
		{
			GD.Print("InternalThirdWebContract is null");
			return null;
		}
		Drop_ClaimCondition claimConditions = await InternalThirdwebContract.DropERC20_GetActiveClaimCondition();
		GD.Print(claimConditions.ToString());
		
		Log("Setting metadata");

		ERC20TokenMetadata metadata = new ERC20TokenMetadata()
		{
			ClaimConditions = claimConditions,
			CurrencyAddress = claimConditions.Currency,
			MaxClaimable = claimConditions.MaxClaimableSupply,
			MerkleRoot = claimConditions.MerkleRoot,
			TokenPrice = claimConditions.PricePerToken,
			WalletLimit = claimConditions.QuantityLimitPerWallet,
			SupplyClaimed = claimConditions.SupplyClaimed,

			BalanceOf = await InternalThirdwebContract.ERC20_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress() ),

			TotalSupply = await InternalThirdwebContract.ERC20_TotalSupply(),
			TokenName = await InternalThirdwebContract.ERC20_Name(),
			TokenSymbol = await InternalThirdwebContract.ERC20_Symbol(),
			TokenDecimals = await InternalThirdwebContract.ERC20_Decimals()
		};
		
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
			ThirdwebCurrencyContract = await ThirdwebContract.Create(
				client: BlockchainClientNode.Instance.internalClient,
				address: metadata.CurrencyAddress,
				chain: ContractResource.chainId
			);
			metadata.CurrencySymbol = await ThirdwebCurrencyContract.ERC20_Symbol();
			metadata.CurrencyDecimals = await ThirdwebCurrencyContract.ERC20_Decimals();
			metadata.CurrencyContract = ThirdwebCurrencyContract;
		}

		Log("Claim conditions: " + metadata.CurrencyAddress + " MaxClaimable: " + metadata.MaxClaimable + " TokenPrice: " + metadata.TokenPrice + "("+ metadata.CurrencySymbol +") WalletLimit: " + metadata.WalletLimit + " SupplyClaimed: " + metadata.SupplyClaimed );
	
		return metadata;
	}
	
	public async Task<BigInteger> BalanceOf( string address )
	{
		return  await InternalThirdwebContract.ERC20_BalanceOf( address );
	}	
	
	public async Task<ThirdwebTransactionReceipt> Claim( string amount )
	{
		return await InternalThirdwebContract.DropERC20_Claim( BlockchainClientNode.Instance.smartWallet, await BlockchainClientNode.Instance.smartWallet.GetAddress(), amount );
	}	
	
	public async Task<BigInteger> Allowance( string owner, string spender )
	{
		return await InternalThirdwebContract.ERC20_Allowance( owner, spender );
	}

	public async Task<ThirdwebTransactionReceipt> Transfer( string toAddress, BigInteger amount )
	{
		return await InternalThirdwebContract.ERC20_Transfer( BlockchainClientNode.Instance.smartWallet, toAddress, amount );
	}

	public async Task<ThirdwebTransactionReceipt> TransferFrom( string fromAddress, string toAddress, BigInteger amount )
	{
		return await InternalThirdwebContract.ERC20_TransferFrom( BlockchainClientNode.Instance.smartWallet, fromAddress, toAddress, amount );
	}

}