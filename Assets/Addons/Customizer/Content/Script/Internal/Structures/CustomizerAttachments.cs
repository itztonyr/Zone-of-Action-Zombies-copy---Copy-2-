using System;
using System.Collections.Generic;
using UnityEngine;

namespace MFPS.Addon.Customizer
{

    [Serializable]
    public class CustomizerModelInfo
    {
        public string Name = "";
        public int ID = 0;
        public GameObject Model;

        [HideInInspector] public AttachInfo Info;

        public void SetInfo(AttachInfo _info)
        {
            Name = _info.Name;
            ID = _info.ID;
            Info = _info;
        }
    }

    [Serializable]
    public class CustomizerAttachments
    {
        public List<CustomizerModelInfo> Suppressers = new List<CustomizerModelInfo>();
        public List<CustomizerModelInfo> Sights = new List<CustomizerModelInfo>();
        public List<CustomizerModelInfo> Foregrips = new List<CustomizerModelInfo>();
        public List<CustomizerModelInfo> Magazines = new List<CustomizerModelInfo>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="array"></param>
        public void Apply(int[] array)
        {
            Suppressers.ForEach(x => { SetActive(x.Model, false); });
            Sights.ForEach(x => { SetActive(x.Model, false); });
            Foregrips.ForEach(x => { SetActive(x.Model, false); });
            Magazines.ForEach(x => { SetActive(x.Model, false); });

            ActiveModelInList(Suppressers, array[0]);
            ActiveModelInList(Sights, array[1]);
            ActiveModelInList(Foregrips, array[2]);
            ActiveModelInList(Magazines, array[3]);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="id"></param>
        /// <param name="active"></param>
        void ActiveModelInList(List<CustomizerModelInfo> list, int id, bool active = true)
        {
            if (list == null || id >= list.Count || list[id].Model == null) return;

            list[id].Model.SetActive(active);

            if (active)
            {
                var modifier = list[id].Model.GetComponentInChildren<bl_AttachmentGunModifier>();
                if (modifier != null) modifier.ApplyModifiers();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="active"></param>
        void SetActive(GameObject obj, bool active)
        {
            if (obj == null) return;
            obj.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public bool CheckIfIsAttachment(GameObject model)
        {
            var check = CompareInList(Suppressers, model);
            if (check) return true;
            check = CompareInList(Sights, model);
            if (check) return true;
            check = CompareInList(Foregrips, model);
            if (check) return true;
            check = CompareInList(Magazines, model);
            return check;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        bool CompareInList(List<CustomizerModelInfo> list, GameObject model)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Model == null) continue;
                if (list[i].Model == model)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<CustomizerModelInfo> GetAllAttachments(bool skipEmptyModels = false)
        {
            var all = new List<CustomizerModelInfo>();
            if (!skipEmptyModels)
            {
                all.AddRange(Suppressers);
                all.AddRange(Sights);
                all.AddRange(Foregrips);
                all.AddRange(Magazines);
            }
            else
            {
                all.AddRange(Suppressers.FindAll(x => x.Model != null));
                all.AddRange(Sights.FindAll(x => x.Model != null));
                all.AddRange(Foregrips.FindAll(x => x.Model != null));
                all.AddRange(Magazines.FindAll(x => x.Model != null));
            }
            return all;
        }
    }
}