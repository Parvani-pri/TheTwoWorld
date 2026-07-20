using System;
using System.Collections;
using TwoWorlds.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TwoWorlds.Progress
{
    public class EnterYinPaperBurnTransition : MonoBehaviour
    {
        public static EnterYinPaperBurnTransition Instance { get; private set; }

        [SerializeField] GameObject root;
        [SerializeField] Canvas backdropCanvas;
        [SerializeField] Image paperImage;
        [SerializeField] AnimationClip idleClip;
        [SerializeField] AnimationClip burnClip;
        [SerializeField] Sprite[] idleFrames;
        [SerializeField] Sprite[] burnFrames;
        [SerializeField] float framesPerSecond = 12f;
        [SerializeField] float paperDisplaySize = 640f;

        bool isPlaying;

        public bool IsPlaying => isPlaying;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            ResolveReferences();
            HideImmediate();
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public static EnterYinPaperBurnTransition FindInstance()
        {
            if (Instance != null)
                return Instance;

            return FindFirstObjectByType<EnterYinPaperBurnTransition>(FindObjectsInactive.Include);
        }

        public void Play(int chapter, Action<int> onComplete)
        {
            if (isPlaying)
                return;

            ResolveReferences();

            if (!gameObject.activeInHierarchy)
                gameObject.SetActive(true);

            if (paperImage == null || !HasPlayableSequence())
            {
                Debug.LogWarning("[EnterYinPaperBurnTransition] UI paper visuals missing; loading yin level immediately.");
                onComplete?.Invoke(chapter);
                return;
            }

            StartCoroutine(PlayRoutine(chapter, onComplete));
        }

        bool HasPlayableSequence()
        {
            var hasIdle = HasFrames(idleFrames) || idleClip != null;
            var hasBurn = HasFrames(burnFrames) || burnClip != null;
            return hasIdle && hasBurn;
        }

        static bool HasFrames(Sprite[] frames) => frames != null && frames.Length > 0;

        IEnumerator PlayRoutine(int chapter, Action<int> onComplete)
        {
            isPlaying = true;
            ResolveReferences();
            Show();

            yield return PlaySequence(idleFrames, idleClip);
            yield return PlaySequence(burnFrames, burnClip);

            HideImmediate();
            isPlaying = false;
            onComplete?.Invoke(chapter);
        }

        IEnumerator PlaySequence(Sprite[] frames, AnimationClip clip)
        {
            if (HasFrames(frames))
            {
                yield return PlayFrameSequence(frames);
                yield break;
            }

            if (clip != null)
                yield return PlayClipWithSampleRenderer(clip);
        }

        IEnumerator PlayFrameSequence(Sprite[] frames)
        {
            var interval = framesPerSecond > 0f ? 1f / framesPerSecond : 1f / 12f;

            foreach (var frame in frames)
            {
                if (frame != null)
                    paperImage.sprite = frame;

                yield return new WaitForSecondsRealtime(interval);
            }
        }

        IEnumerator PlayClipWithSampleRenderer(AnimationClip clip)
        {
            var sampleHost = new GameObject("PaperBurnSampleRenderer")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var sampleRenderer = sampleHost.AddComponent<SpriteRenderer>();

            var elapsed = 0f;
            var duration = clip.length;

            while (elapsed < duration)
            {
                clip.SampleAnimation(sampleHost, elapsed);
                paperImage.sprite = sampleRenderer.sprite;
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            clip.SampleAnimation(sampleHost, duration);
            paperImage.sprite = sampleRenderer.sprite;
            Destroy(sampleHost);
        }

        void ResolveReferences()
        {
            if (root == null)
                root = gameObject;

            if (backdropCanvas == null)
            {
                var canvasTransform = transform.Find("BackdropCanvas");
                if (canvasTransform != null)
                    backdropCanvas = canvasTransform.GetComponent<Canvas>();
            }

            if (paperImage == null)
            {
                var paperTransform = transform.Find("BackdropCanvas/PaperBurnVisual");
                if (paperTransform != null)
                    paperImage = paperTransform.GetComponent<Image>();
            }

            if (paperImage != null)
            {
                var rect = paperImage.rectTransform;
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = new Vector2(paperDisplaySize, paperDisplaySize);
                paperImage.preserveAspect = true;
                paperImage.raycastTarget = false;
            }
        }

        void Show()
        {
            if (backdropCanvas != null)
            {
                backdropCanvas.gameObject.SetActive(true);
                backdropCanvas.enabled = true;
            }

            if (paperImage != null)
                paperImage.gameObject.SetActive(true);
        }

        void HideImmediate()
        {
            if (backdropCanvas != null)
                backdropCanvas.enabled = false;

            if (paperImage != null)
                paperImage.gameObject.SetActive(false);
        }
    }
}
