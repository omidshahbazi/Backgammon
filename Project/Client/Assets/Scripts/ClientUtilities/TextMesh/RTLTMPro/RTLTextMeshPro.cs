//#define RTL_OVERRIDE

using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

namespace RTLTMPro
{
	[ExecuteInEditMode]
	public class RTLTextMeshPro : TextMeshProUGUI
	{
		// ReSharper disable once InconsistentNaming
		private static StringBuilder Builder = new StringBuilder();
#if RTL_OVERRIDE
        public override string text
#else
		public new string text
#endif
		{
			get { return base.text; }
			set
			{
				if (OriginalText == value)
					return;
			
				OriginalText = value;
				UpdateText();
			}
		}

		public virtual bool PreserveNumbers
		{
			get { return preserveNumbers; }
			set
			{
				if (preserveNumbers == value)
					return;

				preserveNumbers = value;
				havePropertiesChanged = true;
			}
		}

		public bool Farsi
		{
			get { return farsi; }
			set
			{
				if (farsi == value)
					return;

				farsi = value;
				havePropertiesChanged = true;
			}
		}

		public bool FixTags
		{
			get { return fixTags; }
			set
			{
				if (fixTags == value)
					return;

				fixTags = value;
				havePropertiesChanged = true;
			}
		}

		protected bool ForceFix
		{
			get { return forceFix; }
			set
			{
				if (forceFix == value)
					return;

				forceFix = value;
				havePropertiesChanged = true;
			}
		}

		[SerializeField] public string OriginalText;
		[SerializeField] protected bool preserveNumbers;
		[SerializeField] protected bool farsi = true;
		[SerializeField] protected bool fixTags = true;
		[SerializeField] protected bool forceFix;

		protected RTLSupport support;
	
		protected override void Awake()
		{
			base.Awake();
			support = new RTLSupport();
			UpdateSupport();
		}

		//Amn I don't Know Why the programmer of text mesh pro put this function here 
#if UNITY_EDITOR
		protected virtual void Update()
		{
			if (Application.isPlaying)
				return;
			if (havePropertiesChanged)
			{
				if (support == null)
					support = new RTLSupport();

				UpdateSupport();
				UpdateText();
			}
		}
#endif

		public virtual void UpdateText()
		{
			if (support == null)
				support = new RTLSupport();

			if (OriginalText == null)
				OriginalText = "";

			if (ForceFix == false && support.IsRTLInput(OriginalText) == false)
			{
				isRightToLeftText = false;
				base.text = OriginalText; ;
			}
			else
			{
				isRightToLeftText = true;
				base.text = GetFixedText(OriginalText);
			}

			havePropertiesChanged = true;
		}

		protected virtual void UpdateSupport()
		{
			if (support == null)
				support = new RTLSupport();

			support.Farsi = farsi;
			support.PreserveNumbers = preserveNumbers;
			support.FixTextTags = fixTags;
		}

		public virtual string GetFixedText(string input)
		{
			if (string.IsNullOrEmpty(input))
				return input;

			if (support == null)
				support = new RTLSupport();


			char[]chars = support.FixRTL(input).Reverse().ToArray();
			Builder.Clear();
			for (int index = 0; index < chars.Length/* && (uint)chars[index] > 0U*/; ++index)
				Builder.Append(chars[index]);

			return Builder.ToString();

			//return ArrayToString(support.FixRTL(input).Reverse().ToArray());
		}

		//private static string ArrayToString( char[] chars)
		//{
		//	Builder.Clear();
		//	for (int index = 0; index < chars.Length && (uint)chars[index] > 0U; ++index)
		//		Builder.Append(chars[index]);
		//	return Builder.ToString();
		//}

	}
}