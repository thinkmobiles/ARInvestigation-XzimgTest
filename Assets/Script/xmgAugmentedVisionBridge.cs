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
using System.Collections.Generic;	//List
using System.Text;

// --------------------------------------------------------------------------------------------------------------------------

[StructLayout(LayoutKind.Sequential)]
public struct xmgImage
{
    public int m_width;
    public int m_height;

    public IntPtr m_imageData;

    /** Filed not used for Unity */
    public int m_widthstep;

    /** 0: Black and White, 1: Color RGB, 2: Color BGR, 3: Color RGBA, 4: Color ARGB */
    public int m_colorType;

    /** 0: unsigned char, 1: float, 2: double */
    public int m_type;

    /** Has the image to be flipped horinzontally */
    public int m_flippedH;
}

[StructLayout(LayoutKind.Sequential)]
public struct xmgMarkerInfo
{
    public int markerID;
    public Vector3 position;
    public Vector3 euler;
    public Quaternion rotation;
}

[StructLayout(LayoutKind.Sequential)]
public struct xmgFaceApiVideoCapture
{
    public int resolution_mode;         // 0 is 320x240; 1, is 640x480; 2 is 720p
    public int frontal;                 // 0 is frontal; 1 is back
    public int focus_mode;              // 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
    public int exposure_mode;           // 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
    public int while_balance_mode;      // 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
	public System.IntPtr texture;       // texture for background video
    public System.IntPtr texture_uv;    // texture (uv) for background video
}

// --------------------------------------------------------------------------------------------------------------------------

// Import marker detection functions
public class xmgAugmentedVisionBridge
{
#if (UNITY_ANDROID && !UNITY_EDITOR && !UNITY_STANDALONE)

    public static void xzimgPause(bool paused)
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        jo.Call("xzimgPause", paused);
    }
#endif
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Fonctions for Marker Detection
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------


