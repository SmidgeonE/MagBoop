# IMPORTANT INFO

https://youtu.be/JIxhjL_VJ14 - Video to demonstrate.

This mod requires stovepipe. If you do not wish to use it, you can enter the stovepipe config in thunderstores config editor, then disable it. Of course this mod also has a config, where you can change various probabilities to your hearts content.

# How to add exclusions

You can now exclude any weapon you want from the mag unseating function. Simple head to Users/'YourUsername'/AppData/Roaming/StovepipeData/MagBoopExclusions.json.

Here you can add any weapon name you wish. Note, do not include the (Clone) that is sometimes added to weapons' names. 
You can find the name of the weapon using the hand menu while holding the weapon. May need a restart to take effect.

# MagBoop

This mod is still under development, so may have some issues. 
The sound effects may be a bit whack at times. Some of the triggers for certain magazines aren't aligned well, and some magazines for modded weapons don't make sounds, this isn't something I can easily fix unfortunately.

This mod should work for pretty much every weapon in the game that has a removable magazine.

# Contact

If you wish to support me, my kofi is https://ko-fi.com/smidgeon

If you have any issues / ideas / need help with modding, I am always available on discord in the homebrew server under the name Smidgeon, tag ප bir𝛿 ꧁꧂#9320 (not sure if thunderstoreeven supports those characters).


# Changelog

1.4.0 - Added new Third Law thing, where the weapon moves when you boop it. Of course it can be disabled in the config, at the bottom under "Third Law"

1.3.2 - Removed double feed probability modifiers when the mag is half in. The code I wrote for it was awful and no one will notice that it's gone anyway :)

1.3.1 - New icon thanks to Østrem!

1.3.0 - Improved Valve Index mag triggers. Added better mag-boop sound for half-way boop. Added exclusion list system, simply find the name of the weapon you want to no longer have mag unseating, and you can remove it. More in the upper description. Added option to exclude all belt feds (on by default). Adjusted some magazines triggers to be better. Added multipliers so that Closed Bolt Weapons are more likely to not be seated when the bolt is closed (adjustable in config), with a specific config option for HK-style weapons (typically have this issue a lot more). Thank you to HylianWolf on discord for the info. Fixed bug with Stovepipe's bullet creep not working correctly with MagBoop. 

1.2.1 - Improved slide boop trigger positions.

1.2.0 - Removed AK-style weapons from mag boop. Added "slide boop", so that you can ensure it is fully forward, works great with new Stovepipe update, where the slide may fail on some handguns to fully enter battery. Fixed bugs.

1.1.0 - Added buzz when mag booping. Added user customisable probabilities based on weapon type. Added better (hopefully) sounds. Added sounds for environment boops. Slightly increased trigger size. Fixed many issues with top-loading weapons. 

1.0.8 - Removed debug cube... again...

1.0.7 - If you push the magazine with a strong force, it will decrease the probability that it will not fully go in.
 This is customisable in the config, with probabilities. Fixed many triggers on certain weapon magazines. Added correct mag boop logic for weapons that have magazines that insert above / to the side of the weapon.

1.0.6 - Removed debug cube.

1.0.5 - Added sound effects so its more obvious if the mag is not seated correctly. Fixed bugs with certain capsule-shaped magazines. 

1.0.4 - Disabled weird invisible trigger. Disabled mag booping for weapons that dont need it (i.e. G11)

1.0.3 - Fixed many sources of undeseriable mag boops. added ability to boop slinged weapons. adjusted angle logic for booping. Added check for gripping, to remove unwanted boops.

1.0.2 - Updated so it actually works now.

1.0.1 - More info in readme.

1.0.0 - Initial release.
