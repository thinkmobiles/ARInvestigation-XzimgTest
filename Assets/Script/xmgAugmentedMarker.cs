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
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class xmgAugmentedMarker : xmgAugmentedVisionBase
{
#if UNITY_ANDROID ||UNITY_IOS
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
                // __ Prepare and open videocapture
                m_myWebCamEngine = (xmgWebCamTexture)gameObject.AddComponent(typeof(xmgWebCamTexture));
                m_myWebCamEngine.CaptureWidth = videoParameters.GetVideoCaptureWidth();
                m_myWebCamEngine.CaptureHeight = videoParameters.GetVideoCaptureHeight();
                m_myWebCamEngine.MirrorVideo = videoParameters.MirrorVideo;
                m_myWebCamEngine.CameraFOVX = videoParameters.CameraFOVX;
                m_webcamTexture = m_myWebCamEngine.CreateVideoCapturePlane(videoParameters.VideoPlaneScale, videoParameters.videoPlaneFittingMode, videoParameters.videoCaptureIndex);
                m_myWebCamEngine.UseFrontal = false;
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

				// __ Initialize detection engine
				float fovx_radian = videoParameters.CameraFOVX * 3.1415f / 180.0f;
				int res = xmgAugmentedVisionBridge.xzimgMarkerInitialize(captureWidth, captureHeight, videoParameters.GetProcessingWidth(captureWidth), videoParameters.GetProcessingHeight(captureHeight), fovx_radian);
				Debug.Log("initialization " + res);
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
            xmgAugmentedVisionBridge.xzimgMarkerInitialize(videoOptions.resolution_mode, videoParameters.UseFrontal, fovx_radian, true);
        xmgAugmentedVisionBridge.SetFilterStrength(visionParameters.FilterStrength);
        xmgAugmentedVisionBridge.SetMarkerType(visionParameters.MarkerType);

#elif UNITY_IOS
				
				if (videoParameters.UseFrontal)
					videoParameters.MirrorVideo = true;
			videoOptions.resolution_mode = videoParameters.videoCaptureMode;
				videoOptions.frontal = 0;
				if (videoParameters.UseFrontal) videoOptions.frontal = 1;
			videoOptions.focus_mode = 1;			// Continuously updates = 1
			videoOptions.exposure_mode = 1;			// Continuously updates = 1
			videoOptions.while_balance_mode = 1;	// Continuously updates = 1
			
			PrepareBackgroundPlane(videoOptions.frontal==1);

			// Create the texture to display video stream
			imgTexture = new Texture2D(videoParameters.GetVideoCaptureWidth(), videoParameters.GetVideoCaptureHeight(), TextureFormat.BGRA32, false);
			GetComponent<Renderer>().material.mainTexture = imgTexture;
			videoOptions.texture = imgTexture.GetNativeTexturePtr();

            float fovx_radian = videoParameters.CameraFOVX * 3.1415f / 180.0f;
			xmgAugmentedVisionBridge.xzimgMarkerInitialize(ref videoOptions, videoParameters.GetProcessingWidth(), videoParameters.GetProcessingHeight(), fovx_radian);
			
#endif

        if (visionParameters.ObjectPivotLinks.Count > 0)
            {
                int[] arrIndices = new int[visionParameters.ObjectPivotLinks.Count];
                float[] arrObjectWidth = new float[visionParameters.ObjectPivotLinks.Count];
                for (int i = 0; i < visionParameters.ObjectPivotLinks.Count; i++)
                {
                    arrIndices[i] = visionParameters.ObjectPivotLinks[i].MarkerIndex;
                    if (visionParameters.ObjectPivotLinks[i].ObjectRealWidth <= 0)
                        visionParameters.ObjectPivotLinks[i].ObjectRealWidth = 1;
                    arrObjectWidth[i] = visionParameters.ObjectPivotLinks[i].ObjectRealWidth;
                }
                xmgAugmentedVisionBridge.xzimgMarkerSetActiveIndices(ref arrIndices[0], ref arrObjectWidth[0], arrIndices.Length);
            }

        mInitialized = true;
    }

    // -------------------------------------------------------------------------------------------------------------------

    void OnDisable() 
	{
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
		m_myWebCamEngine.ReleaseVideoCapturePlane();
#endif
		xmgAugmentedVisionBridge.xzimgMarkerRelease();
	}

    // -------------------------------------------------------------------------------------------------------------------

    void Update()
    {
        if (!mInitialized) return;
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)

        if (!m_myWebCamEngine) return;

        bool isPlaying = m_webcamTexture.isPlaying;
        if (!isPlaying) return;

        if (!m_myWebCamEngine.GetData()) return;

        m_image.m_imageData = m_myWebCamEngine.m_PixelsHandle.AddrOfPinnedObject(); 
		xmgAugmentedVisionBridge.xzimgMarkerDetect(ref m_image, visionParameters.MarkerType, visionParameters.FilterStrength);
        m_myWebCamEngine.ApplyTexture();
#elif (UNITY_IPHONE)
		xmgAugmentedVisionBridge.xzimgMarkerDetect(visionParameters.MarkerType, visionParameters.FilterStrength);
			
		// to prevent Unity bug on iOS
			UpdateBackgroundPlaneOrientation(videoParameters.UseFrontal);
#elif (UNITY_ANDROID)
		xmgAugmentedVisionBridge.xzimgAugmentedVisionDetect(videoOptions.texture, videoOptions.texture_uv);
#endif

        DisableObjects();

        int iNbrOfDetection = xmgAugmentedVisionBridge.xzimgMarkerGetNumber();
        UpdateDebugDisplay(iNbrOfDetection);
        if (iNbrOfDetection > 0)
        {
            for (int i = 0; i < iNbrOfDetection; i++)
            {
                xmgMarkerInfo markerInfo = new xmgMarkerInfo();
                xmgAugmentedVisionBridge.xzimgMarkerGetInfoForUnity(i, ref markerInfo);
                int indexPivot = GetPivotIndex(markerInfo.markerID);
                if (indexPivot >= 0)
                {
                    EnableObject(indexPivot);
                    UpdateObjectPosition(ref markerInfo);
                }
            }
        }
	}

    // -------------------------------------------------------------------------------------------------------------------

    private int GetPivotIndex(int MarkerIndex)
	{
		
		for (int i = 0; i < visionParameters.ObjectPivotLinks.Count; i++)
			if (visionParameters.ObjectPivotLinks[i].MarkerIndex == MarkerIndex) return i;
		return MarkerIndex;
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
		if (indexPivot < visionParameters.ObjectPivotLinks.Count && visionParameters.ObjectPivotLinks[indexPivot].ScenePivot)
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
		int pivotIndex = GetPivotIndex(markerData.markerID);
		if (pivotIndex < visionParameters.ObjectPivotLinks.Count && visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot)
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
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localPosition = quatRot*position;
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localRotation = quatRot * quat;
            visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localScale = new Vector3(videoParameters.VideoPlaneScale, videoParameters.VideoPlaneScale, videoParameters.VideoPlaneScale);
            //xmgDebug.m_debugMessage = position.ToString() + visionParameters.ObjectPivotLinks[pivotIndex].ScenePivot.transform.localRotation.ToString();
        }
	}
}
