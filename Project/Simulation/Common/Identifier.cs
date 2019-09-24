namespace Deterministic.Common
{
	public struct Identifier
	{
		public static readonly Identifier Empty = new Identifier(-1);

		private int value;

		public Identifier(int Value)
		{
			value = Value;
		}

		public static bool operator ==(Identifier LeftHand, Identifier RightHand)
		{
			return (LeftHand.value == RightHand.value);
		}

		public static bool operator !=(Identifier LeftHand, Identifier RightHand)
		{
			return !(LeftHand == RightHand);
		}

		public static implicit operator int(Identifier Value)
		{
			return Value.value;
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Hash.Get(ToString());
		}

		public override string ToString()
		{
			return "ID" + value;
		}
	}
}