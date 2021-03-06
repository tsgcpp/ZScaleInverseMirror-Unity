﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// WARNING: `UWP, HDRPは非対応
/// </summary>
[RequireComponent(typeof(Renderer))]
public class ZInverseMirror : MonoBehaviour
{
    [SerializeField]
    [Tooltip("positionをミラーの位置、forwardをミラーの反射方向とするTransform")]
    private Transform mirror;

    [SerializeField]
    [Tooltip("鏡用カメラのカリングマスク")]
    private LayerMask cullingMask;

    private Camera mirrorCamera;

    private Material rendererMaterial;

    private RenderTexture renderTexture = null;

    private readonly Matrix4x4 zReverseMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));

    private void Awake()
    {
        mirrorCamera = CreateCamera();

        // 単一マテリアルのみ対象
        rendererMaterial = GetComponent<Renderer>().sharedMaterial;
    }

    void OnWillRenderObject()
    {
        var camera = Camera.current;
        if (camera == null || camera == mirrorCamera) {
            return;
        }

#if UNITY_EDITOR
        if (camera == SceneView.lastActiveSceneView.camera) {
            return;
        }
#endif

        if (renderTexture == null) {
            renderTexture = CreateMirrorTexture();
            mirrorCamera.targetTexture = renderTexture;
            
            // targetTextureを設定するとprojectionMatrixが更新されてしまうため、
            // 対象のカメラのもので上書き
            mirrorCamera.projectionMatrix = camera.projectionMatrix;

            // 影の描画に使用されるため対象のカメラに合わせる
            mirrorCamera.fieldOfView = camera.fieldOfView;
            mirrorCamera.aspect = camera.aspect;
        }

        mirrorCamera.worldToCameraMatrix = CalculateMirrorViewMatrix(camera);
        rendererMaterial.mainTexture = renderTexture;

        bool oldCulling = GL.invertCulling;
        GL.invertCulling = true;
        mirrorCamera.Render();
        GL.invertCulling = oldCulling;
    }

    private void OnBecameInvisible()
    {
        if (renderTexture == null) {
            return;
        }

        rendererMaterial.mainTexture = null;
        mirrorCamera.targetTexture = null;
        ReleaseMirrorTexture(renderTexture);

        renderTexture = null;
    } 

    private Camera CreateCamera()
    {
        var cameraObj = new GameObject("Mirror Camera", typeof(Camera));
        
        // Cameraの親オブジェクトのスケールで影がおかしくなるため、親を設定しない
        // cameraObj.transform.parent = this.transform;

        var camera = cameraObj.GetComponent<Camera>();
        camera.enabled = false;
        camera.cullingMask = cullingMask;

        return camera;
    }

    private Matrix4x4 CalculateMirrorViewMatrix(Camera camera)
    {
        var mirrorMatrix =
            mirror.localToWorldMatrix *
            zReverseMatrix *
            mirror.worldToLocalMatrix;

        return camera.worldToCameraMatrix * mirrorMatrix;
    }

    private RenderTexture CreateMirrorTexture()
    {
        Camera mainCamera = Camera.main;

        return RenderTexture.GetTemporary(
            width: mainCamera.pixelWidth,
            height: mainCamera.pixelHeight);
    }

    private void ReleaseMirrorTexture(RenderTexture texture)
    {
        RenderTexture.ReleaseTemporary(texture);
    }
}
