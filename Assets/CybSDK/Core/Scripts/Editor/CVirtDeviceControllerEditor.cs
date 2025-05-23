/************************************************************************************

Filename    :   CVirtDeviceControllerEditor.cs
Content     :   ___SHORT_DISCRIPTION___
Created     :   August 8, 2014
Last Updated:	September 11, 2018
Authors     :   Lukas Pfeifhofer
				Stefan Radlwimmer

Copyright   :   Copyright 2018 Cyberith GmbH

Licensed under the AssetStore Free License and the AssetStore Commercial License respectively.

************************************************************************************/

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System;
using System.Linq;

namespace CybSDK
{
	[CustomEditor(typeof(CVirtDeviceController))]
	public class CVirtDeviceControllerEditor : Editor
	{
		string factoryMethod = "CVirtDeviceController.InitVirtualizer";
		private readonly GUIContent deviceTypeLabel;
		private readonly GUIContent[] deviceTypeOptions;
		private readonly GUIContent directionCouplingTypeLabel;
		private readonly GUIContent[] directionCouplingTypeOptions;
		private readonly GUIContent activateHapticLabel;
		private readonly GUIContent cabilbrateLabel;

		public CVirtDeviceControllerEditor()
		{
			deviceTypeLabel = new GUIContent("Device Type", "Select the type of device you want to use.");
			deviceTypeOptions = new []
			{
				new GUIContent("[Automatic]", "CVirtDeviceController will automatically choose the best available device for you."),
				new GUIContent("Virtualizer Device", "A native Virtualizer Device hardware device."),
				new GUIContent("XInput (Xbox Controller)", "A virtual IVirtDevice, driven by DirectX xInput."),
				new GUIContent("Keyboard", "A virtual IVirtDevice, driven by Keyboard input."),
				new GUIContent(string.Format("Custom/Added in '{0}()'", factoryMethod), "If you want to use your own custom IVirtDevice implementation."),
			};

			directionCouplingTypeLabel = new GUIContent("Direction Coupling Type", "Override the default decoupling behaviour of the active IVirtDevice.");
			directionCouplingTypeOptions = new []
			{
				new GUIContent("Decoupled", "Camera direction and  movement direction are decoupled - Strongly recommended for the native Virtualizer."),
				new GUIContent("Coupled/Head Based Direction", "Movement and Camera is in the direction of the user's head."),
				new GUIContent("Coupled/Device Based Direction", "Movement and Camera is controlled by the Virtualizer device rotation - Do not use with the native Virtualizer!"),
			};

			activateHapticLabel = new GUIContent("Haptic Feedback", "Activates the vibration unit.");

			cabilbrateLabel = new GUIContent("Calibrate on connect", "Resets height and orientation on connect - Unused due to absolute tracking.");
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			CVirtDeviceController targetScript = (CVirtDeviceController) target;
			
			targetScript.deviceType = (VirtDeviceType) EditorGUILayout.Popup(
				deviceTypeLabel, 
				(int)targetScript.deviceType,
				deviceTypeOptions);

			if (targetScript.deviceType == VirtDeviceType.NativeVirtualizer)
			{
				// Device based direction coupling creates problems with the native Virtualizer
				targetScript.directionCouplingType = (DirectionCouplingType)EditorGUILayout.Popup(
					directionCouplingTypeLabel,
					(int)targetScript.directionCouplingType,
					directionCouplingTypeOptions.Take(2).ToArray());
			}
			else
			{
				targetScript.directionCouplingType = (DirectionCouplingType)EditorGUILayout.Popup(
					directionCouplingTypeLabel,
					(int)targetScript.directionCouplingType,
					directionCouplingTypeOptions);
			}
			
			targetScript.activateHaptic = EditorGUILayout.Toggle(activateHapticLabel, targetScript.activateHaptic);

			targetScript.calibrateOnConnect = EditorGUILayout.Toggle(
				cabilbrateLabel,
				targetScript.calibrateOnConnect);
			
			serializedObject.ApplyModifiedProperties();
			if (GUI.changed)
			{
				EditorUtility.SetDirty(targetScript);
				EditorSceneManager.MarkSceneDirty(targetScript.gameObject.scene);
			}

			if (EditorApplication.isPlaying)
				Repaint();
		}
	}
}