using System;
using System.IO;
using System.Linq;
using TwoWorlds.Progress;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.EditorTools
{
    public static class SetupEnterYinPaperBurnTransition
    {
        const string MenuPath = "Tools/Two Worlds/Setup Enter Yin Paper Burn Transition";
        const string IdleClipPath = "Assets/VFX/paper/idle.anim";
        const string BurnClipPath = "Assets/VFX/paper/burn.anim";
        const string IdleSheetPath = "Assets/VFX/paper-idle1-v3.png";
        const string PrefabPath = "Assets/Prefab/VFX/EnterYinPaperBurnTransition.prefab";

        [MenuItem(MenuPath)]
        public static void Run()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.name != "MainLobby")
            {
                Debug.LogError("[SetupEnterYinPaperBurnTransition] Open MainLobby scene first.");
                return;
            }

            var aiSystem = GameObject.Find("AISystem");
            if (aiSystem == null)
            {
                Debug.LogError("[SetupEnterYinPaperBurnTransition] AISystem not found.");
                return;
            }

            var transitionRoot = EnsureTransitionRoot(aiSystem.transform);
            RebuildTransitionHierarchy(transitionRoot);
            EnsurePrefab(transitionRoot);
            HideTransitionVisuals(transitionRoot);

            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("[SetupEnterYinPaperBurnTransition] Enter Yin paper-burn transition setup complete.");
        }

        static void HideTransitionVisuals(GameObject transitionRoot)
        {
            var backdrop = transitionRoot.transform.Find("BackdropCanvas");
            if (backdrop != null)
            {
                var canvas = backdrop.GetComponent<Canvas>();
                if (canvas != null)
                    canvas.enabled = false;
            }

            var paperVisual = transitionRoot.transform.Find("BackdropCanvas/PaperBurnVisual");
            if (paperVisual != null)
                paperVisual.gameObject.SetActive(false);
        }

        static GameObject EnsureTransitionRoot(Transform parent)
        {
            var existing = parent.Find("EnterYinPaperBurnTransition");
            if (existing != null)
                return existing.gameObject;

            var go = new GameObject("EnterYinPaperBurnTransition");
            go.transform.SetParent(parent, false);
            return go;
        }

        static void RebuildTransitionHierarchy(GameObject transitionRoot)
        {
            for (var i = transitionRoot.transform.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(transitionRoot.transform.GetChild(i).gameObject);

            transitionRoot.transform.localPosition = Vector3.zero;
            transitionRoot.transform.localRotation = Quaternion.identity;
            transitionRoot.transform.localScale = Vector3.one;

            var backdropCanvasGo = CreateUiObject("BackdropCanvas", transitionRoot.transform);
            var backdropCanvas = backdropCanvasGo.AddComponent<Canvas>();
            backdropCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            backdropCanvas.sortingOrder = 32767;
            backdropCanvasGo.AddComponent<CanvasScaler>();
            backdropCanvasGo.AddComponent<GraphicRaycaster>();

            var backdropImageGo = CreateUiObject("Backdrop", backdropCanvasGo.transform);
            var backdropImage = backdropImageGo.AddComponent<Image>();
            backdropImage.color = Color.black;
            backdropImage.raycastTarget = true;
            StretchFull(backdropImageGo.GetComponent<RectTransform>());

            var paperVisualGo = CreateUiObject("PaperBurnVisual", backdropCanvasGo.transform);
            var paperRect = paperVisualGo.GetComponent<RectTransform>();
            paperRect.anchorMin = new Vector2(0.5f, 0.5f);
            paperRect.anchorMax = new Vector2(0.5f, 0.5f);
            paperRect.pivot = new Vector2(0.5f, 0.5f);
            paperRect.anchoredPosition = Vector2.zero;
            paperRect.sizeDelta = new Vector2(640f, 640f);

            var paperImage = paperVisualGo.AddComponent<Image>();
            paperImage.sprite = LoadFirstSprite(IdleSheetPath);
            paperImage.preserveAspect = true;
            paperImage.raycastTarget = false;

            var idleClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(IdleClipPath);
            var burnClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(BurnClipPath);

            var transition = transitionRoot.GetComponent<EnterYinPaperBurnTransition>() ??
                             transitionRoot.AddComponent<EnterYinPaperBurnTransition>();

            var so = new SerializedObject(transition);
            so.FindProperty("root").objectReferenceValue = transitionRoot;
            so.FindProperty("backdropCanvas").objectReferenceValue = backdropCanvas;
            so.FindProperty("paperImage").objectReferenceValue = paperImage;
            so.FindProperty("idleClip").objectReferenceValue = idleClip;
            so.FindProperty("burnClip").objectReferenceValue = burnClip;
            so.FindProperty("idleFrames").arraySize = 0;
            so.FindProperty("burnFrames").arraySize = 0;

            var idleFrames = ExtractSpritesFromClip(idleClip);
            var burnFrames = ExtractSpritesFromClip(burnClip);
            WriteSpriteArray(so.FindProperty("idleFrames"), idleFrames);
            WriteSpriteArray(so.FindProperty("burnFrames"), burnFrames);
            so.FindProperty("framesPerSecond").floatValue = 12f;
            so.FindProperty("paperDisplaySize").floatValue = 640f;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void WriteSpriteArray(SerializedProperty property, Sprite[] sprites)
        {
            property.arraySize = sprites.Length;
            for (var i = 0; i < sprites.Length; i++)
                property.GetArrayElementAtIndex(i).objectReferenceValue = sprites[i];
        }

        static Sprite[] ExtractSpritesFromClip(AnimationClip clip)
        {
            if (clip == null)
                return Array.Empty<Sprite>();

            foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
            {
                if (binding.propertyName != "m_Sprite")
                    continue;

                return AnimationUtility.GetObjectReferenceCurve(clip, binding)
                    .Select(key => key.value as Sprite)
                    .Where(sprite => sprite != null)
                    .ToArray();
            }

            return Array.Empty<Sprite>();
        }

        static void EnsurePrefab(GameObject transitionRoot)
        {
            var dir = Path.GetDirectoryName(PrefabPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            PrefabUtility.SaveAsPrefabAssetAndConnect(
                transitionRoot,
                PrefabPath,
                InteractionMode.AutomatedAction);
        }

        static GameObject CreateUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return go;
        }

        static Sprite LoadFirstSprite(string assetPath)
        {
            return AssetDatabase.LoadAllAssetsAtPath(assetPath)
                .OfType<Sprite>()
                .FirstOrDefault();
        }

        static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }
    }
}
