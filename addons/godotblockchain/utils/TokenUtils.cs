namespace BIGConnect.addons.godotblockchain.utils;

static class TokenUtils {
	public const string BASE_USDC_TOKEN = "0x833589fCD6eDb6E08f4c7C32D4f71b54bdA02913";
	public const string BASE_DEGEN_TOKEN = "0x4ed4e862860bed51a9570b96d89af5e1b0efefed";
	public const string BASE_AERO_TOKEN = "0x940181a94a35a4569e4529a3cdfb74e38fd98631";
	public const string BASE_TESTNET_PAPER_TOKEN = "0xd4b856A271dd46845D924Ae72a4e9b890128cbA7";

    private static readonly byte[] PngSignature = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
    private static readonly byte[] JpegSignature = new byte[] { 0xFF, 0xD8, 0xFF };
    private static readonly byte[] Mp3Signature = new byte[] { 0x49, 0x44, 0x33 }; // ID3 tag
    private static readonly byte[] Mp4Signature = new byte[] { 0x00, 0x00, 0x00, 0x18, 0x66, 0x74, 0x79, 0x70 }; // ftyp
	
	public static string PNG = "PNG";
    public static string JPEG = "JPEG";
    public static string MP3 = "MP3";
    public static string MP4 = "MP4";
	public static string GetFileType( byte[] fileBytes )
	{
        if (fileBytes.Length >= PngSignature.Length && MatchSignature(fileBytes, PngSignature))
            return PNG;
        if (fileBytes.Length >= JpegSignature.Length && MatchSignature(fileBytes, JpegSignature))
            return JPEG;
        if (fileBytes.Length >= Mp3Signature.Length && MatchSignature(fileBytes, Mp3Signature))
            return MP3;
        if (fileBytes.Length >= Mp4Signature.Length && MatchSignature(fileBytes, Mp4Signature))
            return MP4;
        
        return "Unknown";		
	}

   private static bool MatchSignature(byte[] fileBytes, byte[] signature)
    {
        for (int i = 0; i < signature.Length; i++)
        {
            if (fileBytes[i] != signature[i])
                return false;
        }
        return true;
    }
}