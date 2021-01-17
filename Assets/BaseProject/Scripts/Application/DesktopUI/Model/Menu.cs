using System;
using System.Collections.Generic;
using UnityEngine;

namespace CreativeMode
{
    public class Menu
    {
        protected bool Equals(Menu other)
        {
            return Equals(entries, other.entries);
        }

        private readonly Entry[] entries;

        public Entry this[int index] => entries[index];
        public int Size => entries.Length;

        private Menu(Entry[] entries)
        {
            this.entries = entries;
        }

        public static Menu Merge(params Menu[] menuArray)
        {
            var entries = new List<Entry>();
            var groupOffset = 0;
            
            foreach (var menu in menuArray)
            {
                var lastGroup = 0;
                
                for (var e = 0; e < menu.Size; e++)
                {
                    var entry = menu[e];
                    lastGroup = entry.group;

                    switch (entry)
                    {
                        case Button b: entries.Add(new Button(
                                b.title, b.icon, b.isVisible, b.isEnabled, b.group + groupOffset,
                                b.onClick, b.closeOnClick));
                            break;
                        
                        case Toggle t: entries.Add(new Toggle(
                                t.title, t.icon, t.isVisible, t.isEnabled, t.group + groupOffset,
                                t.isChecked, t.onCheckedChanged, t.closeOnChange));
                            break;
                        
                        case SubMenu s: entries.Add(new SubMenu(
                                s.title, s.icon, s.isVisible, s.isEnabled, s.group + groupOffset,
                                s.subMenu, s.closeParentOnSelect));
                            break;
                    }
                    
                }

                groupOffset += lastGroup + 1;
            }
            
            return new Menu(entries.ToArray());
        }

        public abstract class Entry
        {
            public readonly string title;
            public readonly Sprite icon;
            public readonly bool isVisible;
            public readonly bool isEnabled;
            public readonly int group;
    
            public Entry(string title, Sprite icon, bool isVisible, bool isEnabled, int group)
            {
                this.title = title;
                this.icon = icon;
                this.isVisible = isVisible;
                this.isEnabled = isEnabled;
                this.group = group;
            }
        }
    
        public class Button : Entry
        {
            public readonly Action onClick;
            public readonly bool closeOnClick;
    
            public Button(string title, Sprite icon, bool isVisible, bool isEnabled, int group,
                Action onClick, bool closeOnClick) 
                : base(title, icon, isVisible, isEnabled, group)
            {
                this.onClick = onClick;
                this.closeOnClick = closeOnClick;
            }
        }
    
        public class Toggle : Entry
        {
            public readonly bool isChecked;
            public readonly Action<bool> onCheckedChanged;
            public readonly bool closeOnChange;
    
            public Toggle(string title, Sprite icon, bool isVisible, bool isEnabled, int group,
                bool isChecked, Action<bool> onCheckedChanged, bool closeOnChange) 
                : base(title, icon, isVisible, isEnabled, group)
            {
                this.isChecked = isChecked;
                this.onCheckedChanged = onCheckedChanged;
                this.closeOnChange = closeOnChange;
            }
        }
    
        public class SubMenu : Entry
        {
            public readonly Menu subMenu;
            public readonly bool closeParentOnSelect;
    
            public SubMenu(string title, Sprite icon, bool isVisible, bool isEnabled, int group,
                Menu subMenu, bool closeParentOnSelect) 
                : base(title, icon, isVisible, isEnabled, group)
            {
                this.subMenu = subMenu;
                this.closeParentOnSelect = closeParentOnSelect;
            }
        }

        public class Builder
        {
            private List<Entry> entries = new List<Entry>();
            private int group;

            public Builder Button(
                string title, 
                Action onClick,
                Sprite icon = null,
                bool closeOnClick = true,
                bool isVisible = true,
                bool isEnabled = true)
            {
                entries.Add(new Button(title, icon, isVisible, isEnabled, group,
                    onClick, closeOnClick));
                return this;
            }
    
            public Builder Toggle(
                string title,
                bool isChecked,
                Action<bool> onCheckedChanged,
                Sprite icon = null,
                bool closeOnChange = false,
                bool isVisible = true,
                bool isEnabled = true)
            {
                entries.Add(new Toggle(title, icon, isVisible, isEnabled, group, 
                    isChecked, onCheckedChanged, closeOnChange));
                return this;
            }
    
            public Builder SubMenu(
                string title,
                Menu subMenu,
                Sprite icon = null,
                bool closeParentOnSelect = true,
                bool isVisible = true,
                bool isEnabled = true)
            {
                entries.Add(new SubMenu(title, icon, isVisible, isEnabled, group, 
                    subMenu, closeParentOnSelect));
                return this;
            }
            
            public Builder SubMenu(
                string title,
                Builder subMenu,
                Sprite icon = null,
                bool closeParentOnSelect = true,
                bool isVisible = true,
                bool isEnabled = true)
            {
                return SubMenu(title, subMenu.Build(), icon, closeParentOnSelect, isVisible, isEnabled);
            }

            public Builder Divider()
            {
                group++;
                return this;
            }
            
            public Menu Build()
            {
                return new Menu(entries.ToArray());
            }
        }
    }
}