using Godot;

namespace BIGConnect.addons.godotblockchain;

[GlobalClass,Tool]

public partial class BlockchainLogManager : Node
{
	[Signal]
	public delegate void LogMessageEventHandler( string logMessage );

	[Signal]
	public delegate void ErrorMessageEventHandler( string logMessage );

	private static BlockchainLogManager instance = null;
	public static BlockchainLogManager Instance { 
		get 
		{
			if (instance == null )
			{
				instance = new BlockchainLogManager();
			}

			return instance;

		}
	
	}

	public void EmitLog( string message )
	{
		GD.Print( message );
		EmitSignal(SignalName.LogMessage, message);
	}

	public void EmitError( string message )
	{
		GD.PushError( message );
		EmitSignal(SignalName.ErrorMessage, message);
	}

}
