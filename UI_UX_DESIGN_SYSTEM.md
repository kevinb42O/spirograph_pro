# 🎨 Spirograph Pro - UI/UX Design System

## Overview
This document defines the complete design system for Spirograph Pro, ensuring Adobe-level visual consistency and professional polish across all UI components.

---

## 🎨 Color Palette - Cosmic Glassmorphism

### Primary Colors
```csharp
DeepSpaceGlass     = rgba(0.02, 0.02, 0.08, 0.75)  // Primary panel backgrounds
DarkSpaceGlass     = rgba(0.01, 0.02, 0.12, 0.92)  // Darker variant for nested panels
SectionBackground  = rgba(0.03, 0.03, 0.10, 0.70)  // Section backgrounds
ControlBackground  = rgba(0.08, 0.12, 0.22, 0.70)  // Control elements (sliders, inputs)
ButtonBackground   = rgba(0.08, 0.12, 0.22, 0.70)  // Standard button backgrounds
```

### Accent Colors
```csharp
CyanGlow          = rgba(0.4, 0.7, 1.0, 0.6)   // Primary glow/outline color
BlueGlow          = rgba(0.3, 0.6, 1.0, 0.5)   // Secondary glow effect
BrightCyan        = rgba(0.7, 0.85, 1.0, 0.9)  // Text highlights
SoftCyanWhite     = rgba(0.8, 0.9, 1.0, 0.95)  // Primary text color
BrightWhite       = rgba(0.85, 0.95, 1.0, 1.0) // Titles and headers
MutedText         = rgba(0.6, 0.75, 0.9, 0.9)  // Secondary text/labels
```

### Semantic Colors
```csharp
SuccessGreen   = rgba(0.3, 0.8, 0.4, 1.0)  // Success states, start buttons
WarningYellow  = rgba(0.9, 0.7, 0.3, 1.0)  // Warning states, pause buttons
ErrorRed       = rgba(0.8, 0.3, 0.3, 1.0)  // Error states, remove buttons
PurpleAccent   = rgba(0.5, 0.3, 0.6, 0.9)  // Reset buttons, tertiary actions
```

### Accessibility
- All text colors meet **WCAG AA** contrast ratio (4.5:1 minimum)
- Use `UIConstants.GetContrastRatio()` to validate custom color combinations
- Bright text on dark backgrounds ensures readability

---

## 📏 Spacing & Layout System

### Standard Spacing Scale
```csharp
SpacingXS     = 2px   // Tight spacing (borders, minimal gaps)
SpacingSmall  = 5px   // Small gaps between related items
SpacingMedium = 10px  // Default spacing for most elements
SpacingLarge  = 15px  // Section padding, button groups
SpacingXL     = 20px  // Major section spacing, panel padding

PanelPadding      = 15px  // Standard edge padding for panels
SectionSpacing    = 10px  // Spacing between collapsible sections
```

### Layout Guidelines
- **Vertical Spacing**: Use consistent spacing (5px, 10px, 15px, 20px) between elements
- **Horizontal Padding**: Apply 15px padding to panel edges for breathing room
- **Section Gaps**: Maintain 10px between major UI sections
- **Grid System**: Base all measurements on 5px increments for consistency

---

## 📖 Typography System

### Font Sizes
```csharp
FontSizeTitle  = 18px  // Panel titles, major headers
FontSizeHeader = 14px  // Section headers
FontSizeBody   = 12px  // Standard body text, button labels
FontSizeSmall  = 10px  // Small labels, secondary info
FontSizeTiny   = 9px   // Minimum readable size, stats, timestamps
```

### Font Usage Guidelines
- **Titles**: Bold, 18px, BrightWhite - Panel headers and major sections
- **Headers**: Bold, 14px, BrightCyan - Section labels
- **Body**: Bold, 12px, SoftCyanWhite - Buttons, primary labels
- **Small**: Regular/Bold, 10px, MutedText - Status text, values
- **Tiny**: Regular, 9px, MutedText - Statistics, metadata (minimum for readability)

### Best Practices
- Always use **bold** for interactive elements (buttons, labels)
- Use **regular weight** for non-interactive text
- Maintain **4px minimum** line spacing for multi-line text
- Never go below **9px** font size

---

## 🔘 Component Sizes

### Buttons
```csharp
ButtonHeight        = 40px  // Standard button height
ButtonHeightCompact = 32px  // Compact buttons in tight spaces
ButtonHeightSmall   = 28px  // Minimum recommended size
MinTouchTarget      = 44px  // Minimum for primary touch targets (mobile-friendly)
```

### Sliders
```csharp
SliderHeight      = 50px  // Total slider component height
SliderTrackHeight = 8px   // Slider track/background height
SliderHandleSize  = 20px  // Slider handle dimensions (width x height)
```