#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgMarkerInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerInitialize([In][Out] ref xmgFaceApiVideoCapture videoOptions, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_ANDROID
    public static int xzimgMarkerInitialize(int cameraMode, bool isFrontal, double fovXRadian, bool highQuality)
    {

		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgMarkerStartCameraAndInitialize", cameraMode, isFrontal, fovXRadian, highQuality);
        return 1;
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerRelease();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern void xzimgMarkerRelease();
#elif UNITY_ANDROID
    public static void xzimgMarkerRelease()
    {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgRelease");
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerRelease();
#endif

    // --------------------------------------------------------------------------------------------------------------------------

    public static void SetMarkerType(int markerType)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgMarkerSetMarkerType", markerType);
#endif
    }

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);
#elif UNITY_ANDROID
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);    
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerSetActiveIndices(ref int arrIndices, ref float arrMarkerWidth, int nbrOfIndices);
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgMarkerDetect([In][Out] ref xmgImage imageIn, int markerType, int filterStrenght);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerDetect(int markerType, int filterStrenght);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerDetect([In][Out] ref xmgImage imageIn, int markerType, int filterStrengh);
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgMarkerGetNumber();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerGetNumber();
#elif (UNITY_ANDROID)
	public static int xzimgMarkerGetNumber()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        return jo.Call<int>("xzimgGetDetectedNumber");
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgMarkerGetNumber();
#endif

    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif (UNITY_ANDROID)
	public static void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo)
    {
        
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        float [] vctPose = jo.Call<float[]>("xzimgGetPose", iId);
        if (vctPose != null)
        {
            for (int i=0; i<3; i++)markerInfo.position[i] = vctPose[i];
            for (int i=0; i<3; i++)markerInfo.euler[i] = vctPose[i+3];
        }
        markerInfo.markerID = jo.Call<int>("xzimgGetDetectedTargetIdx", iId);

    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgMarkerGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Fonctions for Natural Image Detection
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageInitialize([In][Out] ref xmgFaceApiVideoCapture videoOptions, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_ANDROID
    public static int xzimgNaturalImageInitialize(int cameraMode, bool isFrontal, double fovXRadian, bool highQuality)
    {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgNaturalImageStartCameraAndInitialize", cameraMode, isFrontal, fovXRadian, highQuality);
        return 1;
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgNaturalImageRelease();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern void xzimgNaturalImageRelease();
#elif UNITY_ANDROID
    public static void xzimgNaturalImageRelease()
    {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgRelease");
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgNaturalImageRelease();
#endif


#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_ANDROID
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageFinalizeLearning();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageFinalizeLearning();
#elif UNITY_ANDROID
	[DllImport("xzimgAugmentedVision")]
	public static extern int xzimgNaturalImageFinalizeLearning();
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageFinalizeLearning();
#endif
	
#if (UNITY_EDITOR || UNITY_STANDALONE)
	[DllImport("xzimgAugmentedVision")]
	public static extern int xzimgNaturalImageTrack([In][Out] ref xmgImage imageIn, int filterStrenght);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageTrack(int filterStrenght);

#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageTrack([In][Out] ref xmgImage imageIn, int filterStrenght);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageBuildClassifier([In] ref byte image, int length, int maxWidthOrHeight);
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFillNaturalImageClassifier([In][Out] ref byte classifier);
#endif


#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgNaturalImageGetNumber();
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageGetNumber();
#elif (UNITY_ANDROID)
	public static int xzimgNaturalImageGetNumber()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        return jo.Call<int>("xzimgGetDetectedNumber");
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageGetNumber();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern void xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif (UNITY_ANDROID)
 //   [DllImport("xzimgAugmentedVision")]
 //   public static extern void xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
	public static void xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo)
    {
        
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        float [] vctPose = jo.Call<float[]>("xzimgGetPose", iId);
        if (vctPose != null)
        {
            for (int i=0; i<3; i++)markerInfo.position[i] = vctPose[i];
            for (int i=0; i<3; i++)markerInfo.euler[i] = vctPose[i+3];
        }
         markerInfo.markerID = jo.Call<int>("xzimgGetDetectedTargetIdx", iId);
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgNaturalImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Fonctions for Framed-Image Detection
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFramedImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageInitialize([In][Out] ref xmgFaceApiVideoCapture videoOptions, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#elif UNITY_ANDROID
    public static void xzimgFramedImageInitialize(int cameraMode, bool isFrontal, double fovXRadian, bool highQuality)
    {

		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgFramedImageStartCameraAndInitialize", cameraMode, isFrontal, fovXRadian, highQuality);
}
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageInitialize(int CaptureWidth, int CaptureHeight, int iProcessingWidth, int iProcessingHeight, float fovXRadian);
#endif


#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgFramedImageRelease();
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
    public static extern void xzimgFramedImageRelease();
#elif UNITY_ANDROID
    public static void xzimgFramedImageRelease()
    {
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgRelease");
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgFramedImageRelease();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_IPHONE
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#else
    [DllImport("xzimgAugmentedVision")]
	public static extern int xzimgFramedImageAddTarget(IntPtr bytesData, int iLength, float arrMarkerWidth);
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
	[DllImport("xzimgAugmentedVision")]
	public static extern int xzimgFramedImageDetect([In][Out] ref xmgImage imageIn, int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageDetect(int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageDetect([In][Out] ref xmgImage imageIn, int iFilterStrenght, int bRecursiveTracking, int bRecursiveIdentification, int identificationMode);
#endif



#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFramedImageGetNumber();
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageGetNumber();
#elif (UNITY_ANDROID)
	public static int xzimgFramedImageGetNumber()
    {
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        return jo.Call<int>("xzimgGetDetectedNumber");
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern int xzimgFramedImageGetNumber();
#endif

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif UNITY_IPHONE    
	[DllImport ("__Internal")] 
	public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#elif (UNITY_ANDROID)
  //  [DllImport("xzimgAugmentedVision")]
 //   public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
	public static void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo)
    {
        
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
        float [] vctPose = jo.Call<float[]>("xzimgGetPose", iId);
        if (vctPose != null)
        {
            for (int i=0; i<3; i++)markerInfo.position[i] = vctPose[i];
            for (int i=0; i<3; i++)markerInfo.euler[i] = vctPose[i+3];
        }
        markerInfo.markerID = jo.Call<int>("xzimgGetDetectedTargetIdx", iId);
    }
#elif UNITY_WEBGL
	[DllImport ("__Internal")] 
	public static extern void xzimgFramedImageGetInfoForUnity(int iId, [In][Out] ref xmgMarkerInfo markerInfo);
#endif

    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------
    // Common functions
    // --------------------------------------------------------------------------------------------------------------------------
    // --------------------------------------------------------------------------------------------------------------------------

    public static void SetFilterStrength(int filterStrength)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("xzimgSetFilterStrength", filterStrength);
#endif
    }

    public static float[] xzimgAugmentedVisionDetect(System.IntPtr textureID, System.IntPtr uvTextureID)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		return jo.Call<float[]>("xzimgAugmentedVisionDetect", textureID.ToInt32(), uvTextureID.ToInt32());
#else
        return null;
#endif
    }

#if (UNITY_EDITOR || UNITY_STANDALONE)
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFramedImageBuildClassifier([In] ref byte image, int length, int maxWidthOrHeight);
    [DllImport("xzimgAugmentedVision")]
    public static extern int xzimgFillFramedImageClassifier([In][Out] ref byte classifier);
#endif
}


