/**
*
* Copyright (c) 2016 XZIMG , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of xzimg
*
* The XZIMG company is located at 903 Dannies House, 20 Luard Road, Wanchai, Hong Kong
* contact@xzimg.com, www.xzimg.com
*
*/

using UnityEngine;
using System;
using System.Collections;

using System.Runtime.InteropServices;

public class xmgAugmentedImage : xmgAugmentedVisionBase
{
#if UNITY_ANDROID || UNITY_IOS
	private xmgFaceApiVideoCapture videoOptions;
	private Texture2D imgTexture;
    private Texture2D uvTexture;
    private System.IntPtr textureId_uv;
#endif
    private Boolean mInitialized = false;

    // -------------------------------------------------------------------------------------------------------------------

    void Start()
        {
            mInitialized = false;
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
            if (m_myWebCamEngine == null)
			{
				m_myWebCamEngine = (xmgWebCamTexture)gameObject.AddComponent(typeof(xmgWebCamTexture));
				m_myWebCamEngine.CaptureWidth = videoParameters.GetVideoCaptureWidth();
				m_myWebCamEngine.CaptureHeight = videoParameters.GetVideoCaptureHeight();
				m_myWebCamEngine.MirrorVideo = videoParameters.MirrorVideo;
				m_myWebCamEngine.CameraFOVX = videoParameters.CameraFOVX;
				m_myWebCamEngine.UseFrontal = false;
				m_webcamTexture = m_myWebCamEngine.CreateVideoCapturePlane(videoParameters.VideoPlaneScale, videoParameters.videoPlaneFittingMode, videoParameters.videoCaptureIndex);
            }
            if (!m_webcamTexture) { Debug.Log("No camera detected!"); }
            if (m_webcamTexture)
            {
				int captureWidth = m_webcamTexture.width, captureHeight = m_webcamTexture.height;
				if (captureWidth < 100) 
				{
					// Unity BUG MACOSX
					captureWidth = m_webcamTexture.requestedWidth;
					captureHeight = m_webcamTexture.requestedHeight;
				}
                // Image has the size of obtained video capture resolution
				m_image.m_width = captureWidth;
				m_image.m_height = captureHeight;
                m_image.m_colorType = 4;
				m_image.m_type = 0;
				m_image.m_flippedH = 1;

                float fovx_radian = videoParameters.CameraFOVX * 3.1415f / 180.0f;
				int success = xmgAugmentedVisionBridge.xzimgNaturalImageInitialize(captureWidth, captureHeight, videoParameters.GetProcessingWidth(captureWidth), videoParameters.GetProcessingHeight(captureHeight), fovx_radian);
               	if (success == 1)
                    print("xzimgNaturalImageInitialize - success");
            }
#elif UNITY_ANDROID
				
				if (videoParameters.UseFrontal)
					videoParameters.MirrorVideo = true;
				
				videoOptions.resolution_mode = videoParameters.videoCaptureMode;
			videoOptions.frontal = 0;
				if (videoParameters.UseFrontal) videoOptions.frontal = 1;
			videoOptions.focus_mode = 1;			// Continuously updates = 1
			videoOptions.exposure_mode = 1;			// Continuously updates = 1
			videoOptions.while_balance_mode = 1;	// Continuously updates = 1
			PrepareBackgroundPlane(videoOptions.frontal==1);

		    // Create the texture for video stream
            imgTexture = new Texture2D(videoParameters.GetVideoCaptureWidth(), videoParameters.GetVideoCaptureHeight(), TextureFormat.ARGB32, false);
		    GetComponent<Renderer>().material.mainTexture = imgTexture;
			videoOptions.texture = imgTexture.GetNativeTexturePtr();

            // Second texture
            uvTexture = new Texture2D(videoParameters.GetVideoCaptureWidth() / 2, videoParameters.GetVideoCaptureHeight() / 2, TextureFormat.R16, false);
            GetComponent<Renderer>().material.SetTexture("_UVTex", uvTexture);
			videoOptions.texture_uv = uvTexture.GetNativeTexturePtr();

            double fovx_radian = (double)videoParameters.CameraFOVX * 3.1415 / 180.0;
            xmgAugmentedVisionBridge.xzimgNaturalImageInitialize(videoOptions.resolution_mode, videoParameters.UseFrontal, fovx_radian, videoParameters.HighPrecision);

#elif UNITY_IOS
			// iOS plugin has its own videoCapture module
				if (videoParameters.UseFrontal)
					videoParameters.MirrorVideo = true;
			videoOptions.resolution_mode = 1;
				videoOptions.frontal = 0;
				if (videoParameters.UseFrontal) videoOptions.frontal = 1;
			videoOptions.focus_mode = 1;			// Continuously updates = 1
			videoOptions.exposure_mode = 1;			// Continuously updates = 1
			videoOptions.while_balance_mode = 1;	// Continuously updates = 1
			
			PrepareBackgroundPlane(videoOptions.frontal==1);
			
			// Create the texture to display video stream
			imgTexture = new Texture2D(videoParameters.GetVideoCaptureWidth(), videoParameters.GetVideoCaptureHeight(), TextureFormat.BGRA32, false);
			GetComponent<Renderer>().material.mainTexture = imgTexture;
			//	videoOptions.textureId = imgTexture.GetNativeTextureID();
			videoOptions.texture = imgTexture.GetNativeTexturePtr();
				
			float fovx_radian = videoParameters.CameraFOVX * 3.1415f / 180.0f;
			int status = xmgAugmentedVisionBridge.xzimgNaturalImageInitialize(ref videoOptions, videoParameters.GetProcessingWidth(), videoParameters.GetProcessingHeight(), fovx_radian);
			if (status != 1) Debug.Log("Initialization has failed!");
#endif
            LoadImages();
        mInitialized = true;
    }

