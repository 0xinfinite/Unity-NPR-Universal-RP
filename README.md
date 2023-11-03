# Custom Unity Universal Renderpipeline

 This project forked from Unity URP 14.0.8 and customized for Non-Photorealistic Rendering.
 This package is tailored for Unity 2022.3.2f1 and is not guaranteed to work with other Unity versions.
 
 MIT license adapted on this repository.


# Features



## Stylized Shading on Deferred Rendering

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/Deferred-NPR.gif?raw=true">

## Warpmap Atlas based Shading

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/Warpmap.gif?raw=true">

## Customized Punctual Light Range

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/non-physically-falloff.gif?raw=true">

## Cached Punctual Light Shadow

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/Cached-shadow.gif?raw=true">

In the image above, the shadow casters for static objects are turned off.

## Per-Material Shadow Depth Bias

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/Per-Material-ShadowBias.png?raw=true">

## Customized Skinned Mesh Renderer

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/facial%20normal%20compare.gif?raw=true">




# WIP or Planned in future


## Main Character Focused Light Shadow

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/how%20to%20render%20main%20character%20shadow.png?raw=true">
<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/main%20character%20shadow.gif?raw=true">
An optional feature that increases the shadow resolution of the main character or saves the shadow map by focusing the shadow frustum on the main character.

## Screen-space Shadow caster

Screen-space-aware shadow caster feature for easier shadow placement.

## Face Focused Hair Shadow

The feature to confine the additional shadow frustum to the face to apply natural cartoon hair shadows.

## Global Shade Color Control

Optional feature to set the light source and shadow color collectively in universal render data.


## Bake Cached Shadow Feature on URP

A Feature to bake Cached Shadows within URP (now I have to bake shadows from other projects in the Legacy pipeline).

## Manual Light Probe

Light probes that can be manually placed and modified



