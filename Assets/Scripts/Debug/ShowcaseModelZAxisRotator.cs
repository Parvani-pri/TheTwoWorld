using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach this component to an empty GameObject in the Showcase scene.
/// It rotates every detected 3D model root around its own local Z axis at runtime.
/// </summary>
public sealed class ShowcaseModelZAxisRotator : MonoBehaviour
{
    [SerializeField, Tooltip("Enable or disable rotation for all detected Showcase model roots.")]
    private bool rotationEnabled = true;

    [SerializeField, Min(0f), Tooltip("Rotation speed in degrees per second around each model root's local Z axis.")]
    private float rotationSpeed = 90f;

    [SerializeField, Tooltip("Number of 3D model roots currently detected in this scene.")]
    private int detectedModelRootCount;

    private readonly List<Transform> modelRoots = new List<Transform>();
    private Scene scene;

    private void OnEnable()
    {
        RefreshModelRoots();
    }

    private void Start()
    {
        // Run once more after all scene objects have completed their initialization.
        RefreshModelRoots();
    }

    private void LateUpdate()
    {
        if (scene != gameObject.scene)
            RefreshModelRoots();

        if (!rotationEnabled || rotationSpeed <= 0f)
            return;

        foreach (Transform modelRoot in modelRoots)
        {
            if (modelRoot != null)
                modelRoot.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime, Space.Self);
        }
    }

    [ContextMenu("Refresh Model Roots")]
    private void RefreshModelRoots()
    {
        scene = gameObject.scene;
        modelRoots.Clear();

        if (!scene.IsValid() || !scene.isLoaded)
            return;

        foreach (GameObject rootObject in scene.GetRootGameObjects())
        {
            // Detect actual 3D geometry, while excluding camera, light, UI, and
            // EventSystem roots that do not contain a MeshRenderer or SkinnedMeshRenderer.
            if (ContainsModelRenderer(rootObject))
                modelRoots.Add(rootObject.transform);
        }

        detectedModelRootCount = modelRoots.Count;
        Debug.Log($"[ShowcaseModelZAxisRotator] Detected {detectedModelRootCount} 3D model root(s) in '{scene.name}'.", this);
    }

    private static bool ContainsModelRenderer(GameObject rootObject)
    {
        return rootObject.GetComponentInChildren<MeshRenderer>(true) != null ||
               rootObject.GetComponentInChildren<SkinnedMeshRenderer>(true) != null;
    }
}
