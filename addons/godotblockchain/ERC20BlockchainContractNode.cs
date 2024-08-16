using System;
using System.Numerics;
using System.Threading.Tasks;
using Godot;
using Thirdweb;

namespace BIGConnect.addons.godotblockchain;

[GlobalClass,Tool]
public partial class ERC20BlockchainContractNode : BlockchainContractNode
{
	[Signal]
	public delegate void ERC20BlockchainContractInitializedEventHandler();

	private string _tokenName;
	private string _symbol;
	private BigInteger _totalSupply;
	private BigInteger _balanceOf;

	private string CurrencyAddress { get; set; }
    
	private string CurrencyIcon { get; set; }
    
	private ThirdwebContract ThirdwebCurrencyContract { get; set; }

	public BigInteger CurrencyDecimals { get; private set; }

	private BigInteger MaxClaimable { get; set; }
	private byte[] MerkleRoot { get; set; }
	private BigInteger TokenPrice { get; set; }
	private BigInteger WalletLimit { get; set; }
	private BigInteger SupplyClaimed { get; set; }

	private string CurrencySymbol { get; set; }

	private int TokenDecimals { get; set; }

	public override void _Ready()
	{
		if (ContractResource == null)
		{
			Log("contractResource is null");
			return;
		}
		else
		{
			Log("initializing contract");
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
		var claimConditions = await InternalThirdwebContract.DropERC20_GetActiveClaimCondition( );
		GD.Print(claimConditions.ToString());

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

			CurrencySymbol = chainData.NativeCurrency.Symbol;
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
			CurrencySymbol = await ThirdwebCurrencyContract.ERC20_Symbol();

			CurrencyDecimals = await ThirdwebCurrencyContract.ERC20_Decimals();
		}

		Log("Claim conditions: " + CurrencyAddress + " MaxClaimable: " + MaxClaimable + " TokenPrice: " + TokenPrice + "("+ CurrencySymbol +") WalletLimit: " + WalletLimit + " SupplyClaimed: " + SupplyClaimed );
	
		return claimConditions;
	}

	public async Task<BigInteger> BalanceOf(  )
	{
		_balanceOf = await InternalThirdwebContract.ERC20_BalanceOf( await BlockchainClientNode.Instance.smartWallet.GetAddress() );
		return _balanceOf;
	}

	public async Task<BigInteger> BalanceOf( string address )
	{
		_balanceOf = await InternalThirdwebContract.ERC20_BalanceOf( address );
		return _balanceOf;
	}	

	public async Task<BigInteger> TotalSupply(  )
	{
		_totalSupply = await InternalThirdwebContract.ERC20_TotalSupply();

		return _totalSupply;
	}
	public async Task<string> TokenName(  )
	{
		_tokenName = await InternalThirdwebContract.ERC20_Name();

		return _tokenName;
	}

	public async Task<string> Symbol(  )
	{
		_symbol = await InternalThirdwebContract.ERC20_Symbol();

		return _symbol;
	}

	public async Task<int> Decimals(  )
	{
		TokenDecimals = await InternalThirdwebContract.ERC20_Decimals();

		return TokenDecimals;
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