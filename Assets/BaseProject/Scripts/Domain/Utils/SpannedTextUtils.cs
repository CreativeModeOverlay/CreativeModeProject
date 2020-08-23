using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

namespace CreativeMode
{
    public static class SpannedTextUtils
    {
        private static readonly Regex htmlTagRegex = new Regex(@"(\<\w+?\>)|(\<\w+?\=\S+?\>)|(\<\/\w+?\>)");
        
        public static bool IsBlank(this SpannedText text)
        {
            if (text.TagCount == 0)
                return string.IsNullOrWhiteSpace(text.Text);
            
            var textPosition = 0;

            for (var i = 0; i <= text.TagCount; i++)
            {
                int startIndex;
                int endIndex;
                
                if (i == text.TagCount)
                {
                    startIndex = text.Text.Length;
                    endIndex = startIndex;
                }
                else
                {
                    var span = text.GetTag(i);
                    startIndex = span.textStartIndex;
                    endIndex = span.textEndIndex;
                }

                for (int c = textPosition; c < startIndex; c++)
                {
                    if (!char.IsWhiteSpace(text.Text[c]))
                        return false;
                }

                textPosition = endIndex;
            }

            return true;
        }
        
        public static bool IsIconsOnly(this SpannedText text)
        {
            return IsBlank(text) && HasTag<IconTag>(text);
        }

        public static bool HasTag<T>(this SpannedText text)
        {
            for (var i = 0; i < text.TagCount; i++)
            {
                var tag = text.GetTag(i);

                if (tag.tag is T)
                    return true;
            }

            return false;
        }

        public static int GetTagCount<T>(this SpannedText text)
        {
            var count = 0;
            
            for (var i = 0; i < text.TagCount; i++)
            {
                var tag = text.GetTag(i);

                if (tag.tag is T)
                    count++;
            }

            return count;
        }
        
        public static void Enumerate(SpannedText text, Action<string> onText, Action<object, bool> onTag)
        {
            if (text.TagCount == 0)
            {
                onText(text.Text);
                return;
            }

            var textPosition = 0;

            for (var i = 0; i < text.TagCount; i++)
            {
                var span = text.GetTag(i);
                
                if (textPosition != span.textStartIndex)
                    onText(text.Text.Substring(textPosition, span.textStartIndex - textPosition));

                onTag(span.tag, span.isClosing);
                textPosition = span.textEndIndex;
            }

            if(textPosition != text.Text.Length)
                onText(text.Text.Substring(textPosition));
        }
        
        public static SizeTag ParseSizeSpan(string value)
        {
            return int.TryParse(value, out var size) 
                ? new SizeTag { size = size } 
                : null;
        }

        public static SizeScaleTag ParseSizeScaleSpan(string value)
        {
            return float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var scale) 
                ? new SizeScaleTag { scale = scale } 
                : null;
        }

        public static ColorTag ParseColorSpan(string value)
        {
            return ColorUtility.TryParseHtmlString(value, out var color) 
                ? new ColorTag { color = color }
                : null;
        }

        public static SpannedText ParseLikeUnity(string htmlString)
        {
            var tags = ParseHtmlTags(htmlString, (tag, value) =>
            {
                switch (tag.ToLowerInvariant())
                {
                    case "b": return BoldTag.Instance;
                    case "i": return ItalicTag.Instance;
                    case "size": return ParseSizeSpan(value);
                    case "color": return ParseColorSpan(value);
                }

                return null;
            });
            
            return new SpannedText(htmlString, tags);
        }

        public static List<TextTag> ParseHtmlTags(string text, 
            Func<string, string, object> tagGenerator)
        {
            var openTags = htmlTagRegex.Matches(text);
            var resultSpans = new List<TextTag>();
            var tagStack = new Stack<TagInfo>();

            foreach (Match match in openTags)
            {
                if (match.Value.StartsWith("</"))
                {
                    if (tagStack.Count > 0)
                    {
                        var topTag = tagStack.Peek();
                        var tagName = match.Value.Substring(2, match.Value.Length - 3);

                        if (topTag.name == tagName)
                        {
                            tagStack.Pop();
                            resultSpans.Add(new TextTag
                            {
                                textStartIndex = match.Index,
                                textEndIndex = match.Index + match.Length,
                                tag = topTag.tag,
                                isClosing = true
                            });
                        }
                        // Closing tag mismatch
                    }
                    // Closing not existing tag
                }
                else
                {
                    var tagContents = match.Value
                        .Substring(1, match.Value.Length - 2)
                        .Split('=');
                    
                    var tagName = tagContents[0];
                    var tagValue = tagContents.Length > 1 ? tagContents[1] : null;
                    var tagObject = tagGenerator(tagName, tagValue);

                    if (tagObject != null)
                    {
                        tagStack.Push(new TagInfo
                        {
                            tag = tagObject,
                            name = tagName
                        });
                        resultSpans.Add(new TextTag
                        {
                            textStartIndex = match.Index,
                            textEndIndex = match.Index + match.Length,
                            tag = tagObject,
                            isClosing = false
                        });
                    }
                    // Unknown tag
                }
            }

            if (tagStack.Count > 0) // Mismatched/non closed tags found, treat result as malformed and return without tags
                return new List<TextTag>(0);

            return resultSpans;
        }
        
        private struct TagInfo
        {
            public string name;
            public object tag;
        }
    }
}