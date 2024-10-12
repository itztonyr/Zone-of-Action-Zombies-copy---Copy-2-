using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Addon.Customizer
{
    public class bl_AttachmentGunModifier : MonoBehaviour
    {
        #region Public members
        [Header("General")]
        public float extraWeight = 0;
        public int extraDamage = 0;
        [Header("Sight")]
        public bool OverrideAimPosition = false;
        public bool disableScope = false;
        public Vector3 AimPosition;
        public int extraZoom = 0;
        public float extraAimSpeed = 0;
        [Header("Barrel")]
        public bool OverrideFireSound = false;
        public AudioClip FireSound;
        public float extraSpread = 0;
        public float extraBulletSpeed = 0;
        public float extraRecoil = 0;
        public int extraRange = 0;
        [Header("Magazine")]
        public int ExtraBullets = 0;
        public float extraReloadTime = 0;

        [FormerlySerializedAs("m_Gun")]
        public bl_Gun targetWeapon;
        [HideInInspector] public Vector3 _defaultPosition;
        [HideInInspector] public Vector3 _defaultRotation;
        [HideInInspector] public bool _aimRecord = false;
        #endregion

        /// <summary>
        /// 
        /// </summary>
        void OnEnable()
        {
            if (targetWeapon == null)
            {
                targetWeapon = GetComponentInParent<bl_Gun>();
            }

            ApplyModifiers();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            SetDefaults();
        }

        /// <summary>
        /// 
        /// </summary>
        public void ApplyModifiers()
        {
            if (targetWeapon == null)
            {
                targetWeapon = GetComponentInParent<bl_Gun>();
            }

            if (targetWeapon == null) return;

            if (OverrideAimPosition)
            {
                targetWeapon.AimPosition = AimPosition;
                if (targetWeapon.GetComponent<bl_SniperScopeBase>() != null)
                {
                    targetWeapon.GetComponent<bl_SniperScopeBase>().enabled = !disableScope;
                }
            }

            if (OverrideFireSound) targetWeapon.FireAudioClip = FireSound;
            targetWeapon.BulletsPerMagazine = ExtraBullets;
            targetWeapon.Zoom = extraZoom;
            targetWeapon.Damage = extraDamage; // modify only the extra damage, that means if you set 0 of extra damage, only the gun base/default damage is gonna be applied.
            targetWeapon.MaxSpread = extraSpread;
            targetWeapon.WeaponWeight = extraWeight;
            targetWeapon.WeaponRecoil = extraRecoil;
            targetWeapon.AimSpeed = extraAimSpeed;
            targetWeapon.ReloadTime = extraReloadTime;
            targetWeapon.BulletSpeed = extraBulletSpeed;
            targetWeapon.Range = extraRange;
        }

        /// <summary>
        /// 
        /// </summary>
        public void SetDefaults()
        {
            if (targetWeapon == null) return;
            // in some cases only the weapon childs are disabled, do not reset the modifiers if that is the case.
            if (targetWeapon.gameObject.activeSelf) return;

#if MFPSTPV
            if (bl_CameraViewSettings.IsThirdPerson() && bl_MFPS.LocalPlayerReferences.gunManager.GetCurrentWeapon() == targetWeapon)
            {
                return;
            }
#endif

            if (OverrideFireSound)
            {
                targetWeapon.FireAudioClip = null;
            }
            targetWeapon.BulletsPerMagazine = 0;
            targetWeapon.Zoom = 0;
            targetWeapon.Damage = 0;
            targetWeapon.MaxSpread = 0;
            targetWeapon.WeaponWeight = 0;
            targetWeapon.WeaponRecoil = 0;
            targetWeapon.AimSpeed = 0;
            targetWeapon.ReloadTime = 0;
            targetWeapon.BulletSpeed = 0;
            targetWeapon.Range = 0;
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_AttachmentGunModifier))]
    public class bl_AttachmentGunModifierEditor : Editor
    {
        bl_AttachmentGunModifier script;
        bl_PlayerPlaceholder playerPlaceholder;

        private void OnEnable()
        {
            script = (bl_AttachmentGunModifier)target;
            if (script.targetWeapon == null)
            {
                script.targetWeapon = script.transform.GetComponentInParent<bl_Gun>();
                if (script.targetWeapon != null) EditorUtility.SetDirty(target);
            }
            playerPlaceholder = script.transform.GetComponentInParent<bl_PlayerPlaceholder>();
        }

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();

            GUILayout.BeginVertical("box");
            if (script.targetWeapon == null)
            {
                EditorGUILayout.HelpBox("You can leave this field empty and it will be assigned in runtime.", MessageType.Info);
            }
            script.targetWeapon = EditorGUILayout.ObjectField("Gun", script.targetWeapon, typeof(bl_Gun), true) as bl_Gun;
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            script.OverrideAimPosition = EditorGUILayout.ToggleLeft("Override Aim Position", script.OverrideAimPosition, EditorStyles.toolbarButton);
            if (script.OverrideAimPosition)
            {
                GUILayout.Space(2);
                script.AimPosition = EditorGUILayout.Vector3Field("Aim Position", script.AimPosition);
                if (script.targetWeapon != null && script.targetWeapon.GetComponent<bl_SniperScopeBase>() != null)
                {
                    script.disableScope = EditorGUILayout.ToggleLeft("Disable Sniper Scope", script.disableScope, EditorStyles.toolbarButton);
                }
            }
            script.extraZoom = EditorGUILayout.IntSlider("Extra Aim Zoom", script.extraZoom, -50, 50);
            script.extraAimSpeed = EditorGUILayout.FloatField("Extra Aim Speed", script.extraAimSpeed);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            script.OverrideFireSound = EditorGUILayout.ToggleLeft("Override Fire Sound", script.OverrideFireSound, EditorStyles.toolbarButton);
            if (script.OverrideFireSound)
            {
                GUILayout.Space(2);
                script.FireSound = EditorGUILayout.ObjectField("Fire Sound", script.FireSound, typeof(AudioClip), false) as AudioClip;
            }
            script.extraDamage = EditorGUILayout.IntField("Extra Damage", script.extraDamage);
            script.extraSpread = EditorGUILayout.FloatField("Extra Spread", script.extraSpread);
            script.extraRecoil = EditorGUILayout.FloatField("Extra Recoil", script.extraRecoil);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            script.ExtraBullets = EditorGUILayout.IntField("Extra Bullets", script.ExtraBullets);
            script.extraReloadTime = EditorGUILayout.FloatField("Extra Reload Time", script.extraReloadTime);
            GUILayout.EndVertical();

            GUILayout.BeginVertical("box");
            script.extraWeight = EditorGUILayout.Slider("Extra Weight", script.extraWeight, -3, 4);
            script.extraBulletSpeed = EditorGUILayout.FloatField("Extra Bullet Speed", script.extraBulletSpeed);
            GUILayout.EndVertical();

            if (script.OverrideAimPosition && script.targetWeapon != null)
            {
                string bt = script._aimRecord ? "Save Aim Position" : "Edit Aim Position";
                if (GUILayout.Button(bt))
                {
                    var weaponTransform = script.targetWeapon.transform;
                    if (!script._aimRecord)
                    {
                        script._defaultPosition = weaponTransform.localPosition;
                        script._defaultRotation = weaponTransform.localEulerAngles;
                        if (script.AimPosition == Vector3.zero)
                        {
                            script.AimPosition = script.targetWeapon.AimPosition;
                        }
                        weaponTransform.localPosition = script.AimPosition;
                        weaponTransform.localEulerAngles = script.targetWeapon.aimRotation;
                        script._aimRecord = true;
                        ActiveEditorTracker.sharedTracker.isLocked = true;
                        Selection.activeTransform = weaponTransform;
                    }
                    else
                    {
                        script.AimPosition = weaponTransform.localPosition;
                        weaponTransform.localPosition = script._defaultPosition;
                        weaponTransform.localEulerAngles = script._defaultRotation;
                        script._aimRecord = false;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(script);
                        Selection.activeTransform = script.transform;
                        ActiveEditorTracker.sharedTracker.isLocked = false;
                    }
                    if (playerPlaceholder != null) playerPlaceholder.CalibratingAim = script._aimRecord;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }
    }
#endif
}