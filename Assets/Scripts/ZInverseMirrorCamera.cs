using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ZInverseMirrorCamera : MonoBehaviour
{
    [SerializeField]
    private Camera targetCamera;

    private Camera selfCamera;

    [SerializeField]
    [Tooltip("positionをミラーの位置、forwardをミラーの反射方向とするTransform")]
    private Transform mirror;

    readonly Matrix4x4 zReverseMatrix = Matrix4x4.Scale(new Vector3(1, 1, -1));

    private bool oldCulling;

    void Awake()
    {
        selfCamera = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        var mirrorMatrix =
            mirror.localToWorldMatrix *
            zReverseMatrix *
            mirror.worldToLocalMatrix;

        selfCamera.worldToCameraMatrix =
            targetCamera.worldToCameraMatrix *
            mirrorMatrix;
    }

    void OnPreRender()
    {
        oldCulling = GL.invertCulling;
        GL.invertCulling = true;
    }

    void OnPostRender()
    {
        GL.invertCulling = oldCulling;
    }
}
