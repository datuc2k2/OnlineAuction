using BE_AuctionAOT.Common.Base.Entity;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BE_AuctionAOT.DAO.AuctionManagement.Payment
{
	public class PaymentOutputDto : BaseOutputDto
	{

	}
	public class FeeCaculationOutputDto : BaseOutputDto {
		public long ? Fee { get; set; }
	}
	public class ApiResponse
	{
		[JsonPropertyName("data")]
		public List<GiaoDich> Data { get; set; }

		[JsonPropertyName("error")]
		public bool Error { get; set; }
	}

	public class GiaoDich
	{
		[JsonPropertyName("Mã GD")]
		public int MaGd { get; set; }

		[JsonPropertyName("Mô tả")]
		public string MoTa { get; set; }

		[JsonPropertyName("Giá trị")]
		public int GiaTri { get; set; }

		[JsonPropertyName("Ngày diễn ra")]
		[JsonConverter(typeof(DateTimeConverter))]
		public DateTime NgayDienRa { get; set; }

		[JsonPropertyName("Số tài khoản")]
		public string SoTaiKhoan { get; set; }
	}
	public class DateTimeConverter : JsonConverter<DateTime>
	{
		private const string DateFormat = "yyyy-MM-dd HH:mm:ss"; // Định dạng ngày giờ trong JSON

		public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			if (reader.TokenType == JsonTokenType.String)
			{
				string dateTimeString = reader.GetString();
				if (DateTime.TryParseExact(dateTimeString, DateFormat, null, System.Globalization.DateTimeStyles.None, out DateTime date))
				{
					return date;
				}
			}

			throw new JsonException($"Không thể chuyển đổi giá trị JSON thành DateTime. Giá trị: {reader.GetString()}");
		}

		public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
		{
			writer.WriteStringValue(value.ToString(DateFormat));
		}
	}
}
