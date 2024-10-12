using MFPS.Addon.Customizer;
using MFPSEditor;
using UnityEditor;
using UnityEngine;

public class CustomizerTutorial : TutorialWizard
{
    //required//////////////////////////////////////////////////////
    private const string ImagesFolder = "customizer/editor/";
    private NetworkImages[] m_ServerImages = new NetworkImages[]
    {
        new NetworkImages{Name = "img-1.png", Image = null},
        new NetworkImages{Name = "img-2.jpg", Image = null},
        new NetworkImages{Name = "img-3.jpg", Image = null},
        new NetworkImages{Name = "img-4.jpg", Image = null},
        new NetworkImages{Name = "img-5.jpg", Image = null},
        new NetworkImages{Name = "img-6.jpg", Image = null},
        new NetworkImages{Name = "img-7.jpg", Image = null},
        new NetworkImages{Name = "img-8.jpg", Image = null},
        new NetworkImages{Name = "img-9.jpg", Image = null},
        new NetworkImages{Name = "img-10.jpg", Image = null},
        new NetworkImages{Name = "img-11.jpg", Image = null},
        new NetworkImages{Name = "img-12.jpg", Image = null},
        new NetworkImages{Name = "img-13.jpg", Image = null},
        new NetworkImages{Name = "img-14.jpg", Image = null},
        new NetworkImages{Name = "img-15.jpg", Image = null},
        new NetworkImages{Name = "img-16.jpg", Image = null},
        new NetworkImages{Name = "img-17.jpg", Image = null},
        new NetworkImages{Name = "img-18.jpg", Image = null},
    };
    private readonly GifData[] AnimatedImages = new GifData[]
   {
        new GifData{ Path = "cmz-ps.gif" },
   };
    private Steps[] AllSteps = new Steps[] {
     new Steps { Name = "Get Started", StepsLenght = 0 },
    new Steps { Name = "Add Weapon", StepsLenght = 4 },
    new Steps { Name = "Add Attachments", StepsLenght = 2 },
    new Steps { Name = "Add Camos", StepsLenght = 0 },
    new Steps { Name = "SetUp Players", StepsLenght = 0 },
    new Steps { Name = "Modifiers", StepsLenght = 0 },
    new Steps { Name = "Multi Camo Renders", StepsLenght = 0 },
    new Steps { Name = "Attachment Assets", StepsLenght = 0 },
    };

    //final required////////////////////////////////////////////////
    AssetStoreAffiliate affAssets;

    public override void OnEnable()
    {
        base.OnEnable();
        base.Initizalized(m_ServerImages, AllSteps, ImagesFolder, AnimatedImages);
        GUISkin gs = Resources.Load<GUISkin>("content/MFPSEditorSkin") as GUISkin;
        m_GUISkin = gs;
        if (gs != null)
        {
            base.SetTextStyle(gs.customStyles[2]);
        }
    }

    public override void WindowArea(int window)
    {
        if (window == 0)
        {
            DrawGetStarted();
        }
        else if (window == 1)
        {
            DrawAddWeapon();
        }
        else if (window == 2)
        {
            DrawAddAttachments();
        }
        else if (window == 3)
        {
            DrawCamos();
        }
        else if (window == 4) { SetupPlayerDoc(); }
        else if (window == 5) { MofidiersDoc(); }
        else if (window == 6) { MultiCamoRenderDoc(); }
        else if (window == 7) { AttachmentsModelsDoc(); }
    }

    void DrawGetStarted()
    {
        DrawSuperText("<?background=#FFF>GET STARTED</background>\n\n■ After import the package go to (Toolbar) MFPS ➔ Addons ➔ Customizer ➔ <b>Enable.</b>\n■ Add the '<b>Customizer</b>' scene to the Build Settings, the scene is located in: Assets➔Addons➔Customizer➔Content➔Scene➔ *\n \nUse the <?link=asset:Assets/Addons/Customizer/Resources/MPlayer [Customizer].prefab>example player prefab</link> which have the default MFPS weapons already integrated with customizer for test the system, the prefab is located in the 'Resources' folder of the addon.\n\nTo open the Customizer Menu in game, in the Lobby Menu ➔ (Click) in the top <b>Customizer</b> button.");
        DrawImage(GetServerImage(0));

    }

