using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace MFPS.ULogin
{
    /// <summary>
    /// ULogin Account role data
    /// </summary>
    [Serializable]
    public class ULoginAccountRole
    {
        [Tooltip("The role nice name that will be displayed to the user.")]
        public string RoleName;
        [Tooltip("The role key that will be used in code to identify this role, this should not be changed.")]
        public string RoleKey;
        public Color RoleColor;
        [Tooltip("Insert the role name whenever the player nickname is shown?")]
        public bool InsertRoleInNickName = false;
        public bool HasModerationRights = false;

        /// <summary>
        /// Get the role prefix text
        /// </summary>
        /// <returns></returns>
        public string GetRolePrefix()
        {
            if (!InsertRoleInNickName) return "";

            return $"<color=#{ColorUtility.ToHtmlStringRGBA(RoleColor)}>({RoleName})</color>";
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="roleKey"></param>
        /// <returns></returns>
        public static ULoginAccountRoleRef GetRoleRef(string roleKey)
        {
            int roleId = bl_LoginProDataBase.Instance.roles.FindIndex(x => x.RoleKey.ToLower() == roleKey.ToLower());
            return roleId;
        }
    }

    [Serializable]
    public struct ULoginAccountRoleRef
    {

        private int _index;
        private ULoginAccountRole _role;

        /// <summary>
		/// Role index
		/// </summary>
		public int RoleId
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Role Data
        /// </summary>
        public ULoginAccountRole Role
        {
            get
            {
                if (_role == null) _role = bl_LoginProDataBase.GetRole(this);
                return _role;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj is ULoginAccountRoleRef)
            {
                ULoginAccountRoleRef playerRef = (ULoginAccountRoleRef)obj;
                return _index == playerRef._index;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return _index;
        }

        public override string ToString()
        {
            return Role == null ? "None" : $"{Role.RoleName}";
        }

        public static implicit operator ULoginAccountRoleRef(int value)
        {
            ULoginAccountRoleRef result = default(ULoginAccountRoleRef);
            result._index = value + 1;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(ULoginAccountRoleRef value)
        {
            return value._index > 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(ULoginAccountRoleRef value)
        {
            return value._index - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ULoginAccountRoleRef a, ULoginAccountRoleRef b)
        {
            return a._index == b._index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ULoginAccountRoleRef a, ULoginAccountRoleRef b)
        {
            return a._index != b._index;
        }
    }
}