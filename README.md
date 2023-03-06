# ColorMyLogiX

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that allows you to color your LogiX nodes. This makes the LogiX experience *much* more colorful.

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [ColorMyLogiXNodes.dll](https://github.com/Nytra/NeosColorMyLogiXNodes/releases) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## What does this actually do?
It writes to the Tint color on the Image component for LogiX nodes that have been newly created or newly unpacked by you. The way that the nodes get colored can be configured via [NeosModSettings](https://github.com/badhaloninja/NeosModSettings).
