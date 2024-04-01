using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using GamePlay;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class LoadingPanelController : SingletonComponent<LoadingPanelController>
{
    private TweenerCore<float, float, FloatOptions> _zoomIn = null;
    private TweenerCore<float, float, FloatOptions> _zoomOut = null;
    [HideInInspector]public float zoomSize = 3.5f;
    [SerializeField] private Animator animator;
    private static readonly int CloudIn = Animator.StringToHash("In");
    private static readonly int CloudOut = Animator.StringToHash("Out");
    
    private void OnEnable()
    {
        animator.SetTrigger(CloudIn);
    }

    public void Set_Disable()
    {
        animator.SetTrigger(CloudOut);
    }

    public void DisableGameObject()
    {
        if (GamePlayController.Instance != null)
            GamePlayController.Instance.Invoke(nameof(GamePlayController.Instance.PlayHighLightAnimation), 0.5f);
        gameObject.SetActive(false);
    }

    public void CameraZoomOut()
    {
        KillAnimations(_zoomIn);
        if (LevelGenerator.Instance == null) return;

        var cameraFit = FindObjectOfType<CameraFit>();
        var defaultValue = LevelGenerator.Instance.GetDefaultCameraPosForLevel();
        var targetValue = defaultValue + zoomSize;

        _zoomOut = DOTween.To(() => defaultValue, x => defaultValue = x, targetValue, 1.2f)
            .OnUpdate(() => cameraFit.horizontalFOV = defaultValue).OnKill(() => cameraFit.horizontalFOV = targetValue);
    }


    public void CameraZoomIn()
    {
        KillAnimations(_zoomOut);
        if (LevelGenerator.Instance == null) return;

        var cameraFit = FindObjectOfType<CameraFit>();
        var defaultValue = LevelGenerator.Instance.GetCameraSize();
        var targetValue = LevelGenerator.Instance.GetDefaultCameraPosForLevel();

        _zoomIn = DOTween.To(() => defaultValue, x => defaultValue = x, targetValue, 1.5f)
            .OnUpdate(() => cameraFit.horizontalFOV = defaultValue).OnKill(() => cameraFit.horizontalFOV = targetValue)
            .OnComplete(LevelGenerator.Instance.SetCameraAtEnd).SetEase(Ease.InOutSine);
    }

    private void KillAnimations(Tween animation)
    {
        if (animation != null && animation.IsPlaying())
            animation.Kill();
    }
}