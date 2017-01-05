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

/**
 * Common tool functions and properties for different vision components
 */
public class xmgAugmentedVisionBase : MonoBehaviour
{
    public xmgVideoCaptureParameters videoParameters;
    public xmgVisionParameters visionParameters;
    protected String m_debugStatus = "";

#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
    protected WebCamTexture m_webcamTexture = null;
    protected xmgWebCamTexture m_myWebCamEngine = null;
    protected Color[] m_imageData;
    protected xmgImage m_image;
#endif
    /**
	 * Create a planar mesh and texture coordinates adapted for landscapeRight mode 
	 */
    public Mesh createPlanarMesh()
	{
		Vector3[] Vertices = new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) };
		//Vector2[] UV = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        Vector2[] UV = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        int[] Triangles = new int[] { 0, 1, 2, 0, 2, 3 };
		Mesh mesh = new Mesh();
		mesh.vertices = Vertices;
		mesh.triangles = Triangles;
		mesh.uv = UV;
		return mesh;
    }

	public void UpdateBackgroundPlaneOrientation(bool frontalCamera)
	{
		transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
		if (Screen.orientation == ScreenOrientation.Portrait) 
			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
		else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
#if UNITY_IOS
		if (frontalCamera)
		{
			
			transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
			if (Screen.orientation == ScreenOrientation.Portrait) 
				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
			else if (Screen.orientation == ScreenOrientation.LandscapeRight)
				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
			else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
		}
#endif
		if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			Camera.main.fieldOfView = (float)videoParameters.GetPortraitMainCameraFovV();

	}

    public void PrepareBackgroundPlane(bool frontalCamera)
    {
		// Reset camera rotation and position
		Camera.main.transform.position = new Vector3(0, 0, 0);
		Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);

		// Create a mesh to apply video texture
		Mesh mesh = createPlanarMesh();
		gameObject.AddComponent<MeshFilter>().mesh = mesh;

        // Rotate the mesh according to current screen orientation
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        if (Screen.orientation == ScreenOrientation.Portrait) 
			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
		else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
			gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);

        // Prepare ratios and camera fov
        Camera.main.fieldOfView = (float)videoParameters.GetMainCameraFovV();
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            Camera.main.fieldOfView = (float)videoParameters.GetPortraitMainCameraFovV();

        // Modify Game Object's position & orientation
        double VideoPlaneDistance = 750;
        gameObject.transform.position = new Vector3(0, 0, (float)VideoPlaneDistance);
        double[] scale = videoParameters.GetVideoPlaneScale(VideoPlaneDistance);
        
        if (videoParameters.MirrorVideo)
            transform.localScale = new Vector3((float)scale[0], (float)scale[1], (float)1);
        else
            transform.localScale = new Vector3((float)-scale[0], (float)scale[1], (float)1);
        transform.localScale *= videoParameters.VideoPlaneScale;

        // __ Assign video texture to the renderer
        if (!GetComponent<Renderer>())
            gameObject.AddComponent<MeshRenderer>();


#if UNITY_IOS
        gameObject.GetComponent<Renderer>().material = new Material( Shader.Find("Custom/VideoShader"));
#elif UNITY_ANDROID
        gameObject.GetComponent<Renderer>().material = new Material(Shader.Find("Custom/YShader"));
#endif

    }


    void OnApplicationPaused(bool pauseStatus)
    {
        // Do something here if you need
    }

    void OnApplicationFocus(bool status)
    {
        // track when losing/recovering focus
#if (UNITY_ANDROID && !UNITY_EDITOR && !UNITY_STANDALONE)
            xmgAugmentedVisionBridge.xzimgPause(!status);
#endif
#if (UNITY_STANDALONE || UNITY_EDITOR)
        if (m_webcamTexture != null && status == false)
            m_webcamTexture.Stop();     // you can pause as well
        else if (m_webcamTexture != null && status == true)
            m_webcamTexture.Play();
#endif
    }

    void OnGUI()
    {
        if (videoParameters.ScreenDebug)
        {
            GUILayout.Label(xmgDebug.m_debugMessage);
            //if (Input.deviceOrientation == DeviceOrientation.Portrait) GUILayout.Label("Portrait");
            //if (Input.deviceOrientation == DeviceOrientation.PortraitUpsideDown) GUILayout.Label("PortraitUpsideDown");
            //if (Input.deviceOrientation == DeviceOrientation.LandscapeLeft) GUILayout.Label("LandscapeLeft");
            //if (Input.deviceOrientation == DeviceOrientation.LandscapeRight) GUILayout.Label("LandscapeRight");
            //if (Input.deviceOrientation == DeviceOrientation.FaceUp) GUILayout.Label("FaceUp");
            //if (Input.deviceOrientation == DeviceOrientation.FaceDown) GUILayout.Label("FaceDown");

            if (Screen.orientation == ScreenOrientation.Unknown) GUILayout.Label("Unknown");
            if (Screen.orientation == ScreenOrientation.Portrait) GUILayout.Label("Portrait");
            if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) GUILayout.Label("PortraitUpsideDown");
            if (Screen.orientation == ScreenOrientation.LandscapeLeft) GUILayout.Label("LandscapeLeft");
            if (Screen.orientation == ScreenOrientation.LandscapeRight) GUILayout.Label("LandscapeRight");
        }
    }

    public void UpdateDebugDisplay(int iDetected)
    {
        if (iDetected > 0)
        {
            xmgDebug.m_debugMessage = "Marker Detected";
        }
        else if (iDetected == -11)
            xmgDebug.m_debugMessage = "Protection Alert - Wait or restart";
        else
            xmgDebug.m_debugMessage = "Marker not Detected";
    }
}

public class xmgTools
{
    static public float ConvertToRadian(float degreeAngle )
    {
        return (degreeAngle * ((float)Math.PI / 180.0f));
    }
    static public double ConvertToRadian(double degreeAngle)
    {
        return (degreeAngle * (Math.PI / 180.0f));
    }
    static public float ConvertToDegree(float degreeAngle)
    {
        return (degreeAngle * (180.0f / (float)Math.PI));
    }
    static public double ConvertToDegree(double degreeAngle)
    {
        return (degreeAngle * (180.0f / Math.PI));
    }
    static public double ConvertHorizontalFovToVerticalFov(double radianAngle, double aspectRatio)
    {
        return ( Math.Atan(1.0 / aspectRatio * Math.Tan(radianAngle/2.0)) * 2.0);
    }

    static public double ConvertVerticalFovToHorizontalFov(double radianAngle, double aspectRatio)
    {
        return (Math.Atan(aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
    }
}