### Component Guidelines
- **Primary Actions**: Use 40px or 44px height for main action buttons
- **Secondary Actions**: 32px height for less important buttons
- **Tertiary Actions**: 28px minimum (never smaller than this)
- **Touch Targets**: Mobile-friendly buttons should be at least 44x44px

---

## ⚡ Animation & Timing

### Transition Durations
```csharp
TransitionVeryFast = 0.1s (100ms)  // Instant feedback (hover states)
TransitionFast     = 0.2s (200ms)  // Quick transitions (button presses)
TransitionNormal   = 0.3s (300ms)  // Standard transitions (panels, fades)
TransitionSlow     = 0.5s (500ms)  // Dramatic transitions (major state changes)
```

### Easing Functions
```csharp
// Smooth cubic easing for organic feel
UIConstants.SmoothEase(t) 
  // Use for: All UI transitions, fades, scales

// Elastic ease out for playful interactions
UIConstants.ElasticEaseOut(t)
  // Use for: Button clicks, selection highlights
```

### Animation Guidelines
- **Hover**: 100ms fade with SmoothEase
- **Click**: 200ms scale (0.95x press) with SmoothEase
- **Panel Show/Hide**: 300ms fade + scale with SmoothEase
- **Context Switch**: 200ms fade out, 300ms fade in with SmoothEase
- **Progress Bars**: 500ms smooth fill with SmoothEase

### Scale Multipliers
```csharp
ButtonHoverScale = 1.08   // 8% scale up on hover
ButtonPressScale = 0.95   // 5% scale down on press
```

---

## 🎭 Visual Effects

### Outlines & Borders
```csharp
OutlineDistance      = (2, -2)   // Standard outline offset
OutlineDistanceLarge = (3, -3)   // Emphasized outline for important elements
ShadowDistance       = (1, -1)   // Text shadow offset
GlowDistance         = (0, 0)    // Radial glow (no offset)
```

### Effect Usage
- **Outlines**: Apply to all interactive elements (buttons, sliders, cards)
- **Shadows**: Use for text depth and card elevation
- **Glow**: Apply to handles, fills, and accent elements
- **Colors**: Always use CyanGlow or BlueGlow for consistency

### Glassmorphism Style
- **Background Blur**: Not directly supported in Unity UI, simulate with low alpha
- **Transparency**: Use 0.7-0.9 alpha for glass effect
- **Edges**: Always add outline/glow for definition
- **Depth**: Layer shadows and glows for 3D effect

---

## 📱 Responsive Design

### Canvas Configuration
```csharp
ReferenceResolution = 1920x1080
MatchWidthOrHeight  = 0.5 (balanced)
```

### Resolution Support
- **1920x1080 (1080p)**: Reference resolution, perfect scaling
- **2560x1440 (1440p)**: Scales up 1.33x, maintains proportions
- **3840x2160 (4K)**: Scales up 2x, maintains proportions
- **1280x720 (720p)**: Scales down 0.66x, maintains proportions

### Responsive Guidelines
- **Use Anchors**: Anchor panels to screen edges (corners, centers)
- **Relative Sizing**: Use sizeDelta with negative values for flexible sizing
- **ScrollRects**: Apply to long content areas (agent lists, control panels)
- **Test Points**: Verify layout at 720p, 1080p, 1440p, 4K

---

## ♿ Accessibility

### Touch Targets
- **Minimum Size**: 44x44px for primary interactive elements
- **Compact Allowance**: 28px minimum for secondary actions
- **Spacing**: Maintain 8px minimum between adjacent touch targets

### Color Contrast
```csharp
// WCAG AA Standard: 4.5:1 for normal text, 3:1 for large text
UIConstants.GetContrastRatio(foreground, background)
```

### Current Ratios
- **BrightCyan on DeepSpaceGlass**: ~12:1 ✓ Excellent
- **SoftCyanWhite on ButtonBackground**: ~9:1 ✓ Excellent
- **MutedText on SectionBackground**: ~5:1 ✓ Good

### Keyboard Navigation
- Tab order follows visual flow (top to bottom, left to right)
- Focus indicators use CyanGlow outline
- Enter/Return submits active forms
- ESC closes modals and overlays

---

## 🎯 Component Patterns

### Standard Button
```csharp
- Background: ButtonBackground
- Outline: CyanGlow (2, -2)
- Shadow: BlueGlow (0, 0)
- Text: SoftCyanWhite, 12px Bold
- Hover: Scale 1.08x, 100ms
- Press: Scale 0.95x, 200ms
```

### Modern Slider
```csharp
- Track: ControlBackground, 8px height
- Fill: BlueGlow with CyanGlow shadow
- Handle: BrightWhite, 20x20px with CyanGlow shadow
- Label: BrightCyan, 12px Bold
- Value: MutedText, 10px Regular
```

