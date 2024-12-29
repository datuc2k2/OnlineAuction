namespace BE_AuctionAOT.Common.Utility
{
	public class EkycObj
	{
	}
	public class CompareFaceResponse
	{
		public ImgDetails imgs { get; set; }
		public string dataSign { get; set; }
		public string dataBase64 { get; set; }
		public string logID { get; set; }
		public string message { get; set; }
		public string server_version { get; set; }
		public ResultObject @object { get; set; }
		public int statusCode { get; set; }
		public string challengeCode { get; set; }
	}

	public class ImgDetails
	{
		public string img_face { get; set; }
		public string img_front { get; set; }
	}

	public class ResultObject
	{
		public string result { get; set; }
		public string msg { get; set; }
		public double prob { get; set; }
		public string match_warning { get; set; }
		public bool multiple_faces { get; set; }
	}
	public class CompareFaceResult
	{
		public string Result { get; set; }
		public string Msg { get; set; }
		public double Prob { get; set; }
	}
}
