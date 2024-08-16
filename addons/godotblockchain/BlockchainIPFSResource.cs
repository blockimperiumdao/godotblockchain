using Godot;


namespace BIGConnect.addons.godotblockchain;


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
