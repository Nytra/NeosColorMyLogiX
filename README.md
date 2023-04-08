# ColorMyLogiX

A [NeosModLoader](https://github.com/zkxs/NeosModLoader) mod for [Neos VR](https://neos.com/) that allows you to color your LogiX nodes. This makes the LogiX experience *much* more colorful.

## Installation
1. Install [NeosModLoader](https://github.com/zkxs/NeosModLoader).
1. Place [ColorMyLogiX.dll](https://github.com/Nytra/NeosColorMyLogiX/releases/latest/download/ColorMyLogiX.dll) into your `nml_mods` folder. This folder should be at `C:\Program Files (x86)\Steam\steamapps\common\NeosVR\nml_mods` for a default install. You can create it if it's missing, or if you launch the game once with NeosModLoader installed it will create the folder for you.
1. Start the game. If you want to verify that the mod is working you can check your Neos logs.

## What does this actually do?
It sets the Tint color on the Image component for LogiX nodes that have been newly created or newly unpacked by you. The colors are not permanent, they will go back to the default if the node gets unpacked by someone else. No extra slots or components are created. The way that the nodes get colored can be configured via [NeosModSettings](https://github.com/badhaloninja/NeosModSettings).

## Config
![colormylogix_config_owo_4](https://user-images.githubusercontent.com/14206961/230274771-4606954c-8add-459d-8d96-068b6c7bde1c.png)
The default config is the one that I (Nytra) personally use and it has good values for regular use. You shouldn't need to reconfigure anything unless you really want to.

There are a lot of options here to give you a lot of control over the types of colors that get generated. If you want dark mode or grayscale logix, rainbow, pastel pink or shades of green/blue, you can make that happen with a bit of configuration.

The static node color can be used if you just want a single color to be used for all nodes. There is also an option here to use a random range around this color to allow for some subtle (or not-so-subtle) variations.

You can select which Node Factor is used to seed the randomness in the dynamic section. It is generally best to go with Node Category for this one. The others will introduce more or less variations. Using FullTypeName for example will cause the color of the node to change when it gets overloaded to another type. Choosing RefID will make every node you create have a different color. TopmostNodeCategory is like NodeCategory, except it ignores nested categories and only cares about the first one. NodeName uses the slot name of the node.

You can use the output RGB multiplier to suppress or amplify the color channels of red, green or blue. If you don't want any red in your nodes, set the multiplier for red to zero. Or amplify it, if you like.

The random seed can be changed to get a completely different set of random colors being generated.

![20230314204710_1](https://user-images.githubusercontent.com/14206961/230007411-8b7b9387-019b-4918-8974-8b7c8553f367.jpg)
![20230225035345_1](https://user-images.githubusercontent.com/14206961/230007717-d8d3ffbf-9e50-48d0-a5f4-0c91dc91d67f.jpg)
![20230407033902_1](https://user-images.githubusercontent.com/14206961/230702924-0649d190-b838-4edd-bfab-fd218fa5ac22.jpg)
