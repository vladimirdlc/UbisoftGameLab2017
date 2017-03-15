Toony Colors Pro, version 2.2
2016/08/06
© 2016 - Jean Moreno
=============================

USAGE
-----
Select one of the following shader in your Material:
- Toony Colors Pro 2/Desktop
- Toony Colors Pro 2/Mobile
Then select the features you want to enable (bump, specular, rim...), and the correct shader will automatically
be selected for you.
Use the (?) buttons to see help for specific features, or read the documentation for more information.


PLEASE LEAVE A REVIEW OR RATE THE PACKAGE IF YOU FIND IT USEFUL!
Enjoy! :)


CONTACT
-------
Questions, suggestions, help needed?
Contact me at:
jean.moreno.public+unity@gmail.com

I'd be happy to see Toony Colors Pro 2 used in your project, so feel free to drop me a line about that! :)


UPDATE NOTES
------------
2.2.31
- added "Bump Scale" option in Shader Generator
- fixed issues when updating shaders with custom output path enabled
- removed Unity5.4 templates and added differences in the main Unity5 template

2.2.3
- added Ramp Generator utility
- added option to explicitly set shader model target in Shader Generator
- added Shader Generator template for Terrain shaders (experimental)
- removed explicit "#pragma target 2.0" in PBS shaders for lower LOD, allowing it to default to shader model target 2.5 in Unity 5.4
- renamed "Self-Illumination" feature to "Emission" in Shader Generator
- custom output path fix for already existing shaders

2.2.2
- added option to save generated shaders in custom directory

2.2.1
- added Unity 5.4 compatible templates for the Shader Generator (fix for reflection probes sampling)
- fixed seamless tiling on some sketch textures

2.2
- added "Standard PBS" version of the shaders, based on Unity Standard shaders (Unity 5.3+ only)
- added "TCP2 Demo PBS" scene to show the PBS shaders
- moved "Outline Only" shaders to their own category
- updated documentation

2.14.1
- fixed attenuation factor with "Light-based Mask" for Rim Lighting (point/spot lights, shadows)

2.14
- added "Light-based Mask" option for Rim Lighting in Shader Generator

2.13
- added option to disable wrapped diffuse lighting
- added "Colors Multipliers" option to Shader Generator
- bug fixes and improvements

2.12.2
- disabled custom lightmapping support in Unity5+, turns out it doesn't work anymore with surface shaders

2.12.1
- removed double lighting multiplication in Unity 5 template for Shader Generator
- Smoothed Normal Utility now copies blend shape data in Unity 5.3+

2.12
- added support for "Curved World" by Davit Naskidashvili in the Shader Generator (requires the "Curved World" package from the Asset Store)
- refactored outline shaders code with TCP2_Outline_Include.cginc
- fixed Blended Outline Only shaders queue (from opaque to transparent)
- fixed Outline Only material inspector glitch with slider properties in Unity 5+

2.11.2
- Mobile shader now compiles to shader model 2 as it was supposed to
- added menu options to unpack all mobile and desktop variant shaders

2.11.1
- improved detection of manually modified generated shaders
- fixed issue with Shader Generator and Texture Ramp
- fixed issue with Shader Generator and Cartoon Specular

2.11
- fixed issue with Shader Generator and TCP2 Lightmap
- added script to convert materials from TCP1 to TCP2 (in the tools menu)

2.10
- fixed issue with Smoothed Normals Utility and built-in meshes
- Smoothed Normals Utility no longer requires mesh to be read/write enabled

2.091
- fixed generated Shader userData not always saved when using Shader Generator

2.09
- added option to render outline behind the model (Shader Generator)
- fixed shader model 2 outlines with Shader Generator (Unity 4.5)

2.08
- fixed Parallax offset for diffuse texture (Shader Generator)
- fixed warnings on package import (Unity 5)
- TCP2 shaders now work correctly with Substance materials (Unity 5)

2.07
- fixed MatCap calculations (was incorrect with rotated meshes) in Mobile shaders and Shader Generator template

2.06
- fixed MatCap issue with scaled skinned meshes (added option to turn fix on/off in inspector)
- fixed Pixel MatCap breaking generated shaders if normal map was disabled

2.05
- added Pixel Matcap option in Shader Generator, allows MatCap to work with normal maps

2.04
- fixed path issues on Mac

2.03
- fixed glitched outlines in DX11

2.02
- fixed issue with vertex function in generated shaders
- removed debug information showing in material inspector

2.01
- updated Mobile shaders to target shader model 3: should fix compilation issues with some combinations, will break compatibility with super old desktop GPU (roughly pre-2004)

2.0
- everything redone from scratch!
- lots of new features and optimizations added to the shaders
- Unified Inspector: select one shader and then let the inspector choose the correct optimized shader for you
- Shader Generator: generate your own stylized shader choosing from a lot of features
- Smooth Normals Utility: generate encoded smoothed normals to fix hard-edge outlines
- new Documentation in HTML format with examples and tips

1.71
- updated "JMO Assets" menu

1.7
- added alpha output to shader files (RenderTextures should now work for real)
- Constant Outline shaders now take the object's uniform scale into account

1.6
- fixed alpha output to 0 in lighting model, would cause problems with Render Textures previously
- fixed Warnings in Unity 4+ versions
- fixed shader Warnings for D3D11 and D3D11_9X compilers
- re-enabled ZWrite by default for outlines, would cause them to not show over skyboxes previously

1.5
- fixed the specular lighting algorithm, would cause glitches with small light ranges

1.4
- changed name to "Toony Colors"
- fixed Bump Maps Substance compatibility (WARNING: you may have to re-set the Normal Maps in your materials)

1.3
- added Rim Outline shaders

1.2
- added JMO Assets menu (Window -> JMO Assets), to check for updates or get support

1.1
- Rim lighting is much faster! (excepted on Rim+Bumped shaders)

1.01
- Included Demo Scene

1.0
- Initial Release