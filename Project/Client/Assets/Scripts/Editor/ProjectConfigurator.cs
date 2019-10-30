
using Assets.Scripts.GamePlayLogic;
using Networking.Common;
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class ProjectConfigurator
{
	private const string MARKET_ACTIVITY_PLACEHOLDER = "<MARKET_ACTIVITY/>";
	private const string MARKET_PERMISSION_PLACEHOLDER = "<MARKET_PERMISSION/>";
	private const string MYKET_ACTIVITY = "<activity android:name=\"ir.myket.unity.iab.MyketBillingService$IabActivity\" android:theme=\"@android:style/Theme.Translucent.NoTitleBar.Fullscreen\" android:configChanges=\"orientation|screenSize|keyboardHidden\"/>";
	private const string MYKET_PERMISSION = "<uses-permission android:name=\"ir.mservices.market.BILLING\"/>";

	public static void Configure(Markets Market)
	{
		GameObject projectConfigsObj = Resources.Load<GameObject>("Prefabs/ProjectConfigs");
		if (projectConfigsObj == null)
		{
			Debug.LogError("ProjectConfigs prefab doesn't exists");
			return;
		}

		ProjectConfigs projectConfigs = projectConfigsObj.GetComponent<ProjectConfigs>();
		if (projectConfigs == null)
		{
			Debug.LogError("ProjectConfigs prefab doesn't contain ProjectConfigs");
			return;
		}

		Version version = Version.Parse(projectConfigs.Version);
		int versionNumber = (version.Major * 1000) + (version.Minor * 100) + (version.Build * 10) + version.Revision;

		projectConfigs.market = Market;
		projectConfigs.VersionNumber = versionNumber;

		AssetDatabase.SaveAssets();

		PlayerSettings.companyName = "Zorvan Guys";

		if (Market == Markets.Myket || Market == Markets.Cafebazaar)
			PlayerSettings.productName = "رویال نرد";
		else
			PlayerSettings.productName = "Royal Gammon";

		PlayerSettings.bundleVersion = version.ToString();

		PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;

		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Standalone, Constants.PACKAGE_NAME);
		PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, Constants.PACKAGE_NAME);

		string defines = "MARKET_" + Market.ToString().ToUpper();
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, defines);
		PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Android, defines);

		PlayerSettings.Android.bundleVersionCode = versionNumber;

		PlayerSettings.Android.useCustomKeystore = true;
		PlayerSettings.Android.keystoreName = Application.dataPath + "/../../../Materials/Keys/Main.keystore";
		PlayerSettings.Android.keystorePass = "#EDC2wsx";
		PlayerSettings.Android.keyaliasName = "royalgammon";
		PlayerSettings.Android.keyaliasPass = "#EDC2wsx";

		string templateManifest = File.ReadAllText(Application.dataPath + "/Plugins/Android/AndroidManifest.Template.xml");

		switch (Market)
		{
			case Markets.Windows:
				break;
			case Markets.Cafebazaar:
				break;
			case Markets.Myket:
				templateManifest = templateManifest.Replace(MARKET_ACTIVITY_PLACEHOLDER, MYKET_ACTIVITY);
				templateManifest = templateManifest.Replace(MARKET_PERMISSION_PLACEHOLDER, MYKET_PERMISSION);
				break;
		}

		File.WriteAllText(Application.dataPath + "/Plugins/Android/AndroidManifest.xml", templateManifest);
	}

	[MenuItem("Edit/Configure Project Settings (Myket)")]
	public static void ConfigureMyket()
	{
		Configure(Markets.Myket);
	}
}