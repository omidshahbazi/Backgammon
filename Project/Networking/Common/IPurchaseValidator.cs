namespace Networking.Common
{
	public interface IPurchaseValidator
	{
		bool Validate(string SKU, string Token);
	}
}