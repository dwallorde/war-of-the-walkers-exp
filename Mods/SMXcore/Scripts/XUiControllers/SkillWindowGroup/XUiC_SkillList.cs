using Quartz;
using SMXcore.HarmonyPatches;
using System;
using System.Collections.Generic;

//	Terms of Use: You can use this file as you want as long as this line and the credit lines are not removed or changed other than adding to them!
//	Credits: The Fun Pimps.
//	Tweaked: Laydor

//	Changes the SkillList XUiController is only track Attribute and Perk skills

namespace SMXcore
{
    public class XUiC_SkillList : XUiController
    {
        private List<ProgressionValue> skills = new List<ProgressionValue>();
        private List<ProgressionValue> currentAttributeSkills = new List<ProgressionValue>();
        private List<ProgressionValue> currentPerkSkills = new List<ProgressionValue>();

        private XUiC_PerkSkillEntry[] perkEntries;
        private XUiC_AttributeSkillEntry[] attributeEntries;

        private string selectName;

        private XUiC_SkillEntry selectedEntry;

        private XUiC_TextInput txtInput;


        public XUiC_SkillEntry SelectedEntry
        {
            get
            {
                return selectedEntry;
            }
            set
            {
                if (selectedEntry != null)
                {
                    selectedEntry.IsSelected = false;
                }

                selectedEntry = value;

                if (selectedEntry != null)
                {
                    selectedEntry.IsSelected = true;
                    xui.selectedSkill = selectedEntry.Skill;
                }
            }
        }

        public XUiC_SkillListWindow SkillListWindow { get; set; }

        public override void Init()
        {
            base.Init();

            attributeEntries = GetChildrenByType<XUiC_AttributeSkillEntry>();
            foreach(XUiC_SkillEntry entry in attributeEntries)
            {
                entry.OnPress += XUiC_SkillEntry_OnPress;
                entry.DisplayType = ProgressionClass.DisplayTypes.Standard;
            }

            perkEntries = GetChildrenByType<XUiC_PerkSkillEntry>();
            foreach (XUiC_SkillEntry entry in perkEntries)
            {
                entry.OnPress += XUiC_SkillEntry_OnPress;
                entry.DisplayType = ProgressionClass.DisplayTypes.Standard;
            }

        }

        public void SelectFirstEntry()
        {
            SelectedEntry = attributeEntries[0];
        }

        private void XUiC_SkillEntry_OnPress(XUiController sender, int _mouseButton)
        {
            XUiC_SkillEntry skillEntry = sender as XUiC_SkillEntry;
            if (skillEntry.Skill != null)
            {
                SelectedEntry = skillEntry;
                selectName = "";
            }
        }

        internal int GetActiveCount()
        {
            return currentAttributeSkills.Count + currentPerkSkills.Count;
        }

        public void RefreshSkillList()
        {
            UpdateSkillLists();
            RefreshSkillListEntries();
        }

        private void UpdateSkillLists()
        {
            currentAttributeSkills.Clear();
            currentPerkSkills.Clear();

            foreach (ProgressionValue skill in skills)
            {
                ProgressionClass progressionClass = skill?.ProgressionClass;
                if (progressionClass == null || progressionClass.Name == null || !progressionClass.ValidDisplay(ProgressionClass.DisplayTypes.Standard))
                {
                    continue;
                }


                if (progressionClass.IsPerk)
                {
                    currentPerkSkills.Add(skill);
                    continue;
                }

                if (progressionClass.IsAttribute && !progressionClass.Name.Equals("attbooks") && !progressionClass.Name.Equals("attcrafting"))
                {
                    currentAttributeSkills.Add(skill);
                }
            }

            currentAttributeSkills.Sort(ProgressionClass.ListSortOrderComparer.Instance);
            currentPerkSkills.Sort(ProgressionClass.ListSortOrderComparer.Instance);
        }

        private void RefreshSkillListEntries()
        {
            XUiView attributeInfoViewComponent = ((XUiC_SkillWindowGroup)WindowGroup.Controller).skillAttributeInfoWindow.GetChildById("0").ViewComponent;
            XUiView perkInfoViewComponent = ((XUiC_SkillWindowGroup)WindowGroup.Controller).skillPerkInfoWindow.GetChildById("0").ViewComponent;

            SelectedEntry = null;
            PopulateSkillEntry(attributeEntries, currentAttributeSkills, attributeInfoViewComponent);
            PopulateSkillEntry(perkEntries, currentPerkSkills, perkInfoViewComponent);

            if (SelectedEntry == null)
            {
                SelectedEntry = attributeEntries[0];
                SelectedEntry.RefreshBindings();
                ((XUiC_SkillWindowGroup)WindowGroup.Controller).CurrentSkill = SelectedEntry.Skill;
            }

            //if (xui.selectedSkill == null)
            //{
            //    if (selectedEntry != null)
            //    {
            //        ((XUiC_SkillWindowGroup)WindowGroup.Controller).CurrentSkill = selectedEntry.Skill;
            //        xui.selectedSkill = selectedEntry.Skill;
            //    }
            //}

            RefreshBindings();
            SkillListWindow.RefreshBindings();
        }

        private void PopulateSkillEntry(XUiC_SkillEntry[] entries, List<ProgressionValue> progressionValues, XUiView navRightTarget)
        {
            for (int i = 0; i < entries.Length; i++)
            {
                XUiC_SkillEntry entry = entries[i];
                if (i < progressionValues.Count && progressionValues[i] != null && Progression.ProgressionClasses.ContainsKey(progressionValues[i].Name))
                {
                    ProgressionValue skill = progressionValues[i];
                    entry.Skill = skill;

                    if (!string.IsNullOrEmpty(selectName) && skill.ProgressionClass.Name.Equals(selectName))
                    {
                        SelectedEntry = entry;
                        ((XUiC_SkillWindowGroup)WindowGroup.Controller).CurrentSkill = SelectedEntry.Skill;
                    } 
                    else
                    {
                        entry.IsSelected = false;
                    }
                    entry.ViewComponent.Enabled = true;
                    entry.ViewComponent.NavRightTarget = navRightTarget;
                    entry.RefreshBindings();

                }
                else
                {
                    entry.Skill = null;
                    entry.IsSelected = false;
                    entry.ViewComponent.Enabled = false;
                    entry.RefreshBindings();

                }
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            skills.Clear();
            xui.playerUI.entityPlayer.Progression.GetDict().CopyValuesTo(skills);
            RefreshSkillList();
        }

        public override void OnClose()
        {
            base.OnClose();
            selectName = "";
        }

        public XUiC_SkillEntry GetEntryForSkill(ProgressionValue skill)
        {
            XUiC_SkillEntry[] skillEntries = new XUiC_SkillEntry[0];
            if(skill.ProgressionClass.IsAttribute)
            {
                skillEntries = attributeEntries;
            } 
            else if (skill.ProgressionClass.IsPerk)
            {
                skillEntries = perkEntries;
            }

            foreach (XUiC_SkillEntry xuiC_SkillEntry in skillEntries)
            {
                if (xuiC_SkillEntry.Skill == skill)
                {
                    return xuiC_SkillEntry;
                }
            }
            return null;
        }
    }
}