    void DrawAddWeapon()
    {
        if (subStep == 0)
        {
            DrawText("When you add a new weapon to your player prefabs and you want to the player be able to customize it with the Customizer menu, you have to set up the weapon:\n \n<b>1.</b> You have to set up the weapon in the Customizer scene which is what you will learn to do in this section.\n \n<b>2.</b> After you set up the weapon in the Customizer scene you have to transfer the weapon set up to the Player Prefabs weapon, which is an automated process that you can learn in the <b>Setup Player</b> section.\n \nThese are the steps required to integrate a weapon on Customizer.");
            DownArrow();
            DrawTitleText("Add Model");
            DrawText("In the <b>Customizer</b> scene <i>(Assets➔Addons➔Customizer➔Content➔Scenes➔*)</i>, go to the hierarchy window ➔ Foldout the CustomizerManager ➔ Root ➔ *, there you will see all the already set up weapons and an object called <b>'__TEMPLATE__'</b>\nwhat you have to do is duplicated that object <i>(__TEMPLATE__)</i>, enable the duplicate instance and change the gameobject name to the name of your new weapon to identify it easily in the hierarchy, we gonna call this <b>Customizer Weapon</b>, keep in mind this term.");
            DrawImage(GetServerImage(1));
            DownArrow();
            DrawText("Drag your new weapon model prefab <i>(just the weapon model without arms)</i> and place inside of <i>*Your New Customizer Weapon*</i> object <i>(the one that you just duplicate)</i> inside of the child <b>Model</b>  as shown in the image below:");
            DrawImage(GetServerImage(2));
            DownArrow();
            DrawText("Now positioned the weapon model in the center of the Game View Screen Camera, use one of the already set up weapons as reference if is needed");
            DrawImage(GetServerImage(3));
            DownArrow();
            DrawText("Then in the inspector window of CustomizerManager -> bl_CustomizerManager -> in the <b>AllCustom</b> List -> Add a new field and in it drag the new weapon <i>(the duplicated object not the model)</i> OR just click in the button bellow to do this " +
                "automatically.");
            Space(5);
            if (DrawButton("Refresh Weapon List"))
            {
                bl_CustomizerManager cm = FindObjectOfType<bl_CustomizerManager>();
                bl_Customizer[] all = cm.transform.GetComponentsInChildren<bl_Customizer>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    if (!cm.AllCustom.Contains(all[i]) && all[i].name != "***TEMPLATE***")
                    {
                        cm.AllCustom.Add(all[i]);
                        EditorUtility.SetDirty(cm);
                    }
                }
                Debug.Log("List Refreshed");
            }
            DownArrow();
            DrawText("Probably the button (the circles with a dot) will not in the correct position for your new weapon, for fix it simply go under <b>Buttons</b> -> there you will have 5 buttons, one for each attachment type and one for the camo," +
                "position it as desired");
            DrawImage(GetServerImage(9));
        }
        else if (subStep == 1)
        {
            DrawTitleText("CREATE INFO");
            DrawText("Now you have to create the information of the new weapon, in this you will have to setup all the camos and attachments that this weapon will have <i>(just the info not the models yet)</i>");
            DownArrow();
            DrawText("Go to <i>Assets -> Addons -> Customizer -> Resources -> <b>CustomizerData</b></i> or click on the button bellow to located automatically");
            Space(5);
            if (DrawButton("CustomizerData"))
            {
                bl_CustomizerData cd = Resources.Load<bl_CustomizerData>("CustomizerData");
                Selection.activeObject = cd;
                EditorGUIUtility.PingObject(cd);
            }
            DownArrow();
            DrawText("Now in the Project View Window you'll see a list called <b>Weapons</b>, in this list add a new field.\n \nIn the new field change the Weapon Name with the one of your new weapon and assign the correct <b>GunID</b>" +
                 " (with the one from GameData), then you have two list more: Camos and Attachments, in these you have to list all the camos and attachments infos respectively that this weapon will have.");
            DrawImage(GetServerImage(4));
        }
        else if (subStep == 2)
        {
            DrawTitleText("SETUP CAMOS");
            DrawText("Now in the list <b>Camos</b> as the name say you have to assign the camos that this weapon will have available to customize, each field in the list is equal to one camo.\n \nSo in order to add a camo -> add a field to the list and " +
                "fill the data:");

            DrawPropertieInfo("Global ID", "int", "The camo ID from the <b>Global Camo</b> list, if you want to add a new camo, you have to first add the camo in the <b>Global Camos</b> list <i>(check the Add Camo section)</i>.");
            DrawPropertieInfo("Camo", "Material", "The material that will be applied to the model when this camo is used, if you leave empty the default material (from the Global Camo info) will be used.");
            DrawPropertieInfo("Override Preview", "Texture2D", "A small preview of the camo texture to show in the customizer menu, if you leave empty the default texture (from the Global Camo info) will be used.");

            DrawText("Note: the first camo in the list have to be the default camo of the weapon.");
            DrawServerImage("img-6.png");
            DownArrow();
            DrawTitleText("SETUP ATTACHMENTS");
            DrawText("Now in the <b>Attachments</b> fold you have 4 list for 4 different attachments items: <b>Suppressors, Sights, Foregrips and Magazines.</b>\n \n" +
                "in each list like with the camos each field represent an attachment option where the first one is the default one, so in order to add a new attachment simply add a new field in the corresponding list then just fill the three " +
                "variables: Name, ID and Description where ID like in camos has to bee an unique ID per list (can use the same ID in other lists)");
            DrawImage(GetServerImage(6));
        }
        else if (subStep == 3)
        {
            DrawText("Now back to the new weapon under CustomizerManager -> Root -> * -> Select it and in bl_Customizer attach to it change the <b>Customizer ID</b> to the one that you just create for it and click on the button <b>Refresh</b> (right next to it)");
            DrawImage(GetServerImage(7));
            DownArrow();
            DrawText("Now fold out <b>WeaponCamo Render</b>, in it you have two fields: <b>Material ID</b> and <b>Renderer</b>, first in <b>Render</b> You have to drag the <b>Mesh or Skinned Mesh Renderer</b> of the weapon model where the camo material" +
                " will be applied, <b>Material ID</b> is the index of the material in the <b>Materials</b> list of the Mesh Renderer this just in case that the mesh have multiple materials, if it have just one leave it 0");
            DrawImage(GetServerImage(8));
            DrawText("That's all for the setup, now you are ready to add more camos and attachments for this weapon, follow the other sections of this tutorial to learn how to do it and after that, you can transfer the setup to the player prefabs.");
        }
        else if (subStep == 4)
        {


            /*  EditorGUI.BeginChangeCheck();
              DrawText("After you integrate a weapon in the Customizer scene you also have to set up the weapon in the player prefab <i>(in all the player prefabs that you are using).</i>\n\nBut don't worry, we have done this step automatically, you only have to drag the player prefab in the field below.\n\n<b>IMPORTANT:</b> The customizer scene has to be open.");
              Space(10);
              playerPrefab = EditorGUILayout.ObjectField("Player Prefab", playerPrefab, typeof(bl_PlayerNetwork), false) as bl_PlayerNetwork;

              if (EditorGUI.EndChangeCheck())
              {
                  CheckPlayerWeapons();
              }

              if (fpWeaponCheck.Count > 0)
              {
                  Space(20);
                  DrawText("Below are listed the weapons available in the player prefab to setup with Customizer.");
                  Space(20);
                  wScroll = GUILayout.BeginScrollView(wScroll);
                  for (int i = 0; i < fpWeaponCheck.Count; i++)
                  {
                      var weapon = fpWeaponCheck[i];
                      if (weapon.State == PlayerWeaponState.NoAvailable)
                      {
                          GUI.color = new Color(1, 1, 1, 0.3f);
                      }
                      Rect r = EditorGUILayout.BeginHorizontal();
                      EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                      GUILayout.Space(10);
                      GUILayout.Label(weapon.GunInfo.Name, GUILayout.Width(120));
                      if (weapon.State == PlayerWeaponState.New)
                      {
                          if (GUILayout.Button("SETUP",EditorStyles.toolbarButton,GUILayout.Width(100)))
                          {
                              SetupWeapon(weapon);
                          }
                      }
                      else if (weapon.State == PlayerWeaponState.Refresh)
                      {
                          if (GUILayout.Button("UPDATE", EditorStyles.toolbarButton, GUILayout.Width(100)))
                          {

                          }
                      }
                      EditorGUILayout.EndHorizontal();
                      Space(4);
                      GUI.color = Color.white;
                  }
                  GUILayout.EndScrollView();
              }*/
        }
    }

    void SetupPlayerDoc()
    {
        DrawText("After you integrate a weapon in the Customizer scene, you have to transfer the attachments to the player FP and TP weapon, this process will be pretty much automatic and you only have to do a few manual steps:\n\n1. <b>With the Customizer scene open</b> <b><color=#FFFC01FF>(IMPORTANT)</color></b>, Instance the FPWeapon Container, for this go to the top editor navigation menu > <b>MFPS > Actions > Preview FP Weapon</b>, this will instance the FP Weapon container in a place holder player so you can preview the weapons in the Game View.\n\n2. In the instanced weapon container, foldout the <b>FP Weapons</b> in the hierarchy window and select the weapon that you want to transfer the attachments to, in this case following the tutorial you can see that I was integration the \"shotgun\", so I select the FP Shotgun weapon <i>(where the bl_Gun script is attached)</i>\n\n3. In FP Weapon add the script <b>bl_CustomizerWeapon</b>, then set up the variables as follows:");

        DrawPropertieInfo("CustomizerID", "enum", "The ID of the customizer information, this is not the same as the GunID, it's the index of the customizer weapon info in the CustomizerData -> Weapons.");
        DownArrow();
        DrawText("After you assign the correct <b>CustomizerID</b>, click on the '<b>Refresh</b>' button that is next to.\n\n4. Always in the <i>bl_CustomizerWeapon</i> inspector, at the bottom you will see a button called '<b>Import</b>', click on this button and a new box will appear, in this box make sure the '<b>Customizer Weapon</b>' options is the correct one <i>(should be the same as the 'CustomizerID')</i>.\n\nThen in the following fields make sure of:\n\n- <b>Customizer Mesh</b> is assigned, it should automatically be assigned, in case it's empty could be due you have not assigned it in the Customizer weapon (the weapon setup from \"Add Weapon\" section), so you have to assign it and then back here.\n\n- <b>Target Weapon:</b> It should be assigned automatically, if it's empty it means that the FPWeapon model is not the same as the weapon model that you integrate into the Customizer scene, so what you can do here is drag the mesh where you wanna apply the camos in the FPWeapon, simply drag the object <i>(with the Mesh Renderer)</i> in the field.\n\n5. Once all fields are assigned, click on the button '<b>Transfer</b>'.\n\nIf all works correctly you should see a message in the console letting you know it, otherwise you will see errors or warnings in the console as well.\n\nSo if everything is ok, apply the changes to the weapon container by selecting the FP Weapons object in the hierarchy > bl_ViewWeaponContainer (inspector) > click on 'Commit Changes', then delete the 'Player Placeholder' object from the hierarchy and that's.");

        DrawNote("The following clip was recorded in an older version of MFPS where the FPWeapons where include inside the player prefabs instead of an separate weapon container, beside dragging the player manually, the rest of the steps in the clip are still the same, so instead of dragging the player do what is mentioned above.");
        DrawAnimatedImage(0);
    }

    void DrawAddAttachments()
    {
        if (subStep == 0)
        {
            DrawText("• So now is time to setup the attachments models, in the previous steps you setup the info but now you gonna position the models in the weapons prefabs,\nFirst you have to setup the weapon in the Customizer scene," +
                "So select it in <i>CustomizerManager -> Root -> *YOUR WEAPON* -> Attachments</i>, there you have four empty objects in which you gonna put your attachments models depending of the type, for example I will add a suppressor:");
            DrawImage(GetServerImage(10));
            DownArrow();
            DrawText("Then positioned the model correctly with respect to the weapon model");
            DrawImage(GetServerImage(11));
            DownArrow();
            DrawText("Select the root of weapon object -> bl_Customizer -> Attachments -> find the attachment info in the lists and in the model field drag your attachment model <i>(the one that just positioned before)</i>.");
            DrawImage(GetServerImage(12));
            DrawText("Good, you have added an attachment to the customizer weapon, still missing setup in the TP and FP Weapon tho :/,\nYou have to repeat this for each other attachments, is highly recommended that you setup all the attachments for " +
                "this weapon in the customizer weapon before process with the FP and TP Weapon (next step).");
        }
        else if (subStep == 1)
        {
            DrawTitleText("Player Weapons");
            DrawText("Now we have to setup the attachment to the <b>First Person Weapon</b> in the Player prefabs for the Local Player perspective, so first drag in the <b>Customizer</b> scene hierarchy the player prefab that have the weapon <i>(or if more than one player have the weapon " +
                "you'll have to setup for it too to export the weapon)</i>, now fold out the player prefab until get in the FPWeapon where <b>bl_Gun.cs</b> is attached, if this is the first attachment for this weapon you have to add the script " +
                "<b>bl_CustomizerWeapon.cs</b> -> Set the <b>Customizer ID</b> and Mark as True the toggle <b>is First Person Weapon</b>");
            DrawImage(GetServerImage(13));
            DownArrow();
            DrawText("Now we have to setup the attachments in the FP and TP Weapon :/, but don't worry you don't have to do it manually like you do with the Customizer weapon, now you only will transfer these models from the Customizer weapon" +
                " automatically, for it in the bl_CustomizerWeapon inspector, at the bottom you will see a few buttons <b>Import</b> and <b>Update Attachments</b> if this is the first time adding an attachment to this weapon click on the <b>Import</b> " +
                "button.\n \nif everything work correctly you should see three new fields and a button called <b>Transfer</b>, the first field is the Customizer weapon from where we gonna transfer the attachments, the second is the mesh of the Customizer model " +
                "the last one is the mesh of the FPWeapon model, so if the three of them are assigned correctly the button <b>Transfer</b> will be enable, so click on it -> if everything work as expected you should see this message in console: <b>All done!</b>, " +
                "that mean that the attachments model has been transfer from Customizer weapon to the FP and TP Weapon correctly!.\n \n• Now if this is not the first time adding an attachment for this weapon which mean you have already done the last step before (Transfer)" +
                "Now you only have to Update the attachments, for example when you move, rotate or scale an existing attachment or add new attachments, what you have to do is simply instead of click in the <b>Import</b> Button click on <b>Update Attachments</b>");
            DrawImage(GetServerImage(14));
        }
    }

    void DrawCamos()
    {
        DrawText("In order to add a new camo for one or more weapons you need:\n-The camo material: the Material that will replace the default weapon material, of course this material have to be setup already with the custom texture, shader, etc...\n-A sprite preview of your camo.");
        DownArrow();
        DrawText("Ok, so if this is the first time that you add this camo to a weapon, you have to set up it in the <b>\"GlobalCamos\"</b> list first. <i>if you have already added this camos in the" +
            " GlobalCamos list, skip this and continue bellow</i>,\nalright for add the camo for first time go to <b>CustomizerData</b> <i>Assets -> Addons -> Customizer -> Resources -> CustomizerData</i> add a new field in the list <b>GlobalCamos</b>, in this field fill the variables respectively:\n•Name: The name of the camo.\n•Preview: a Sprite texture that show the camo pattern.\n•Price: How much does this camo cost? 0 = Free, note that this is only used if you are using Shop addon.");
        DownArrow();
        DrawText("So now that we have the GlobalCamo we need to tell in which weapons this camo are available and set the custom material for it, so in the <b>Weapons</b> list (always in <b>CustomizerData</b>) fold out the weapon in which you wanna add the camo -> Open the <b>WeaponCamo</b> list -> add a new field -> in the new field select the popup <b>GlobalID</b> and select the name of the camo that you wanna add.\nThen in the <b>WeaponCamo</b> variable you have to set the Material that contain the camo texture for that weapon.\nIn <b>OverridePreview</b> you can leave it empty if you wanna use the default preview (the one from GlobalCamo info) or set one texture to override it for this weapon.");
        DrawServerImage(15, TextAlignment.Center);
        DrawText("That's!");
    }

    void MofidiersDoc()
    {
        DrawText("There's another feature that you may require with the attachments, you may find that after adding some attachments like a Scope for example when you use that attachment in the weapon <i>(FPWeapon)</i> and you aim with that weapon since the default Aim position was not set up with that extra model in mind the weapon will not be line up with the scope, causing players can't aim properly.");
        DrawServerImage(16);
        DrawText("That not just apply for scopes/sights, but also for example if you want to use a different fire sound with a certain attachment barrel is used like a silencer, if you may want to use a different fire sound, luckely Customizer comes with an easy solution for this cases.\n\nAll what you have to do is add the <b>bl_AttachmentGunModifier</b>.cs  script to the attachment object in the FPWeapon; for example if you wanna a different aim position and/or extra aim zoom for a specific sight/scope, add that script to that scope/sight object and setup the properties according.");
        DrawServerImage(17);
        DownArrow();
        DrawText("The same steps apply for all other attachments, for example for add more or less bullets depend of the magazine attachment selected, simply add the script to the magazine object and check the <b>Add Bullets</b> toggle and set how many extra bullets add or remove <i>(by set a negative value)</i>");
    }

    void MultiCamoRenderDoc()
    {
        DrawText("There could be the case in which your weapon model has multiple sub-meshes that make up the whole weapon model, for those weapons, you will run into the problem that you can only assign a mesh for the camo render <i>(following the <b>Add Weapon</b> guide)</i>, but what if you need to assign more render meshes of the same weapon?,\nWell, here is the solution:\n\nIn the Renderer that you assign in the <b>Camo Render ➔ Render</b>, attach the script <b>bl_MultiCamoRenders.cs</b> to it ➔ then, in the inspector of that script, in the list Renders ➔ add as many fields as renders needed, and in each field assign the different weapon sub-meshes of your weapon that you need to apply the camo.\n\nMake sure you do this for all the weapons where the camo is applied:\n\n- The weapons in the customizer scene (bl_Customizer ➔ Camo Render ➔ Render)\n- The FP Weapons in the player prefabs (bl_CustomizerWeapon ➔ Camo Render ➔ Render)\n- The TP Weapons in the player prefabs (bl_CustomizerWeapon ➔ Camo Render ➔ Render)");
        DrawServerImage("img-19.png");
    }

    void AttachmentsModelsDoc()
    {
        DrawText("Here you will find a lists of hand-pick attachments model assets available in the Asset Store that work for action-shooter games in general that you may find useful in case you are looking for some.");

        if (affAssets == null)
        {
            affAssets = new AssetStoreAffiliate();
            affAssets.randomize = true;
            affAssets.Initialize(this, "https://assetstore.unity.com/linkmaker/embed/list/4673555541691/widget-medium");
            affAssets.FixedHeight = 0;
            affAssets.selfScroll = false;
            affAssets.onSwitchPage = () => { SetContentScrollPosition(Vector2.zero); };
        }
        else
            affAssets.OnGUI();
    }

    [MenuItem("MFPS/Addons/Customizer/Tutorial")]
    private static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(CustomizerTutorial));
    }

    [MenuItem("MFPS/Tutorials/Customizer")]
    private static void ShowWindowMFPS()
    {
        EditorWindow.GetWindow(typeof(CustomizerTutorial));
    }
}