    // -------------------------------------------------------------------------------------------------------------------

    void OnDisable()
    {
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
        m_myWebCamEngine.ReleaseVideoCapturePlane();
#endif
        xmgAugmentedVisionBridge.xzimgNaturalImageRelease();
    }

    // -------------------------------------------------------------------------------------------------------------------

    void Update ()
    {
        if (!mInitialized) return;
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
        if (!m_myWebCamEngine) return;

        bool isPlaying = m_webcamTexture.isPlaying;
        if (!isPlaying) return;

        if (!m_myWebCamEngine.GetData()) return;

        m_image.m_imageData = m_myWebCamEngine.m_PixelsHandle.AddrOfPinnedObject();
        xmgAugmentedVisionBridge.xzimgNaturalImageTrack(ref m_image, visionParameters.FilterStrength);
        m_myWebCamEngine.ApplyTexture();
#elif (UNITY_IPHONE)
			xmgAugmentedVisionBridge.xzimgNaturalImageTrack(visionParameters.FilterStrength);

			// to prevent Unity bug on iOS
			UpdateBackgroundPlaneOrientation(videoParameters.UseFrontal);
#elif (UNITY_ANDROID)
		xmgAugmentedVisionBridge.xzimgAugmentedVisionDetect(videoOptions.texture, videoOptions.texture_uv);
#endif
        DisableObjects();
        int iNbrOfDetection = xmgAugmentedVisionBridge.xzimgNaturalImageGetNumber();
        UpdateDebugDisplay(iNbrOfDetection);
        if (iNbrOfDetection > 0)
        {
            for (int i = 0; i < iNbrOfDetection; i++)
            {
                xmgMarkerInfo markerInfo = new xmgMarkerInfo();
                xmgAugmentedVisionBridge.xzimgNaturalImageGetInfoForUnity(i, ref markerInfo);
                    EnableObject(markerInfo.markerID);
                    UpdateObjectPosition(ref markerInfo);
            }
        }
	}

    // -------------------------------------------------------------------------------------------------------------------

