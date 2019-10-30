
using Assets.Scripts.GamePlayLogic;
using Networking.Common;
using System;
using UnityEditor;
using UnityEngine;

public static class ProjectConfigurator
{
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

		PlayerSettings.Android.bundleVersionCode = versionNumber;

		PlayerSettings.Android.useCustomKeystore = true;
		PlayerSettings.Android.keystoreName = Application.dataPath + "/../../../Materials/Keys/Main.keystore";
		PlayerSettings.Android.keystorePass = "#EDC2wsx";
		PlayerSettings.Android.keyaliasName = "royalgammon";
		PlayerSettings.Android.keyaliasPass = "#EDC2wsx";
	}

	[MenuItem("Edit/Configure Project Settings (Myket)")]
	public static void ConfigureMyket()
	{
		Configure(Markets.Myket);
	}
}