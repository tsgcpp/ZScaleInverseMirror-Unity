using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private LayerMask layerMask;

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

        mirrorCamera.worldToCameraMatrix = CalculateMirrorViewMatrix(camera);
        mirrorCamera.targetTexture = renderTexture;

        if (renderTexture == null) {
            renderTexture = CreateMirrorTexture();
        }
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
        ReleaseMirrorTexture(renderTexture);

        renderTexture = null;
    } 

    private Camera CreateCamera()
    {
        var cameraObj = new GameObject("Mirror Camera", typeof(Camera));
        cameraObj.transform.parent = this.transform;
        cameraObj.hideFlags = HideFlags.HideAndDontSave;

        var camera = cameraObj.GetComponent<Camera>();
        camera.enabled = false;
        camera.cullingMask = layerMask;

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
        return RenderTexture.GetTemporary(
            width: Screen.width,
            height: Screen.height);
    }

    private void ReleaseMirrorTexture(RenderTexture texture)
    {
        RenderTexture.ReleaseTemporary(texture);
    }
}
