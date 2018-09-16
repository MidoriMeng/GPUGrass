/*! \mainpage

\tableofcontents



\n \section about About

Time of Day is a package to render realistic dynamic sky domes with day and night cycle, clouds, cloud shadows, weather types and physically based atmospheric scattering.<br>

<br><strong>Sky:</strong><br>
- Physically based sky shading<br>
- Rayleigh & Mie scattering<br>
- Highly customizable<br>
- Dynamic weather manager<br>
- Sun and moon god rays (Unity Pro)<br>

<br><strong>Lighting:</strong><br>
- Full PBR & HDR support<br>
- Realtime Unity 5 ambient light<br>
- Realtime Unity 5 reflections<br>

<br><strong>Clouds:</strong><br>
- Dynamic wind speed & direction<br>
- Configurable shape, color & scale<br>
- Correctly projected cloud shadows<br>

<br><strong>Time & Location:</strong><br>
- Dynamic day & night cycle<br>
- Adjustable time progression curve<br>
- Full longitude, latitude & time zone support<br>
- Full Gregorian calendar support<br>
- Rotating night sky<br>
- Realistic sun position<br>
- Realistic moon position and phase<br>

<br><strong>Performance & Requirements:</strong><br>
- Extremely optimized shaders & scripts<br>
- Zero dynamic memory allocations<br>
- Supports shader model 2.0<br>
- Supports all platforms<br>
- Supports linear & gamma color space<br>
- Supports forward & deferred rendering<br>
- Supports HDR & LDR rendering<br>
- Supports virtual reality hardware<br>

<br>[ <a href="http://goo.gl/jAvPoE">Forum Thread</a> | <a href="http://goo.gl/uwZsbo">Web Player</a> | <a href="http://goo.gl/n0RP0i">Documentation</a> ]<br>

<br><strong>You can expect a thoroughly documented, well-written and highly optimized code base.<br>
All equations used in the shaders and scripts include references to the scientific papers they are based on.</strong>



\n \section start Getting Started

1. Add the sky dome to your scene:
 - Drag the prefab "Time of Day/Prefabs/Sky Dome" into your scene
 - Tweak the parameters until you are satisfied with the result

2. Move the sky dome to the camera position in every frame:
 - Select your camera and add the Time of Day camera script (Component -> Time of Day -> Camera Main Script)
 - Tag this camera game object as "MainCamera"

3. Render god rays on the main camera:
 - Select your camera and add the Time of Day god ray script (Component -> Time of Day -> Camera God Rays)
 - Tweak the parameters until you are satisfied with the result

<strong>REMARK:</strong>
The camera script moves the sky dome directly before clipping the scene, guaranteeing that all other position updates have been processed. You should not move the sky dome in "LateUpdate" because this can cause minor differences in the sky dome position between frames when moving the camera.



\n \section time Day & Night Cycle

The sky dome prefab has a script TOD_Time attached to it that manages the dynamic day & night cycle. Enabling and disabling this script enables and disables the automatic cycle.

The following parameters are being set by the script:
- TOD_Sky.Cycle.Hour
- TOD_Sky.Cycle.Day
- TOD_Sky.Cycle.Month
- TOD_Sky.Cycle.Year

The script TOD_Time offers a time curve property that can be modified via the Unity inspector to speed up or slow down certain parts of the day-night cycle. The X axis of the graph denotes the current internal time, which always progresses linearly. The Y axis of the graph denotes the time that is being set in the sky dome and is therefore visible to the player. That means the higher the inclination of the curve the faster this certain part of the day passes by.



\n \section weather Weather Manager

The script TOD_Weather can be used to automatically set various parameters of TOD_Sky according to weather presets.

The following parameters are being set by the script:
- TOD_Sky.Atmosphere.Fogginess
- TOD_Sky.Clouds.Density
- TOD_Sky.Clouds.Sharpness
- TOD_Sky.Clouds.Brightness



\n \section location Time Zone & Location Coordinates

The TOD_Sky.World and TOD_Sky.Cycle parameter sections allow for configuration of the sky dome to represent the exact sun and moon movement of any location on the planet depending on Gregorian date, UTC/GMT time zone and geographic coordinates. This also allows to recreate eclipses just as they would occur in real life. It is important to manually set the correct time zone offset (TOD_Sky.World.UTC) that fits the longitude and latitude parameters in order to use local time instead of UTC.

All of those parameters are completely optional - if the sky dome should be used in a generic fantasy world they can simply be ignored and left at their default values.



\n \section ambient Ambient Light & Reflections

Unity 5 introduced new ways to approximate ambient light and reflections. For a primer on the new features, watch the <a href="https://www.youtube.com/watch?v=eoXb-f_pNag">Unite 2014 talk</a>.

Time of Day offers full support for Unity 5 image-based ambient light and reflections. It can update both the per-scene ambient light and a realtime reflection probe at runtime. Ambient light can be disabled (i.e. not managed by Time of Day), a solid color, a gradient or spherical harmonics. Reflections can be disabled (i.e. not managed by Time of Day) or a cubemap. Both ambient light and reflections contain approximations of the atmosphere in the top half and lerp to the configured ambient light color towards the bottom half. This means the ambient light color set on the Time of Day prefab can be looked at as the ground color of the scene in those cases.

Time of Day also allows you to include some or all layers of your scene in the reflection probe bake process. This is to be used with care since updating a reflection probe with various reflected objects is an expensive operation. For most scenes it should be fine to only render the sky dome to the realtime reflection probe by using "Skybox" clear flags and a "Nothing" culling mask.



\n \section quality Rendering Quality

There are various different quality levels for both the sky dome and the cloud shader. Those quality settings can be configured both dynamically at runtime and directly in the Unity editor window using two inspector enums.

TOD_CloudQuality:
- Bumped offers complex cloud shading with dynamic density and cloud normal mapping
- Density offers simplified cloud shading with dynamic density but without normal mapping
- Fastest offers extremely simplified cloud shading with simplified cloud shape calculations

TOD_MeshQuality:
- High tessellation sky dome (2562 verts) and moon (574 verts)
- Medium tessellation sky dome (642 verts) and moon (294 verts)
- Low tessellation sky dome (162 verts) and moon (148 verts)

For the best visual quality it is recommended to use Time of Day with the following Unity Pro setup:
- Linear color space
- HDR enabled on the main camera
- The following image effects on the main camera (in that order)
  1. "Image Effects -> Bloom and Glow -> Bloom" or "SE Natural Bloom & Dirty Lens" from the Asset Store
  2. "Image Effects -> Color Adjustments -> Tonemapping"
  3. "Image Effects -> Color Adjustments -> Color Correction" or "Amplify Color" from the Asset Store



\n \section performance Performance Remarks

- The size of a web player with just the sky dome is only around 200KB as most equations are evaluated dynamically
- All scripts and shaders are highly optimized and will not have a significant FPS impact on desktop computers
- Older mobile devices should switch to the cloud and dome quality settings that offer suitable performance
- Cloud shadows utilize a Unity projector and require another draw call for all objects they are projected on
- Reducing the texture resolution in the cloud texture import settings can increase performance on mobile
- Realtime reflections that include objects other than the sky dome can be expensive and should be used with care



\n \section rendering Rendering Order

All components of the sky dome are being rendered after the opaque but before the transparent meshes of your scene.That means that only areas of the sky dome that are not being occluded by any other geometry have to be rendered.

The rendering order of the sky dome components is the following:
- Transparent-490 Space Dome  (if not manually disabled)
- Transparent-480 Sun Plane   (if on screen)
- Transparent-470 Moon Mesh   (if on screen)
- Transparent-460 Atmosphere  (if not manually disabled)
- Transparent-455 Clear Alpha (if god rays are enabled)
- Transparent-450 Cloud Layer (if not manually disabled)

This leads to 3-6 draw calls to render the complete sky dome, depending on the scene setup.



\n \section shaders Custom Shaders

The TOD_Sky script sets some global shader parameters that can be used in your custom shaders. For a complete list see the TOD_Base.cginc file. Any of those variables can be used in any shader by simply defining uniform variables with the same name, which will then automatically be set to the most recent values every frame. It is also possible to simply include TOD_Base.cginc to get access to all variables.

In addition to those base variables there is also TOD_Scattering.cginc, which offers functions to easily evaluate the scattering equations in custom shaders. The method float4 ScatteringColor(float3 dir) is the easiest to use since it automatically takes care of all the required calculations. It is recommended to call this function in a vertex shader and interpolate the color to the fragment or surface shader, but it can also be used in image effects to implement things like global fog.



\n \section networking Networking

- To network the date, synchronize the property TOD_Sky.Cycle.Ticks of type long
- To network the cloud movement, synchronize the property TOD_Sky.Components.Animation.CloudUV of type Vector4



\n \section serialization Parameter Import & Export

It is possible to export custom presets via the "Export" button in the TOD_Sky inspector panel and import them on a different prefab or even in a different project via the "Import" button. Exported parameters can also be loaded at runtime by using the appropriate API calls.



\n \section examples Example Scripts

The package comes with various example scripts to demonstrate sky dome integration.

- AudioAtDay / AudioAtNight / AudioAtWeather:
  Fade audio sources in and out according to a time of day or a specific weather type
- ParticleAtDay / ParticleAtNight / ParticleAtWeather:
  Fade particle systems in and out according to a time of day or a specific weather type
- RenderAtDay / RenderAtNight / RenderAtWeather:
  Enable or disable renderer components according to a time of day or a specific weather type
- LightAtDay / LightAtNight / LightAtWeather:
  Fade light intensities in and out according to a time of day or a specific weather type
- DeviceTime:
  Automatically set the time of day to the device time on scene start
- LoadSkyFromFile:
  Load exported sky dome parameters at runtime from a TextAsset that can be assigned via drag & drop



\n \section faq Frequently Asked Questions

Q: How can I get a TOD_Sky reference in my custom scripts?
- TOD_Sky.Instance keeps a static reference to the most recent sky dome that has been instantiated
- TOD_Sky.Instances keeps a static list of referenes to all sky domes that have been instantiated

Q: How can I use the sky dome with virtual reality devices like the Oculus Rift?
- Add the TOD_Camera script to one of the cameras (preferably the one that's being rendered first)
- The sky will render correctly without artifacts

Q: How can I render a cubemap or custom skybox at night?
- Select the shader "Time of Day/Space (Cube)" on the space material
- Assign your cubemap to the material

Q: How can I align the sky dome geographic directions with those of my scene?
- Rotate the sky dome around the y-axis such that the sun rises in the east of your scene

Q: How can I fix Z-fighting and sorting issues with the cloud shadows?
- Adjust the values for "Offset" directly in the shader code of the cloud shadow shaders

<strong>REMARK:</strong>
Offset values have to be constants and can therefore only be adjusted directly in the shader code. Suitable values depend on the depth buffer resolution of the targeted platform and hardware. While the default values work in most scenarios, some scenes might require some further tweaking.

Q: How can I disable some part of the sky dome?
- Disable any child game object to keep that specific part of the sky dome from rendering
- You can also disable any script on the parent game object individually to disable that specific functionality

<strong>REMARK:</strong>
Always disable entire child objects instead of their individual components like mesh renderers. The enabled states of components are being be modified by the sky dome scripts, which will otherwise override your changes.



\n \section contact Contact Information

If you have any questions that cannot be answered using the FAQ or documentation feel free to contact me:
- In the official <a href="http://forum.unity3d.com/threads/172763-Time-of-Day-Realistic-day-night-cycle-and-atmospheric-scattering">forum thread</a> of the package
- Via <a href="http://forum.unity3d.com/members/30479-plzdiekthxbye">personal message</a> on the Unity community forums
- Via <a href="https://twitter.com/andererandre">Twitter</a>
- Via <a href="http://modmonkeys.net/contact">my website</a>

<strong>REMARK:</strong>
I should always be able to reply within two work days. If I have not replied after several days, please try using a different method to contact me as there might be an issue with the one you chose. If I am not available for multiple days I will always try to announce this beforehand in the offical forum thread.



\n \section literature Literature

The following literature has been used to implement physically correct atmospheric scattering:
1. <a href="http://www.cs.utah.edu/~shirley/papers/sunsky/sunsky.pdf">Preetham, Shirley, Smits</a>
2. <a href="http://www.vis.uni-stuttgart.de/~schafhts/HomePage/pubs/wscg07-schafhitzel.pdf">Schafhitzel, Falk, Ertl</a>
3. <a href="http://www-ljk.imag.fr/Publications/Basilic/com.lmc.publi.PUBLI_Article@11e7cdda2f7_f64b69/article.pdf">Bruneton, Neyret</a>
4. <a href="https://engineering.purdue.edu/purpl/level2/papers/egsr2004_riley.pdf">Riley, Ebert, Kraus, Tessendorf, Hansen</a>
5. <a href="http://developer.amd.com/wordpress/media/2012/10/ATI-LightScattering.pdf">Hoffman, Preetham</a>
6. <a href="http://nishitalab.org/user/nis/cdrom/sig93_nis.pdf">Nishita, Sirai, Tadamura, Nakamae</a>
7. <a href="http://etd.dtu.dk/thesis/58645/imm2554.pdf">Nielsen, Christensen</a>

These papers are being referenced in the code in the following way:

    See [N] page P equation (E)

Where the letters are being replaced according to this:
- N: Paper #
- P: Page #
- E: Equation # (if available)



\n \section changelog Changelog

    VERSION 2.3.4
    -------------
    - Fixed moon position being vastly off
    - Fixed space texture tiling to infinity towards the horizon (could cause issues when rotating)
    - Tweaked horizon line for low haziness values
    - Tweaked the default prefab parameters
    - Disabled headless mode detection in-editor
    - Simplified and optimized TOD_Time calculations
    - Changed rendering order of sun and moon to support eclipses
    - Made inspector adjustments to the cycle properties correctly progress day, month and year
    - Made moon phase get calculated directly from the sun position
    - Removed Moon.Phase inspector variable (no longer required)
    - Removed Progress* fields from TOD_Time (no longer required)
    - Removed Moon (Flat) shader (adjusting Moon.Contrast now has the same effect)

    VERSION 2.3.3
    -------------

    - Added TOD_Sky.LoadParameters(...) to load exported parameters at runtime
    - Added LoadSkyFromFile example script
    - Added skybox material that is assigned to the render settings skybox for dynamic GI
    - Added TOD_Sky.Moon.HaloSize to increase or decrease the size of the moon halo
    - Added TOD_Sky.Reflection.ClearFlags to specify which clear flags to use for the reflection cubemap
    - Added TOD_Sky.Reflection.CullingMask to specify which layers to include in the reflection cubemap
    - Added warning to TOD_Camera if skybox clear flags are used (redundant with a sky dome)
    - Made parameter export and import remember the most recently specified path
    - Made the reflection cubemap less bright in the bottom hemisphere
    - Made light source color fall off to black before switching positions
    - Changed reflection baking to use a native Unity 5 realtime reflection probe (better quality)
    - Changed TOD_Sky.RenderToSphericalHarmonics(...) and TOD_Sky.RenderToCubemap(...) APIs
    - Renamed TOD_Components.*Shader to TOD_Components.*Material
    - Removed TOD_Sky.Fog.UpdateInterval (it's fast enough to update every frame anyhow)
    - Removed TOD_Sky.Fog/Ambient/Reflection.Directional (now part of fog mode, unused for the others)
    - Removed some parameters that are unused on Unity 3 and Unity 4 if running those versions

    VERSION 2.3.2
    -------------

    - Fixed that the sky dome would go into headless mode (i.e. black) on mobile
    - Fixed an error in Unity 5 Beta 14 (this means Beta 13 is no longer supported)
    - Made sky fogginess correctly affect the light intensity
    - Optimized coloring calculations
    - Renamed TOD_AmbientType.Flat to TOD_AmbientType.Color
    - Renamed TOD_AmbientType.Trilight to TOD_AmbientType.Gradient
    - Renamed TOD_Sky.RenderToSH3(...) to TOD_Sky.RenderToSphericalHarmonics(...)

    VERSION 2.3.1
    -------------

    - Fixed errors if sky dome renderers or mesh filters were deleted (i.e. when running on a server)
    - Fixed that ScatteringColor(...) in TOD_Scattering.cginc would add some stuff to its alpha value
    - Fixed issues if the main camera of a scene changes after scene load
    - Added TOD_Sky.World.Horizon to specify whether or not to adjust the horizon to zero level
    - Added TOD_Sky.UpdateFog(), TOD_Sky.UpdateAmbient() and TOD_Sky.UpdateReflection() to API
    - Added headless mode detection to skip some rendering calculations when running on a server
    - Made TOD_Sky.SampleAtmosphere(...) only include the moon halo if directLight is true
    - Made the moon halo always fade out when the moon is below the horizon
    - Made TOD_Sky.Cycle.DateTime have DateTimeKind.Utc instead of DateTimeKind.Unspecified
    - Made the fog color values clamp between 0 and 1 to avoid super bright glowing directional fog
    - Changed TOD_AdditiveColor and TOD_MoonHaloColor in TOD_Base.cginc to float3 (alpha is unused)
    - Removed TOD_Components.CameraTransform as it is no longer required

    VERSION 2.3.0
    -------------

    - Fixed atmosphere banding towards nighttime by adding dithering from a lookup texture
    - Fixed that SetupQualitySettings() would allocate 0.6kb of memory every frame
    - Added TOD_Animation.RandomInitialCloudUV to randomize the clouds at startup
    - Added optional shader "Moon (Flat)" for a flatter moon shading
    - Added TOD_Sky.World.ZeroLevel to set the zero / water level of a scene
    - Added TOD_Camera.DomePosOffset to specify a sky dome position offset relative to the camera
    - Added TOD_Sky.Initialized to check whether or not the sky dome has been initialized
    - Made RenderSettings.ambientLight get set in every ambient mode (for legacy shaders)
    - Made fog, ambient and reflection really get updated every single frame if their update interval is 0
    - Made sun and moon meshes fade out exactly at the horizon line
    - Made the color of the sky dome beneath the horizon line fade to a darker tone towards the bottom
    - Made the atmosphere shader additive (greatly improves moon / atmosphere blend)
    - Made the night texture fade to black at daytime (due to the new additive atmosphere)
    - Made the moon phase always be rotated towards the direction of the orbital path of the moon
    - Made the sun texture converge towards a circle for very high sun mesh brightnesses
    - Moved more enums to the global namespace and added the TOD_ prefix
    - Moved Cycle.Longitude, Cycle.Latitude and Cycle.UTC to the World parameter category
    - Changed the returned alpha value of TOD_Sky.SampleAtmosphere(...) to one
    - Changed the returned alpha value of ScatteringColor(...) in TOD_Scattering.cginc to one
    - Renamed TOD_Sky+Variables to TOD_Sky+API (now contains all API methods and properties)
    - Renamed TOD_Sky+Quality to TOD_Sky+Settings (now sets all project and scene settings)
    - Renamed TOD_SunShafts to TOD_Rays (now handles god rays of both sun and moon)
    - Renamed TOD_Sky.SunShaftColor to TOD_Sky.RayColor
    - Renamed TOD_Sky.Light.ShaftColoring to TOD_Sky.Light.RayColoring
    - Renamed TOD_Sky.Sun.ShaftColor to TOD_Sky.Sun.RayColor and added TOD_Sky.Moon.RayColor
    - Removed TOD_Sky.World.HorizonOffset and TOD_Sky.World.ViewerHeight (now covered by ZeroLevel)
    - Removed TOD_AmbientType.Hemisphere since it was removed from Unity 5 (use trilight instead)
    - Removed clampAlpha parameter from TOD_Sky.SampleAtmosphere(...)
    - Replaced TOD_Sky.Ambient.Exposure with Day.AmbientMultiplier and Night.AmbientMultiplier
    - Replaced TOD_Sky.Reflection.Exposure with Day.ReflectionMultiplier and Night.ReflectionMultiplier

    VERSION 2.2.0
    -------------

    - Fixed a moon shader compilation error in Unity 5 on Windows
    - Added support for Unity 5 ambient light modes (tricolor, hemisphere, spherical harmonics)
    - Added support for Unity 5 realtime reflections (sky cubemap)
    - Added TOD_Sky.Stars.Position to specify whether or not to move the stars with the earth rotation
    - Added TOD_Sky.SampleAtmosphere(...) overload that ignores direct light
    - Added TOD_Sky.RenderToCubemap(...) with various overloads
    - Added TOD_Sky.RenderToSH3(...) with various overloads
    - Added TOD_Sky.SampleFogColor(), TOD_Sky.SampleSkyColor() and TOD_Sky.SampleEquatorColor()
    - Added optional shader to project cubemaps onto the space object
    - Removed TOD_Sky.FogColor (access RenderSettings.fogColor instead)
    - Removed TOD_Sky.Stars.Density (directly adjust the texture instead)
    - Moved all fog parameters to TOD_Sky.Fog
    - Moved all ambient light parameters to TOD_Sky.Ambient
    - Moved all reflection parameters to TOD_Sky.Reflection
    - Made audio example scripts set the volume in OnEnable()

    VERSION 2.1.1
    -------------

    - Fixed various issues in gamma color space
    - Fixed time not properly incrementing in some cases if TOD_Time.ProgressDate was checked
    - Fixed some inconsistencies with the light and cloud color calculations, leading to better results overall
    - Fixed cloud shadow shape calculation being off for the lowest quality setting
    - Fixed cloud UV world space adjustments being off for rotated sky domes
    - Rescaled TOD_Sky.Light.CloudColoring (custom prefabs have to be readjusted accordingly)
    - Rescaled TOD_Sky.Night.CloudMultiplier (custom prefabs have to be readjusted accordingly)
    - Added TOD_Sky.Day.CloudColor and TOD_Sky.Night.CloudColor
    - Added TOD_Sky.Instance and TOD_Sky.Instances to easily get the most recent sky or all skies in the scene
    - Added TOD_Animation.WorldSpaceCloudUV
    - Added overloads of T() and ScatteringColor() that take distance into account to TOD_Scattering.cginc
    - Removed TOD_Base.cginc include from TOD_Scattering.cginc (now has to be included in the shader file)
    - Brought the sun shaft image effect up to date
    - Changed the code indentation policy (indent with tabs, align with spaces)
    - Prepared more parts of the codebase for Unity 5

    VERSION 2.1.0
    -------------

    - Added XML export and import of the prefab parameters
    - Added TOD_Scattering.cginc that contains functions to sample the scattering color
    - Added TOD_Base.cginc that contains shader parameters and common transformations
    - Added TOD_World2Sky and TOD_Sky2World shader matrices
    - Added TOD_Sky.Stars.Brightness parameter to make stars get affected by bloom image effects
    - Added TOD_Sky.LocalMoonDirection, TOD_Sky.LocalSunDirection and TOD_Sky.LocalLightDirection
    - Added TOD_Sky.Sun.MeshBrightness and TOD_Sky.Moon.MeshBrightness
    - Added TOD_Sky.Sun.MeshContrast and TOD_Sky.Moon.MeshContrast
    - Added TOD_Sky.Clouds.Glow to adjust the light source glow applied to the clouds
    - Added TOD_Sky.Atmosphere.FakeHDR to adjust the fake HDR mapping that is applied at dusk and dawn
    - Added TOD_Time.TimeCurve to specify a time progression curve for the day night cycle
    - Added two new cloud textures (the old ones can be deleted if unused)
    - Removed two unnecessary calls to InverseTransformDirection from TOD_Sky.SampleAtmosphere
    - Improved space texture to better work with the new brightness parameter
    - Improved visual quality of the atmosphere when using HDR
    - Improved cloud layer rendering
    - Made TOD_Sky.Cycle.DateTime accurate to one millisecond rather than one second
    - Made camera scripts automatically search for the sky dome if no reference is set in the inspector
    - Moved all moon parameters to TOD_Sky.Moon.X (was TOD_Sky.Night.MoonX and TOD_Sky.Cycle.MoonX)
    - Moved all sun parameters to TOD_Sky.Sun.X (was TOD_Sky.Day.SunX)

    VERSION 2.0.9
    -------------

    - Fixed time not getting incremented properly
    - Fixed inaccuracies when progressing time and moon phase with extremely high frame rates
    - Fixed inaccuracies when progressing time and moon phase with extremely fast time scales

    VERSION 2.0.8
    -------------

    - Fixed that sun and moon could visibly pop in and out if scaled extremely huge
    - Fixed that the date would not get fully incremented for extremely fast time scales
    - Fixed that the sun shafts could go through clouds
    - Tweaked the TOD_Sky.IsDay and TOD_Sky.IsNight thresholds
    - Replaced TOD_Time.UpdateInterval with TOD_Sky.Light.UpdateInterval (now only affects the light source)
    - Prepared parts of the codebase for Unity 5 (specifically the new transform behaviour)

    VERSION 2.0.7
    -------------

    - Fixed an issue where the ambient light color would never fully lerp to the night value

    VERSION 2.0.6
    -------------

    - Replaced Day/Night.AmbientIntensity with Day/Night.AmbientColor to offer more customization options
    - Added Light.AmbientColoring to adjust ambient light coloring at dusk and dawn
    - Added example scripts to enable / disable lights in the scene at day / night / weather
    - Added inspector variable to adjust the time update interval in TOD_Time
    - Added option to use the real-life moon position rather than the fake "opposite to sun" moon position
    - Made all components of TOD_Sky initialize before Start() so that they are accessible from other scripts
    - Disabled the automatic light source shadow type adjustment so that the user can manually set it

    VERSION 2.0.5
    -------------

    - Changed cloud scale parameters from float to 2D vectors to define different scales in x and y direction
    - Fixed TOD_Camera always causing the scene to be edited if enabled
    - Fixed cloud inconsistencies between linear and gamma color space
    - Fixed moon halo disappearing in gamma color space and made the color alpha affect its visibility
    - Fixed an issue where the demo mouse look script could overwrite previously imported Standard Assets
    - Fixed possible sun and moon gimbal lock that could cause them to spin towards zenith
    - Fixed sun shafts being too faint in some setups
    - Improved overall lighting calculations
    - Improved moon visuals
    - Made the sky dome play nice with "depth only" clear flags
    - Made the cloud coloring still darken the clouds even for very low values
    - Made Components.Animation.CloudUV modulo with the cloud scale to avoid unnecessarily large values
    - Added inspector variables to adjust sun shaft base color and sun shaft coloring
    - Added the property Cycle.Ticks to get the time information as a long for easy network synchronization
    - Added the property Cycle.DateTime to get the time information as a System.DateTime
    - Added an inspector variable to set a minimum value for the light source height

    VERSION 2.0.4
    -------------

    - Added a property for the atmosphere renderer component to TOD_Components
    - Added properties for all child mesh filter components to TOD_Components
    - Changed the quality settings to be adjustable at runtime via public enum inspector variables
    - Merged the three prefabs into a single prefab as separate quality prefabs are no longer required
    - Fixed the materials always showing up in version control
    - Fixed the sky dome always causing the scene to be modified and the editor always asking to save on close
    - Fixed the customized sky dome inspector not always looking like the default inspector
    - Improved the performance of all cloud shaders by reducing interpolations from frag to vert
    - Improved the visuals of all cloud shaders and streamlined their style
    - Increased the default cloud texture import resolution to 1024x1024
    - Added a white noise texture for future use

    VERSION 2.0.3
    -------------

    - Fixed all issues with DX11 rendering in order to fully support DX11 from this point on

    VERSION 2.0.2
    -------------

    - Fixed an issue where the image effect shaders could overwrite previously imported Standard Assets

    VERSION 2.0.1
    -------------

    - Changed date and time organization to represent the valid Gregorian calendar
    - Addressed issues with the Unity sun shaft image effect by providing a modified image effect
    - Fixed clouds not correctly handling the planetary atmosphere curvature
    - Fixed clouds not offsetting according to the world position of the sky dome
    - Fixed cloud glow passing through even the thickest of clouds
    - Fixed cloud shadow projection
    - Fixed Light.Falloff not affecting the toggle point of the light position between sun and moon
    - Automatically disable the corresponding shadows if Day/Night/Clouds.ShadowStrength is set to 0
    - Removed Clouds.ShadowProjector toggle as it is no longer required
    - Tweaked the old moon halo to not require an additional draw call and added it back in
    - Made the sky dome position in world space add an offset to the cloud UV coordinates
    - Added Light.Coloring to adjust the light coloring separate from the sky coloring
    - Rescaled some parameters for easier use and tweaked their default values

    VERSION 2.0.0
    -------------

    - Moved all documentation to Doxygen
    - Renamed the folder "Sky Assets" to "Assets"
    - Made the color space be detected automatically by default
    - Reworked the sun texture and shader
    - Allow light source intensities greater than one
    - Reworked the way ambient light is being calculated
    - Reworked the way light affects the atmosphere and clouds
    - Improved all scattering calculations, especially the integral approximation
    - Automatically disable space the game object at night
    - Added a public method to sample the sky dome color in any viewing direction
    - Added a fog bias parameter to lerp between zenith and horizon color
    - Adjusted the atmosphere alpha calculation
    - Added a parameter to easily adjust the scattering color
    - Added shader parameters for the moon texture color and contrast
    - Adjusted the render queue positions
    - Removed the moon halo material as it is no longer required
    - Added the physical scattering model to the night sky
    - Greatly improved the weather system
    - Added fog and contrast parameters to the atmosphere
    - Restructured the parameter classes to be more intuitive to use
    - Moved all component references into a separate class
    - Made the sky presets be applied via editor script rather than separate prefabs
    - Improved cloud shading and performance across the board
    - Removed the cloud shading parameter
    - Added cloud glow from the sun and moon
    - Added sky and cloud tone multipliers to sun and moon
    - Added viewer height and horizon offset parameters
    - Slightly improved overall performance
    - Replaced ambient intensity with two parameters for sun and moon
    - Replaced the two directional lights with a single one that automatically follows either sun or moon

    VERSION 1.7.3
    -------------

    - Added two parameters "StarTiling" and "StarDensity" to the "Night" section
    - Added "Offset -1, -1" to the cloud shadow shaders to avoid Z-fighting on some platforms
    - Tweaked the cloud shader for more consistent results in linear and gamma color space
    - Tweaked the moon texture to be a lot brighter by default, especially on mobile
    - Tweaked the automatically calculated fog color to be similar to the horizon color
    - Removed the property "Brightness" from the moon shader as it is no longer needed

    VERSION 1.7.2
    -------------

    - Fixed the ambient light calculation being too dark, even with high ambient light parameter values
    - Added the properties "SunZenith" and "MoonZenith" to access sun and moon zenith angles in degrees
    - Added a paramter "Halo" to adjust the moon halo intensity and made its color be derived from the light
    - Changed several parameters to be clamped between 0 and 1
    - Changed the name of the property "OrbitRadius" to "Radius"
    - Tweaked the moon phase calculation of both moon mesh and moon halo
    - Tweaked several default parameter values of the prefabs

    VERSION 1.7.1
    -------------
    - Changed the default cardinal direction axes of the sky dome (x axis is now west/east, z axis south/north)
    - Removed the property "ZenithFactor" as it is no longer being used
    - Moved all child object references into a separate toggleable section called "Children"
    - Tweaked the default parameters of the prefabs (brightness, haziness, cloud color, moon light intensity)
    - Tweaked the calculations of the moon light color, ambient light at night and cloud tone at night
    - Tweaked the default sun and moon base color based on good real life approximations
    - Tweaked the moon halo
    - Renamed the parameter "ShadowAlpha" in "Clouds" to "ShadowStrength"
    - Added the parameter "ShadowStrength" for the sun and moon lights

    VERSION 1.7.0
    -------------

    - Fixed an issue where the sun could incorrectly travel around the north,
      even though the location is in the northern hemisphere (Thanks Gregg!)
    - Fixed an issue that led to the brightest parts of the sky dome being slightly too dark
    - Fixed the automatically calculated fog color not being exactly the same as the horizon
    - Added a name prefix to all components to prevent name collisions with other packages
    - Added cloud shadows (can be disabled)
    - Added UTC time zone support
    - Added a parameter to configure the color of the light reflected by the moon
    - Added parameters for wind direction in degrees and wind speed in knots
    - Added an option to automatically adjust the ambient light color (disabled by default)
    - Added a parameter to adjust the sun's light color
    - Added a plane with an additive shader at the sun's position to always render a circular sun
    - Added dynamic cloud shape adjustments to the "Low" prefab (cloud weather types will now also work)
    - Added shading calculations to the "Low" and "Medium" prefabs
    - Improved the performance of "Low" prefab by reducing the vertex count
    - Improved the performance of "Low" prefab by removing the moon halo for that prefab by default
    - Improved the cloud shading of the "High" prefab
    - Improved the visual quality of the weather presets
    - Improved the calculation of the sun's position
    - Changed the automatic fog color adjustment to be disabled by default
    - Changed the moon halo to adjust according to the moon phase
    - Changed the name of the parameter from "Color" to "AdditiveColor" for both day and night
    - Changed the cloud animation to support network synchronization
    - Changed the default tiling of the stars texture to 1 (was 3)
    - Changed the moon vertex count in all presets to scale with the device performance
    - Removed the parameter "CloudColor" from "NightParameters" as it is now derived from the moon light color

    VERSION 1.6.1
    -------------
    - Fixed an issue related to HDR rendering

    VERSION 1.6.0
    -------------

    - Improved the visuals and functionality of the weather system
      (most METAR codes should now be possible to achieve visually)
    - Improved performance of the moon halo shader
    - Added official support for HDR rendering
    - Replaced the sun mesh with implicit sun scattering in the atmosphere layer
      to reduce dome vertex count, draw calls and pixel overdraw
    - Added an additional quality level (now Low/Medium/High instead of Desktop/Mobile)
    - Added sky dome presets from various locations around the globe for easier use
    - Tweaked the wavelength constants a little to allow for a wider range of sun coloring adjustments

    VERSION 1.5.1
    -------------

    - Fixed an issue causing a missing sun material in the mobile prefab

    VERSION 1.5.0
    -------------

    - Enabled mip mapping of the stars texture by default to avoid flickering
    - Added support for using custom skyboxes at night (see readme for details)
    - Greatly improved the parametrization of the sun color influence at sunrise and sunset
    - Added internal pointers to commonly used components for faster access
    - Split the sun and moon parameters into their own property classes
    - Adjusted the cloud shading calculation to keep it from darkening some clouds too much
    - Adjusted the color wavelengths to produce a more realistic blue color of the sky by default
    - Made the moon phase influence the intensity of the sunlight reflected by the moon
    - Replaced the lens flares with custom halo shaders that are correctly being occluded by clouds
    - Enabled the new halo effects on mobile
    - Moved all shaders into a "Time of Day" category
    - Added a basic weather manager with three weather types

    VERSION 1.4.0
    -------------

    - Added "Fog { Mode Off }" to the shaders to properly ignore fog
    - Added the parameter "Night Cloud Color" to render clouds at night
    - Added the parameter "Night Haze Color" to render some haze at night
    - Added the parameter "Night Color" to add some color to the night sky
    - Renamed the parameter "Haze" to "Haziness"
    - Renamed the parameter "Sky Tone" to "Brightness"
    - Renamed the properties "Day" and "Night" to "IsDay" and "IsNight"
    - Restructured all sky parameters into groups
    - Improved the sun lens flare texture
    - Improved the stars texture
    - Fixed a rendering artifact at the horizon for low haziness values
    - Made the scattering calculation in gamma space look identical to linear space

    VERSION 1.3.0
    -------------

    - Greatly improved performance on mobile devices
    - Greatly improved sunset and sunrise visual quality
    - Added a parameter to control how strongly the sun color affects the sky color
    - Added realistic sun and moon lens flare effects
    - Added two additional cloud noise textures
    - Improved handling of latitude and longitude
    - Made the sky dome render correctly independent of its rotation

    VERSION 1.2.0
    -------------

    - Fixed some bugs regarding linear vs. gamma space rendering
    - Fixed some issues with the horizon fadeout
    - Adjusted sun and moon size
    - Optimized sun and fog color calculation
    - Greatly improved visual quality of the cloud system
    - Added parameter to control cloud tone, allowing for dark clouds
    - Added improved stars texture at night
    - Added parameter to control the sun color falloff speed

    VERSION 1.1.0
    -------------

    - First public release on the Asset Store

    VERSION 1.0.0
    -------------

    - First private release for internal use

*/
