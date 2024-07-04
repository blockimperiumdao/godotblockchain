using Godot;
using Thirdweb;

public partial class UIManager : Node
{
	
	public static UIManager Instance { get; private set; }
	
	private UIManager() { 
		// prevent other instances
	}


	public Panel LoginPanel { get; private set; }
	public LineEdit EmailInput { get; private set; }
	public LineEdit OTPInput { get; private set; }
	public TextEdit LogArea { get; private set; }
	public Button EmailSubmit { get; private set; }
	public Button OTPSubmit { get; private set; }
	public HBoxContainer OTPContainer { get; private set; }
	public HBoxContainer EmailContainer { get; private set; }	
	
	public string Log
	{
		get => LogArea.Text;
		set
		{
			LogArea.Text = value;
			GD.Print(value);
		}
	}

	public override void _Ready()
	{
		if (Instance != null)
		{
			GD.PrintErr("Multiple instances of UIManager are not allowed");
			return;
		}

		Instance = this;

		string emailContainerPath = "VBoxContainer/MenuBackground/LoginPanel/GridContainer/EmailContainer";
		string emailInputPath = "VBoxContainer/MenuBackground/LoginPanel/GridContainer/EmailContainer/Email Entry";
		string emailSubmitButtonPath = "VBoxContainer/MenuBackground/LoginPanel/GridContainer/EmailContainer/Email Submit";
		
		string otpContainerPath = "VBoxContainer/MenuBackground/LoginPanel/GridContainer/OTPContainer";
		string otpInputPath = "VBoxContainer/MenuBackground/LoginPanel/GridContainer/OTPContainer/OTP Entry";
		string otpSubmitButtonPath = "VBoxContainer/MenuBackground/LoginPanel/GridContainer/OTPContainer/OTP Submit";
		
		string logAreaPath = "VBoxContainer/MenuBackground/LogPanel/LogArea";

		LogArea = GetNode<TextEdit>( logAreaPath );

		OTPContainer = GetNode<HBoxContainer>( otpContainerPath );
		OTPContainer.Visible = false;

		OTPInput = GetNode<LineEdit>( otpInputPath );
		OTPInput.Visible = true;

		OTPSubmit = GetNode<Button>( otpSubmitButtonPath );
		OTPSubmit.Pressed += BlockchainClientNode.Instance.OnOTPSubmit;
		
		EmailContainer = GetNode<HBoxContainer> ( emailContainerPath );
		EmailContainer.Visible = true;
		
		EmailInput = GetNode<LineEdit>( emailInputPath );
		EmailInput.Visible = true;		
		
		EmailSubmit = GetNode<Button>( emailSubmitButtonPath );
		EmailSubmit.Pressed += BlockchainClientNode.Instance.OnStartLogin;


		BlockchainClientNode.Instance.LogMessage += ProcessLogMessage;
		BlockchainClientNode.Instance.AwaitingOTP += SetStateAwaitingOTP;
	}

	private void ProcessLogMessage(string message)
	{
		Log += message + "\n";
	}
	
	private void SetStateAwaitingOTP()
	{
		Log += "Awaiting OTP \n";
		
		OTPContainer.Visible = true;
		EmailContainer.Visible = false;
	}
}