### Agent Card
```csharp
- Background: State-dependent color (Idle/Active/Paused/Complete)
- Outline: CyanGlow (2, -2)
- Shadow: BlueGlow (3, -3)
- Title: Agent color, 12px Bold
- Status: MutedText, 10px Regular
- Progress Bar: Agent color fill with smooth animation
```

### Context Banner
```csharp
- Background: DarkSpaceGlass
- Outline: CyanGlow (2, -2)
- Shadow: Black 50% (0, -3)
- Text: BrightWhite, 18px Bold
- Accent: Agent/Master color bar (6px wide)
- Transitions: 200ms fade out, 300ms fade in
```

---

## 🔧 Implementation Guide

### Using UIConstants in Code
```csharp
// Colors
image.color = UIConstants.DeepSpaceGlass;
text.color = UIConstants.BrightCyan;

// Spacing
layout.spacing = UIConstants.SpacingMedium;
padding.left = UIConstants.PanelPadding;

// Typography
text.fontSize = UIConstants.FontSizeBody;

// Sizing
buttonRect.sizeDelta = new Vector2(100, UIConstants.ButtonHeight);

// Animation
float duration = UIConstants.TransitionNormal;
float easedT = UIConstants.SmoothEase(t);

// Effects
outline.effectDistance = UIConstants.OutlineDistance;
shadow.effectColor = UIConstants.BlueGlow;
```

### Checklist for New Components
- [ ] Use UIConstants colors (no hardcoded RGBA values)
- [ ] Apply standard spacing (5px, 10px, 15px, 20px)
- [ ] Use typography scale (9, 10, 12, 14, 18px)
- [ ] Meet minimum touch targets (28px+)
- [ ] Add smooth easing to all animations
- [ ] Apply consistent outlines and glows
- [ ] Test at multiple resolutions
- [ ] Verify color contrast ratios

---

## 📊 Performance Targets

### UI Update Frequency
```csharp
UIUpdateFrequency = 10Hz (10 updates/second)
UIUpdateInterval  = 0.1s
```

### Performance Guidelines
- **60 FPS Target**: UI animations should maintain 60fps
- **Update Throttling**: Update stats/displays at 10Hz, not every frame
- **Canvas Rebuilds**: Minimize by avoiding frequent layout changes
- **Object Pooling**: Consider for frequently created/destroyed elements

### Optimization Techniques
- Cache component references (don't use GetComponent in Update)
- Use SetValueWithoutNotify for sliders when updating programmatically
- Batch UI updates (update multiple elements at once)
- Disable CanvasGroup.interactable when panels are hidden

---

## 🎓 Best Practices

### Do's
✓ Always use UIConstants for colors, spacing, sizes
✓ Apply SmoothEase to all animations for organic feel
✓ Maintain consistent outline/shadow styling
✓ Test at multiple resolutions (720p, 1080p, 1440p, 4K)
✓ Use semantic colors (SuccessGreen, ErrorRed)
✓ Keep touch targets 44px+ for primary actions
✓ Update UI at 10Hz, not every frame
✓ Cache component references for performance

### Don'ts
✗ Never hardcode color values (use UIConstants)
✗ Don't use random spacing (stick to 5px increments)
✗ Avoid fonts smaller than 9px
✗ Don't use linear Lerp (use SmoothEase instead)
✗ Never update UI every frame if not necessary
✗ Don't make buttons smaller than 28px
✗ Avoid inconsistent animation durations

---

## 🔄 Version History

### v1.0 - Initial Design System (Current)
- Established cosmic glassmorphism theme
- Defined complete color palette (15 colors)
- Standardized spacing system (5 increments)
- Created typography scale (5 sizes)
- Implemented smooth easing functions
- Set performance targets (60fps, 10Hz updates)

### Future Enhancements
- [ ] Additional color themes (Dark, Light, High Contrast)
- [ ] Custom font integration (currently using LegacyRuntime.ttf)
- [ ] Advanced animation curves (bounce, spring)
- [ ] Localization support (multi-language)
- [ ] Theme switching system
- [ ] Advanced particle effects system

---

## 📚 References

### Inspiration
- **Glassmorphism**: Modern iOS/macOS UI style
- **Cosmic Theme**: Space-inspired color palette
- **Material Design**: Component structure and patterns
- **Fluent Design**: Animation principles

### Standards
- **WCAG 2.1**: Accessibility guidelines
- **Material Design**: Component sizing
- **Apple HIG**: Touch target recommendations
- **Unity UI Best Practices**: Performance optimization

---

## 🤝 Contributing

When adding new UI components:
1. Review this design system first
2. Use UIConstants for all values
3. Follow component patterns
4. Test at multiple resolutions
5. Verify accessibility standards
6. Update this document if introducing new patterns

---

**Design System Version**: 1.0  
**Last Updated**: 2025-11-13  
**Maintained By**: UI/UX Team  
**Status**: ✅ Complete & Production Ready