    // Load Resources from the /Assets/Resources directory
    void LoadImages()
	{
		for (int i = 0; i < visionParameters.ObjectPivotLinks.Count; i++)
		{
			if (visionParameters.ObjectPivotLinks[i].Classifier)
			{
                if (visionParameters.ObjectPivotLinks[i].ObjectRealWidth <= 0)
                    visionParameters.ObjectPivotLinks[i].ObjectRealWidth = 1;

				TextAsset asset = visionParameters.ObjectPivotLinks[i].Classifier as TextAsset;
				byte[] arrBytes = new byte[asset.bytes.Length];
				Buffer.BlockCopy(asset.bytes, 0, arrBytes, 0, asset.bytes.Length);


				GCHandle bytesHandle = GCHandle.Alloc(arrBytes, GCHandleType.Pinned);
				int success = xmgAugmentedVisionBridge.xzimgNaturalImageAddTarget(bytesHandle.AddrOfPinnedObject(), arrBytes.Length, visionParameters.ObjectPivotLinks[i].ObjectRealWidth);
				if (success == 1) 
					print("xzimgMarkerlessLoadClassifier - Success");
				else 
					print("failed to load " + asset.name);

				bytesHandle.Free();

			}
		}
	}

    // -------------------------------------------------------------------------------------------------------------------

    void UpdateDebugDisplay(int iDetected, bool protectionAlert)
	{
		if (protectionAlert)
			m_debugStatus = "Protection Alert - Please reload the plugin";
		else
		{
			if (iDetected > 0)
				m_debugStatus = "Marker Detected  " + iDetected;
			else
				m_debugStatus = "Marker Not Detected";
		}
	}

    // -------------------------------------------------------------------------------------------------------------------

    private void DisableObjects()
	{
		if (visionParameters.ObjectPivotLinks.Count > 0)
		{
			for (int i = 0; i < visionParameters.ObjectPivotLinks.Count; i++)
			{
				if (visionParameters.ObjectPivotLinks[i].ScenePivot)
				{
					Renderer[] renderers = visionParameters.ObjectPivotLinks[i].ScenePivot.GetComponentsInChildren<Renderer>();
					foreach (Renderer r in renderers) r.enabled = false;
				}
			}
		}
	}

    // -------------------------------------------------------------------------------------------------------------------

    private void EnableObject(int indexPivot)
	{
		if (indexPivot < visionParameters.ObjectPivotLinks.Count &&
            visionParameters.ObjectPivotLinks[indexPivot].ScenePivot)
		{
            visionParameters.ObjectPivotLinks[indexPivot].ScenePivot.SetActive(true);
			Renderer[] renderers = visionParameters.ObjectPivotLinks[indexPivot].ScenePivot.GetComponentsInChildren<Renderer>();
			foreach (Renderer r in renderers) r.enabled = true;
		}
	}

    // -------------------------------------------------------------------------------------------------------------------

    void UpdateObjectPosition(ref xmgMarkerInfo markerData)
    {
        Quaternion quatRot = Quaternion.Euler(0, 0, 0);
#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
		if (Screen.orientation == ScreenOrientation.Portrait) 
            quatRot = Quaternion.Euler(0, 0, -90);
        else if (Screen.orientation == ScreenOrientation.LandscapeRight)
            quatRot = Quaternion.Euler(0, 0, 180);
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            quatRot = Quaternion.Euler(0, 0, 90);
#endif
        int pivotIndex = markerData.markerID;
        if (pivotIndex < visionParameters.ObjectPivotLinks.Count &&
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot)
        {
            Vector3 position = markerData.position;
            position.x *= videoParameters.VideoPlaneScale;
            position.y *= videoParameters.VideoPlaneScale;
            Quaternion quat = Quaternion.Euler(markerData.euler);
#if UNITY_IOS

			if (videoParameters.UseFrontal)
				{
					position.x = -position.x;
					position.y = -position.y;
					quat.x = -quat.x;
					quat.y = -quat.y;
					
				}
#endif
			if (videoParameters.MirrorVideo)
            {
                quat.y = -quat.y;
                quat.z = -quat.z;
                position.x = -position.x;
            }
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localPosition = quatRot * position;
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localRotation = quatRot * quat;
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localScale = new Vector3(videoParameters.VideoPlaneScale, videoParameters.VideoPlaneScale, videoParameters.VideoPlaneScale);
            //xmgDebug.m_debugMessage = position.ToString() + visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localRotation.ToString();
        }
    }
}
