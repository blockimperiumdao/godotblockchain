using Godot;


namespace GodotBlockchain.addons.godotblockchain;

[GlobalClass,Tool]
public partial class BlockchainContractResource : Resource
{
	[Export]
	public string contractAddress { get; set; }

	[Export(PropertyHint.Enum, "EthereumMainnet:1,EthereumGoerli:5,BaseMainnet:8453,BaseSepolia:84532,ArbitrumMainnet:42161,ArbitrumSepolia:421614,PolygonMainnet:137,PolygonzkEVM:1101,PolygonzkEVMTestnet:1442,OPMainnet:10,OPBedrock:28528,OPKovan:69,DogechainMainnet:2000,DogechainTestnet:568")]
	public int chainId {get; set;}


	public BlockchainContractResource()
	{
		contractAddress = "0x";
		chainId = 82532; // BaseSepolia
	}

}
