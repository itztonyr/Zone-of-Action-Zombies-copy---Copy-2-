Thanks for purchase Customizer addon for MFPS.

Version 2.1.0

For full documentation please unzip the "documentation" file out of unity project for avoid errors.
or see the online documentation:
http://lovattostudio.com/documentations/customizer/

MFPS:---------------------------------------------------------------------------------------------

For use with MFPS:

USE THE IN-EDITOR TUTORIALS, IN UNITY TOOLBAR -> MFPS -> Tutorials -> Customizer

After import the package go to (Toolbar)MFPS -> Addons -> Customizer -> Enable
Then add the 'Customizer' scene to the Build Settings, the scene is located in: Assets -> Addons -> Customizer -> Content -> Scene -> *
Use the example player prefab which have the rifle integrated with customizer for test the system, the prefab is located in the 'Resources' folder of the addon.

   Change Log:

2.1.0
-Compatible with MFPS 1.10.0
-Fix: The attachment position buttons were not working with the gamepad pointer.
-Fix: Locked camos show no-allowed coins in the coin price UI.

2.0.3
-Fix: Randomize attachments and camos apply locked camos to the weapon even when they are not purchased.
-Improved: Now the Customizer weapon info no longer require to set a weapon name, now it will use the weapon name from the weapon GameData info.
-Fix: Weapon fire override sound gets reset when changing the camera view from first-person to third-person.

2.0.2
-Fix: Can't scroll the camo list when there are a lot of camo options.

2.0.1
-Fix: bl_AttachmentGunModifier > Fire Sound modifier was not affecting the weapon fire sound.

2.0.0
-Improved: Use TextMeshPro instead of UGUI Text.
-Fix: Example Shotgun in the customizer scene have the wrong GunID.
-Fix: Attachment buttons selection doesn't hide when switch weapon in the customizer scene.

1.9.1
-Fix: Launcher-type weapons do always appear locked even if the player purchases or unlocked them.

1.9
-Compatibility with MFPS 1.9
-Improve: Integrate with the new MFPS Coin system.
-Improve: Add support for camos in weapons with multiple sub-meshes.
-Fix: The example rifle weapon transfers the attachments with a different scale.
-Fix: bl_AttachmentGunModifier keeps adding zoom every time that aiming with a scope.
-Fix: Error at the start of the Customizer scene after rename a weapon in GameData.
-Improve: Add more weapon modifier properties, now attachments can also modify the weapon weight, range, aim speed, reload time, bullet speed and recoil.
-Improve: Added a warning message to let you know when the CustomizerData attachment list is out of sync, which causes attachments to not properly transfer.
-Fix: Weapons with SkinnedMeshRender cause errors when trying to transfer the attachments.
-Improve: Add 'Aim Position' helper in bl_AttachmentGunModifier inspector so you can easily modify the aim position modifier for your scopes/sight attachments.

1.8.5
-Improve: Compatibility with Third Person View addon.

1.8.2
-Fix: The cobra sight example mesh was not included in the package.
-Improve: Add help boxes in the bl_CustomizerWeapon inspector for better understand of common issues.

1.8
-Improve: Exit window UI.
-Improve: Attachment Scroll view.
-Improve: Rotation input on touch devices.

1.7.4
-Fix: Randomize feature used non-purchased camos.
-Fix: Weapon list display over other UI weapon there's more than 10 weapons added.

1.7.3
-Improve: Attachment gun modifier, add: Extra Damage, Extra Spread and Extra Aim Zoom options.
-Improve: Add documentation about the attachment gun properties modifier.

1.7
-Improve: Integrated with Shop addons, now you can set price for camos in order to be for sale in the In-Game shop (using Shop addon).
-Improve: Added: Global Camos, now you only have create 1 Camo Info for all weapons instead of one per weapon, you can set a global preview of the camo or can override per weapon.
-Improve: Integrate with Shop addon, now weapons will not be available to customize if the weapon is for sale and it's not purchased by the player yet.
-Improve: Integrate with Level Manager addon, now weapons will not be available to customize if the weapon require a minimum player level and player doesn't meet it.
-Improve: Now the weapon icon is displayed in the button of customizer weapon list instead of just the weapon name.
-Fix: Missing scripts in example player prefab.
-Improve: Now you don't have to set the unique ID for each camo of each weapon.
-Fix: Can't assign bl_Gun reference in bl_AttachmentGunModifier inspector.

1.6
-Add: In-Editor Documentation
-Improve: Easily way to transfer attachments in weapons models (customizer, tp and fp weapon)
-Fix: Error if an attachment model is not assigned in the customizer weapon.
-Fix: Weapons reset attachments after change of weapon.
-Improve: Integrate the Shotgun, Sniper and Pistol example weapons of MFPS.
-Improve: bl_AttachmentGunModifier.cs

1.5.3
- Add attachment gun modifier, allow change properties of the weapon like: Aim Position, Fire Sound and Bullets when the attachment is active.


Any problem or question please contact us.
http://www.lovattostudio.com/en/select-support/