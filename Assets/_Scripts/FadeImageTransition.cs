using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class FadeImageTransition : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 1f;       // Duration of the fade effect
    public Color fadeInColor = Color.black;  // Color to fade into
    public Color fadeOutColor = Color.clear; // Color to fade out to

    private static FadeImageTransition _instance;
    private Image _image;

    public static FadeImageTransition Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.LogError("FadeImageTransition is not found in the scene!");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject); // Ensure only one instance exists
        }

        _image = GetComponent<Image>();
        if (_image == null)
        {
            Debug.LogError("No Image component found on this GameObject!");
        }

        // Ensure the image is initially invisible
        _image.color = fadeOutColor;
    }

    /// <summary>
    /// Fades the image to full visibility.
    /// </summary>
    public void FadeIn()
    {
        if (_image != null)
        {
            _image.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad);
        }
    }
    
    public void FadeInOut(Action callback)
    {
        if (_image != null)
        {
            _image.DOFade(1f, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
            {

                FadeOut(callback);
                callback.Invoke();
            });
        }
    }

    /// <summary>
    /// Fades the image to full transparency.
    /// </summary>
    public void FadeOut(Action callback)
    {
        if (_image != null)
        {
            _image.DOFade(0f, fadeDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
            {

                
            }); ;
        }
    }
}
