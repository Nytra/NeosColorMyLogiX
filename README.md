# ColorMyLogiX

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that allows you to color your LogiX nodes. This makes the LogiX experience *much* more colorful.

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [ColorMyLogiX.dll](https://github.com/Nytra/NeosColorMyLogiXNodes/releases) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## What does this actually do?
It sets the Tint color on the Image component for LogiX nodes that have been newly created or newly unpacked by you. The colors are not permanent, they will go back to the default if the node gets unpacked by someone else. No extra slots or components are created. The way that the nodes get colored can be configured via [NeosModSettings](https://github.com/badhaloninja/NeosModSettings).

## Config
![colormylogix_config_owo_2](https://user-images.githubusercontent.com/14206961/230002625-376a6e71-3853-4c33-ba0b-8b2d46f59e3e.png)
The default config is the one that I (Nytra) personally use and it has good values for regular use. You shouldn't need to reconfigure anything unless you really want to.

There are a lot of options here to give you a lot of control over the types of colors that get generated. If you want dark mode or grayscale logix, rainbow, pastel pink or shades of green/blue, you can make that happen with a bit of configuration.
