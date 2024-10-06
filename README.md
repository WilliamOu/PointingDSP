# Pointing Dual Solution Paradigm
PointingDSP is an open-source project initially developed for the Hegarty Spatial Thinking Lab at the University of California, Santa Barbara, designed to facilitate research on spatial reasoning through an interactive maze-navigation task.

The most up-to-date build of the study may be found under 'Build & Patch History/', which contains the setup for conducting the original study (refer to "Original Gamemodes & Objectives" for details). Download of the entire repository for the source code is only necessary for those interested in modifying or expanding the study (see "Source Code & Unity Asset Store Policy" for instructions on how to rebuild the project).

Note that there may be bugs, as the project is still under active development.

# Original Gamemodes & Objectives
The study supports three different gameplay modes:
- Desktop Mode: Standard first-person controls with keyboard and mouse.
- Virtualizer Mode: Compatible with the Cyberith Virtualizer Omnidirectional Treadmill.
- Roomscale VR Mode: Standard room-scale virtual reality.

Each mode logs participant data, with VR modes capturing additional details, such as eye-tracking data. 

While in the study, participants progress through the following stages:
- Training Stage: Participants familiarize themselves with the controls of their respective mode.
- Learning Stage: Participants navigate a maze circuit to learn its layout.
- Retracing Stage: Participants retrace the exact path they took during the Learning Stage.
- Pointing Stage: Participants are presented with randomized trials and must indicate the direction of a prompted object.
- Wayfinding Stage: Participants navigate randomized trials to locate the prompted objects.

# Presets and Map Creation
A wide array of parameters and settings can be adjusted and saved as presets, allowing for flexible customization of the study's style and structure.
Additionally, PointingDSP offers tools for creating custom maps, a hybrid build featuring a sandbox-like voxel engine supporting everything from custom block types to custom trial lists and the importing of 3D models in real-time.

# Source Code & Unity Asset Store Policy
In adherence with the Unity Asset Store Terms of Service, which prohibits the redistribution of Store assets, several Package Manager Assets and Asset Store Plugins have not been included in this open-source build. As such, the project not compile immediately upon download. The following assets must be manually installed for the project to function:

From Window > Package Manager:
- Unity UI
- TextMeshPro
- OpenXR Plugin
- Unity glTFast 

From Window > Asset Store:
- QuickOutline
- SteamVR

From Assets > Import Package > Custom Package:
- CybSDK

Note that the CybSDK package is only required for those who wish to run studies with the Cyberith Virtualizer, and may be found on the Cyberith website.
After initializing the missing dependencies, ensure that XR is set to be initialized on startup via Project Settings > XR Plug-in Management > Initialize XR on Startup. Also ensure that "OpenVR Loader" and "OpenXR" are checked off.

# Attribution
This project is licensed under the Apache License 2.0. 

You are free to use, modify, and distribute this software, but you **must credit me via attribution**. Failure to do so will be a violation of this license. 
And, while not legally required, I kindly request that said credit be given in a prominent manner (e.g., including the author's name in the project's documentation or credits screen).