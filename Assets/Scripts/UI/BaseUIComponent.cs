using UnityEngine;
using UnityEngine.UI;
using SpirographPro.Configuration;

namespace SpirographPro.UI
{
    /// <summary>
    /// Base class for UI components.
    /// Provides common UI creation and styling methods to reduce code duplication.
    /// </summary>
    public abstract class BaseUIComponent : MonoBehaviour
    {
        /// <summary>
        /// Reference to the configuration.
        /// </summary>
        protected SpirographConfiguration Config => SpirographConfiguration.Instance;
        
        /// <summary>
        /// Create a modern button with glassmorphic styling.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="name">Button name.</param>
        /// <param name="position">Anchored position.</param>
        /// <param name="size">Button size.</param>
        /// <param name="text">Button text.</param>
        /// <param name="color">Button background color.</param>
        /// <returns>Created button component.</returns>
        protected Button CreateModernButton(Transform parent, string name, Vector2 position, Vector2 size, 
            string text, Color? color = null)
        {
            GameObject buttonObj = new GameObject(name);
            buttonObj.transform.SetParent(parent, false);
            RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0, 1);
            buttonRect.anchorMax = new Vector2(0, 1);
            buttonRect.pivot = new Vector2(0, 1);
            buttonRect.anchoredPosition = position;
            buttonRect.sizeDelta = size;
            
            // Background
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = color ?? new Color(0.08f, 0.12f, 0.22f, 0.7f);
            
            // Outline
            Outline buttonOutline = buttonObj.AddComponent<Outline>();
            buttonOutline.effectColor = new Color(0.3f, 0.5f, 0.8f, 0.4f);
            buttonOutline.effectDistance = new Vector2(1, -1);
            
            // Shadow
            Shadow buttonGlow = buttonObj.AddComponent<Shadow>();
            buttonGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
            buttonGlow.effectDistance = new Vector2(0, 0);
            
            // Button component
            Button button = buttonObj.AddComponent<Button>();
            button.targetGraphic = buttonImage;
            
            // Colors
            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.85f, 0.95f, 1f, 1f);
            colors.pressedColor = new Color(0.6f, 0.8f, 1f, 1f);
            colors.selectedColor = new Color(0.85f, 0.95f, 1f, 1f);
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            colors.colorMultiplier = 1.2f;
            colors.fadeDuration = 0.15f;
            button.colors = colors;
            
            // Text
            CreateButtonText(buttonObj.transform, text);
            
