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

left is original skinned mesh renderer and right is customized one.

## freely-placeable Custom Shadow 

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/how%20to%20render%20main%20character%20shadow.png?raw=true">

usage

### Main Character Focused Light Shadow

<img src="https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/main%20character%20shadow.gif?raw=true">
Increases the shadow resolution of the main character or increase performance by focusing the shadow frustum on the main character.

### 2D Hair Shadow

<img src = "https://github.com/0xinfinite/0xinfinite.github.io/blob/master/img/2d-hair-shadow.png?raw=true">
Confine the additional shadow frustum to the face to apply natural cartoon hair shadows. (It's difficult to archive this with built-in shadow)


## Works on Android

<iframe width="560" height="315" src="https://www.youtube.com/embed/kwWVc1ryGLs?si=qsOO6tYRzaSjJ8Yr" title="YouTube video player" frameborder="0" allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share" allowfullscreen></iframe>

Device : Samsung Galaxy S8+

Adapted Settings : Deferred Rendering, Warp Texture Shading, Custom Shadow(Main Character Focused Light Shadow, 2D Hair Shadow)

(Disclaimer : This repository is not guaranteed to work properly on your build. The video above demonstrates the optimization of the implementation in a different repository.)


# WIP or Planned in future




## Screen-space Shadow caster

Screen-space-aware shadow caster feature for easier shadow placement.


## Global Shade Color Control

Optional feature to set the light source and shadow color collectively in universal render data.


## Bake Cached Shadow Feature on URP

A Feature to bake Cached Shadows within URP (now I have to bake shadows from other projects in the Legacy pipeline).

## Manual Light Probe

Light probes that can be manually placed and modified



The 3D model was created by myself. If you are interested in the model, please visit here. [https://twitter.com/Mootonashi](https://twitter.com/Mootonashi)
