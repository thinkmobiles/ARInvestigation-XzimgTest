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

public class xmgWebCamTexture : MonoBehaviour 
{
    public int CaptureWidth = 640, CaptureHeight = 480;
	
	public bool UseFrontal = false;
	public bool MirrorVideo = true;
	public float CameraFOVX = 60.0f;
    
	// private variables
	private int VideoPlaneDistance = 750;
	private WebCamTexture m_webcamTexture = null;
    private Color32[] m_data;
	public GCHandle m_PixelsHandle;
	private Texture2D l_texture;
	private bool m_initialized = false;

    private Mesh createPlanarMesh()
    {
        Vector3[] Vertices = new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) };
        Vector2[] UV = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        int[] Triangles = new int[] { 0, 1, 2, 0, 2, 3 };

        Mesh mesh = new Mesh();
		mesh.vertices = Vertices;
		mesh.triangles = Triangles;
		mesh.uv = UV;
		return mesh;
    }

	public WebCamTexture CreateVideoCapturePlane(float screenScaleFactor, xmgVideoPlaneFittingMode fittingMode, int cameraIdx)
	{
        // Reset some data
        String deviceName;
        Camera.main.clearFlags = CameraClearFlags.Skybox;
		Camera.main.transform.position = new Vector3(0, 0, 0);
		Camera.main.transform.eulerAngles = new Vector3(0, 0, 0);
		transform.position = new Vector3(0, 0, 0);

        int nbDevices = WebCamTexture.devices.Length;
        if (cameraIdx == -1 || cameraIdx >= nbDevices)
        {
			for (int cameraIndex = 0; cameraIndex < WebCamTexture.devices.Length; cameraIndex++)
            {
				// We want the back camera
				if (!WebCamTexture.devices [cameraIndex].isFrontFacing && !UseFrontal) {
					deviceName = WebCamTexture.devices [cameraIndex].name;
					m_webcamTexture = new WebCamTexture (deviceName, CaptureWidth, CaptureHeight, 30);
					break;
				} else if (WebCamTexture.devices [cameraIndex].isFrontFacing && UseFrontal) {
					deviceName = WebCamTexture.devices [cameraIndex].name;
					m_webcamTexture = new WebCamTexture (deviceName, CaptureWidth, CaptureHeight, 30);
					break;
				}
			}
		}
        else
        {
			deviceName = WebCamTexture.devices [cameraIdx].name;
			m_webcamTexture = new WebCamTexture (deviceName, CaptureWidth, CaptureHeight, 30);
		}

		if (!m_webcamTexture)	// If camera opening is still unsuccessful try with the first index
		{
			if (!UseFrontal || WebCamTexture.devices.Length == 1)
				deviceName = WebCamTexture.devices[0].name;
			else
				deviceName = WebCamTexture.devices[1].name;
			m_webcamTexture = new WebCamTexture(deviceName, CaptureWidth, CaptureHeight, 30);
		}

		if (!m_webcamTexture)
			Debug.Log("No camera detected!");
		else
		{
			m_webcamTexture.Play();

			int captureWidth = m_webcamTexture.width, captureHeight = m_webcamTexture.height;
			if (captureWidth < 100) 
			{
				// Unity BUG MACOSX && WEBGL
				captureWidth = m_webcamTexture.requestedWidth;
				captureHeight = m_webcamTexture.requestedHeight;
			}
            // Determine aspect ratios and camera fov value
			float video_AR = (float)captureWidth / (float)captureHeight;
            float screen_AR = (float)Screen.width / (float)Screen.height;


            double trackingCamera_fovv_radian = xmgTools.ConvertToRadian((double)CameraFOVX);
            double trackingCamera_fovh_radian;
            if (fittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
                trackingCamera_fovh_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovv_radian, (double)screen_AR);
            else
                trackingCamera_fovh_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovv_radian, (double)video_AR);
            Camera.main.fieldOfView = (float)xmgTools.ConvertToDegree(trackingCamera_fovh_radian);

            // Create the mesh (plane)
            Mesh mesh = createPlanarMesh();
			
            // Attach it to the current GO
			gameObject.AddComponent<MeshFilter>().mesh = mesh;
            
	        // Assign video texture to the renderer
	        if (!GetComponent<Renderer>())
				gameObject.AddComponent<MeshRenderer>();

			// Modify Game Object's position & orientation according to the main camera's focal
	        transform.position = new Vector3(0, 0, VideoPlaneDistance);
            double scale_u, scale_v;
            if (fittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
            {
                double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)Camera.main.fieldOfView);
                double mainCamera_fovh_radian = xmgTools.ConvertVerticalFovToHorizontalFov(mainCamera_fovv_radian, (double)screen_AR);
                scale_u = (VideoPlaneDistance * Math.Tan(mainCamera_fovh_radian /2.0));
                scale_v = (VideoPlaneDistance * Math.Tan(mainCamera_fovh_radian /2.0) * 1.0/ video_AR);
            }
            else
            {
                double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)Camera.main.fieldOfView);
                scale_u = (VideoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0) * video_AR);
                scale_v = (VideoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0));
            }
			
			if (MirrorVideo)
                transform.localScale = new Vector3((float)-scale_u, (float)scale_v, (float)1.0);
	        else
                transform.localScale = new Vector3((float)scale_u, (float)scale_v, (float)1.0);
            transform.localScale *= screenScaleFactor;

            transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);

            // Apply shader to set the texture as the background
            GetComponent<Renderer>().material = new Material(Shader.Find("Custom/VideoShader"));
       
		}
		return m_webcamTexture;
    }

	public void InitializeDataStructures()
	{
		m_initialized = true;
			m_data = new Color32[m_webcamTexture.width * m_webcamTexture.height];
			m_PixelsHandle = GCHandle.Alloc(m_data, GCHandleType.Pinned);
			
			//GetComponent<Renderer>().material.mainTexture = m_webcamTexture;
			l_texture = new Texture2D(m_webcamTexture.width, m_webcamTexture.height, TextureFormat.RGBA32, false);
			GetComponent<Renderer>().material.mainTexture = l_texture;
    }

	public bool GetData()
	{
		if (m_webcamTexture) 
		{
			if (m_webcamTexture.didUpdateThisFrame)
            {
				if (m_webcamTexture.width < 100) return false;
				else if (!m_initialized) InitializeDataStructures();

                m_webcamTexture.GetPixels32(m_data);
                return true;
            } 
			else 
			{ 
				return false;
			}
		}
		return false;

    }

    public void ReleaseVideoCapturePlane()
    {
        m_PixelsHandle.Free();
        if (m_webcamTexture)
        {
            m_webcamTexture.Stop();
            m_webcamTexture = null;
        }

    }

    public void ApplyTexture()
    {
        // don't change - sequenced to avoid crash
        l_texture.SetPixels32(m_data);
        l_texture.Apply();

    }
}