            return button;
        }
        
        /// <summary>
        /// Create button text component.
        /// </summary>
        protected Text CreateButtonText(Transform parent, string text)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>(SpirographConstants.LEGACY_FONT);
            textComponent.fontSize = 12;
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.color = new Color(0.85f, 0.95f, 1f, 0.95f);
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            // Shadow
            Shadow textShadow = textObj.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0, 0, 0, 0.5f);
            textShadow.effectDistance = new Vector2(1, -1);
            
            return textComponent;
        }
        
        /// <summary>
        /// Create a modern slider with labels and styling.
        /// </summary>
        /// <param name="parent">Parent transform.</param>
        /// <param name="name">Slider name.</param>
        /// <param name="position">Anchored position.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        /// <param name="defaultValue">Default value.</param>
        /// <param name="labelText">Label text.</param>
        /// <param name="valueText">Initial value text.</param>
        /// <returns>Tuple of (Slider, Label Text, Value Text).</returns>
        protected (Slider slider, Text label, Text value) CreateModernSlider(Transform parent, string name, 
            Vector2 position, float minValue, float maxValue, float defaultValue, 
            string labelText, string valueText)
        {
            // Create container
            GameObject sliderObj = new GameObject(name);
            sliderObj.transform.SetParent(parent, false);
            RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0, 1);
            sliderRect.anchorMax = new Vector2(0, 1);
            sliderRect.pivot = new Vector2(0, 1);
            sliderRect.anchoredPosition = position;
            sliderRect.sizeDelta = new Vector2(290, SpirographConstants.SLIDER_HEIGHT);
            
            // Label
            Text label = CreateSliderLabel(sliderObj.transform, labelText);
            
            // Value text
            Text value = CreateSliderValueText(sliderObj.transform, valueText);
            
            // Slider
            Slider slider = CreateSliderComponent(sliderObj, minValue, maxValue, defaultValue);
            
            return (slider, label, value);
        }
        
        /// <summary>
        /// Create slider label.
        /// </summary>
        protected Text CreateSliderLabel(Transform parent, string text)
        {
            GameObject labelObj = new GameObject("Label");
            labelObj.transform.SetParent(parent, false);
            RectTransform labelRect = labelObj.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0, 1);
            labelRect.anchorMax = new Vector2(0, 1);
            labelRect.pivot = new Vector2(0, 1);
            labelRect.anchoredPosition = new Vector2(0, 0);
            labelRect.sizeDelta = new Vector2(200, 25);
            
            Text label = labelObj.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>(SpirographConstants.LEGACY_FONT);
            label.fontSize = 11;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.6f, 0.75f, 0.9f, 0.9f);
            label.alignment = TextAnchor.MiddleLeft;
            
            return label;
        }
        
        /// <summary>
        /// Create slider value label.
        /// </summary>
        protected Text CreateSliderValueText(Transform parent, string text)
        {
            GameObject valueObj = new GameObject("ValueLabel");
            valueObj.transform.SetParent(parent, false);
            RectTransform valueRect = valueObj.AddComponent<RectTransform>();
            valueRect.anchorMin = new Vector2(1, 1);
            valueRect.anchorMax = new Vector2(1, 1);
            valueRect.pivot = new Vector2(1, 1);
            valueRect.anchoredPosition = new Vector2(0, 0);
            valueRect.sizeDelta = new Vector2(80, 25);
            
            Text value = valueObj.AddComponent<Text>();
            value.text = text;
            value.font = Resources.GetBuiltinResource<Font>(SpirographConstants.LEGACY_FONT);
            value.fontSize = 10;
            value.fontStyle = FontStyle.Bold;
            value.color = new Color(0.4f, 0.75f, 0.9f, 0.95f);
            value.alignment = TextAnchor.MiddleRight;
            
            return value;
        }
        
        /// <summary>
        /// Create the actual slider component.
        /// </summary>
        protected Slider CreateSliderComponent(GameObject sliderObj, float minValue, float maxValue, float defaultValue)
        {
            // Background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = new Vector2(0, 0);
            bgRect.anchorMax = new Vector2(1, 0);
            bgRect.pivot = new Vector2(0.5f, 0);
            bgRect.anchoredPosition = new Vector2(0, 10);
            bgRect.sizeDelta = new Vector2(0, 8);
            
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.08f, 0.08f, 0.15f, 0.8f);
            
            // Fill Area
            GameObject fillAreaObj = new GameObject("FillArea");
            fillAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0, 0);
            fillAreaRect.anchorMax = new Vector2(1, 0);
            fillAreaRect.pivot = new Vector2(0, 0);
            fillAreaRect.anchoredPosition = new Vector2(0, 10);
            fillAreaRect.sizeDelta = new Vector2(-4, 8);
            
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = new Vector2(0, 1);
            fillRect.pivot = new Vector2(0, 0.5f);
            fillRect.sizeDelta = Vector2.zero;
            
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            
            // Handle
            GameObject handleAreaObj = new GameObject("HandleArea");
            handleAreaObj.transform.SetParent(sliderObj.transform, false);
            RectTransform handleAreaRect = handleAreaObj.AddComponent<RectTransform>();
            handleAreaRect.anchorMin = new Vector2(0, 0);
            handleAreaRect.anchorMax = new Vector2(1, 1);
            handleAreaRect.sizeDelta = new Vector2(-4, -35);
            handleAreaRect.anchoredPosition = new Vector2(0, -17.5f);
            
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleAreaObj.transform, false);
            RectTransform handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16, 16);
            
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(0.4f, 0.7f, 1f, 1f);
            
            // Slider component
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.minValue = minValue;
            slider.maxValue = maxValue;
            slider.value = defaultValue;
            
            return slider;
        }
        
        /// <summary>
        /// Apply glassmorphic panel styling.
        /// </summary>
        protected void ApplyPanelStyling(GameObject panel, Color backgroundColor)
        {
            Image panelImage = panel.GetComponent<Image>();
            if (panelImage == null)
            {
                panelImage = panel.AddComponent<Image>();
            }
            panelImage.color = backgroundColor;
            
            // Outline
            Outline panelOutline = panel.GetComponent<Outline>();
            if (panelOutline == null)
            {
                panelOutline = panel.AddComponent<Outline>();
            }
            panelOutline.effectColor = new Color(0.3f, 0.5f, 0.9f, 0.25f);
            panelOutline.effectDistance = new Vector2(2, -2);
            
            // Shadow
            Shadow panelGlow = panel.GetComponent<Shadow>();
            if (panelGlow == null)
            {
                panelGlow = panel.AddComponent<Shadow>();
            }
            panelGlow.effectColor = new Color(0.2f, 0.4f, 0.8f, 0.3f);
            panelGlow.effectDistance = new Vector2(0, 0);
        }
    }
}
