using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Runtime-only animation preview panel for the Showcase scene.
/// Drop this file anywhere below an Assets folder; it creates itself in Play Mode.
/// It never needs to be attached to a GameObject and does not change scene assets.
/// </summary>
public sealed class ShowcaseSharedAnimationDebug : MonoBehaviour
{
    private struct AnimationChoice
    {
        public readonly string Label;
        public readonly string[] StateNames;

        public AnimationChoice(string label, params string[] stateNames)
        {
            Label = label;
            StateNames = stateNames;
        }
    }

    private static readonly AnimationChoice[] Choices =
    {
        new AnimationChoice("Idle", "idle", "Idle"),
        new AnimationChoice("Walk", "walk", "Walk"),
        new AnimationChoice("Run", "run", "Run"),
        new AnimationChoice("Jump", "jump", "Jump"),
        new AnimationChoice("Hurt", "hurt", "Hurt"),
        new AnimationChoice("Attack 1", "attack1", "Attack1"),
        new AnimationChoice("Attack 2", "attack2", "Attack2"),
        new AnimationChoice("Attack 3", "attack3", "Attack3"),
        new AnimationChoice("Die", "die", "Die"),
        new AnimationChoice("Disappear", "disappear", "Disappear")
    };

    private readonly List<Animator> animators = new List<Animator>();
    private Scene scannedScene;
    private Vector2 scrollPosition;
    private string lastResult = "Ready";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Create()
    {
        if (FindFirstObjectByType<ShowcaseSharedAnimationDebug>() != null)
            return;

        var host = new GameObject("[Debug] Showcase Shared Animation Panel");
        DontDestroyOnLoad(host);
        host.AddComponent<ShowcaseSharedAnimationDebug>();
    }

    private void Update()
    {
        if (scannedScene != SceneManager.GetActiveScene())
            RefreshAnimators();
    }

    private void RefreshAnimators()
    {
        scannedScene = SceneManager.GetActiveScene();
        animators.Clear();

        // Resources.FindObjectsOfTypeAll includes disabled GameObjects. Scene filtering
        // excludes Animator components belonging to prefabs or other loaded scenes.
        foreach (var animator in Resources.FindObjectsOfTypeAll<Animator>())
        {
            if (animator == null || animator.runtimeAnimatorController == null)
                continue;

            if (animator.gameObject.scene != scannedScene)
                continue;

            animators.Add(animator);
        }

        lastResult = string.Format("Found {0} character Animator(s)", animators.Count);
    }

    private bool IsAvailable(AnimationChoice choice)
    {
        foreach (var animator in animators)
        {
            if (FindSupportedState(animator, choice) != null)
                return true;
        }

        return false;
    }

    private void Play(AnimationChoice choice)
    {
        int played = 0;

        foreach (var animator in animators)
        {
            string stateName = FindSupportedState(animator, choice);
            if (stateName == null)
                continue;

            ResetPersistentPreviewParameters(animator);
            ApplyPreviewParameters(animator, choice.Label);

            // Locomotion states have transitions back to idle when their bool is false.
            // Set the bool first, then enter the state so Walk/Run stay looping.
            animator.Play(stateName, 0, 0f);
            animator.Update(0f);
            played++;
        }

        lastResult = string.Format("{0}: {1}/{2} compatible character(s)", choice.Label, played, animators.Count);
    }

    private static string FindSupportedState(Animator animator, AnimationChoice choice)
    {
        foreach (string stateName in choice.StateNames)
        {
            if (animator.HasState(0, Animator.StringToHash(stateName)))
                return stateName;
        }

        return null;
    }

    private static void ResetPersistentPreviewParameters(Animator animator)
    {
        SetBoolIfPresent(animator, "isWalk", false);
        SetBoolIfPresent(animator, "isRun", false);
        SetBoolIfPresent(animator, "isJump", false);
        SetBoolIfPresent(animator, "isGrounded", true);
        SetIntegerIfPresent(animator, "Attack", 0);
        SetIntegerIfPresent(animator, "attack", 0);
        SetIntegerIfPresent(animator, "attackIndex", 0);
    }

    private static void ApplyPreviewParameters(Animator animator, string animationLabel)
    {
        switch (animationLabel)
        {
            case "Walk":
                SetBoolIfPresent(animator, "isWalk", true);
                break;
            case "Run":
                SetBoolIfPresent(animator, "isRun", true);
                break;
            case "Jump":
                SetBoolIfPresent(animator, "isGrounded", false);
                SetBoolIfPresent(animator, "isJump", true);
                break;
            case "Hurt":
                SetTriggerIfPresent(animator, "onHurt");
                break;
            case "Die":
                SetTriggerIfPresent(animator, "onDie");
                break;
            case "Disappear":
                SetTriggerIfPresent(animator, "onDisappear");
                break;
        }
    }

    private static void SetBoolIfPresent(Animator animator, string parameterName, bool value)
    {
        if (HasParameter(animator, parameterName, AnimatorControllerParameterType.Bool))
            animator.SetBool(parameterName, value);
    }

    private static void SetIntegerIfPresent(Animator animator, string parameterName, int value)
    {
        if (HasParameter(animator, parameterName, AnimatorControllerParameterType.Int))
            animator.SetInteger(parameterName, value);
    }

    private static void SetTriggerIfPresent(Animator animator, string parameterName)
    {
        if (HasParameter(animator, parameterName, AnimatorControllerParameterType.Trigger))
            animator.SetTrigger(parameterName);
    }

    private static bool HasParameter(Animator animator, string parameterName, AnimatorControllerParameterType type)
    {
        foreach (var parameter in animator.parameters)
        {
            if (parameter.type == type && string.Equals(parameter.name, parameterName, StringComparison.Ordinal))
                return true;
        }

        return false;
    }

    private void OnGUI()
    {
        if (!string.Equals(SceneManager.GetActiveScene().name, "showcase", StringComparison.OrdinalIgnoreCase))
            return;

        if (Event.current.type == EventType.Repaint && scannedScene != SceneManager.GetActiveScene())
            RefreshAnimators();

        const float width = 230f;
        const float height = 405f;
        var area = new Rect(Screen.width - width - 16f, Screen.height - height - 16f, width, height);

        GUILayout.BeginArea(area, GUI.skin.box);
        GUILayout.Label("Showcase Animations");
        GUILayout.Label(lastResult);

        if (GUILayout.Button("Refresh Characters"))
            RefreshAnimators();

        GUILayout.Space(6f);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(300f));
        foreach (var choice in Choices)
        {
            GUI.enabled = IsAvailable(choice);
            if (GUILayout.Button(choice.Label))
                Play(choice);
        }
        GUI.enabled = true;
        GUILayout.EndScrollView();

        GUILayout.Label("Only compatible Animators are played.");
        GUILayout.EndArea();
    }
}
