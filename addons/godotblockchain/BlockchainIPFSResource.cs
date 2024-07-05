#if TOOLS
using System.Threading.Tasks;
using Godot;
using Thirdweb;
using System.Numerics;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;

[GlobalClass, Tool]
public partial class BlockchainIPFSResource : Resource
{
    [Export]
    public string name { get; set; }  
   
    [Export]
    public string ipfsURI { get; set; }

	public BlockchainIPFSResource()
	{
        name = "";
        ipfsURI = "";
	}  
}

#endif