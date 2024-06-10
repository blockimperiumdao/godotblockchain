#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;

[GlobalClass, Tool]
public partial class BlockchainNFTResource : Resource
{
    [Export]
    public string name { get; set; }  

    [Export]
    public string symbol { get; set; }

	[Export(PropertyHint.Enum, "EthereumMainnet:1,EthereumGoerli:5,BaseMainnet:8453,BaseSepolia:84532,ArbitrumMainnet:42161,ArbitrumSepolia:421614,PolygonMainnet:137,PolygonzkEVM:1101,PolygonzkEVMTestnet:1442,OPMainnet:10,OPBedrock:28528,OPKovan:69,DogechainMainnet:2000,DogechainTestnet:568")]
	public int chainId {get; set;}
    
    [Export]
    public string contractAddress { get; set; }

	public BlockchainNFTResource()
	{
		name = "";
		symbol = "";
		chainId = 82532; // BaseSepolia
		contractAddress = "0x";
	}

    
}

#endif