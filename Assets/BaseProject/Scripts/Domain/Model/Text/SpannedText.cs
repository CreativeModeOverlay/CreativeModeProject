﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CreativeMode
{
    public class SpannedText
    {
        public string Text { get; }

        public int TagCount => tags.Length;
        
        private readonly TextTag[] tags;

        public SpannedText(string text, List<TextTag> tagsList = null)
        {
            Text = text;

            if (tagsList != null)
            {
                tags = tagsList.ToArray();
                Array.Sort(tags, (l, r) => l.textStartIndex.CompareTo(r.textStartIndex));
            }
            else
            {
                tags = new TextTag[0];
            }
        }

        public TextTag GetTag(int index)
        {
            return tags[index];
        }

        public override string ToString()
        {
            return Text;
        }

        public static SpannedText Join(string separator, params SpannedText[] items)
        {
            var builder = new StringBuilder();
            var spans = new List<TextTag>();
            var addSeparator = false;

            foreach (var item in items)
            {
                if(item.IsBlank())
                    continue;
                
                if (addSeparator)
                    builder.Append(separator);
                
                if (item.tags != null)
                {
                    foreach (var span in item.tags)
                    {
                        spans.Add(new TextTag
                        {
                            textStartIndex = builder.Length + span.textStartIndex,
                            isClosing = span.isClosing,
                            tag = span.tag
                        });
                    }
                }

                builder.Append(item.Text);
                addSeparator = !string.IsNullOrWhiteSpace(item.Text);
            }
            
            return new SpannedText(builder.ToString(), spans);
        }
    }
}