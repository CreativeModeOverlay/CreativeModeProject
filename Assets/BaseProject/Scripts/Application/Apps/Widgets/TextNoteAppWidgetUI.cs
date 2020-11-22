using System.Collections.Generic;
using CreativeMode;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TextNoteAppWidgetUI : BaseAppWidgetUI<TextNoteAppWidget>
{
    public Text titleText;
    public Text counterText;
    public Text noteText;
    public RectTransform noteRect;
    public RectTransform noteContainer;

    public float timePerPage = 10;
    public float scrollSpeed = 32;
    
    private readonly object pageSequence = new object();
    private Sequence displaySequence;

    private void OnRectTransformDimensionsChange()
    {
        displaySequence?.Restart();
    }

    public void SetNotes(List<Note> notes)
    {
        if (notes == null || notes.Count == 0)
        {
            ShowNote(new Note
            {
                id = -1,
                title = "null",
                pages = new []{ "null" }
            }, null);
            return;
        }
        
        var currentNote = 0;
        
        void ShowNextNote()
        {
            if (currentNote >= notes.Count)
                currentNote = 0;

            var note = currentNote++;
            ShowNote(notes[note], ShowNextNote);
        }
        
        ShowNextNote();
    }
    
    private void ShowNote(Note note, TweenCallback onShowCompleted)
    {
        var currentPage = 0;
        var pageCount = note.pages.Length;

        void ShowNextPage()
        {
            if (currentPage >= pageCount)
            {
                onShowCompleted?.Invoke();
                return;
            }
            
            var page = currentPage++;
            ShowTextPage(pageCount > 0 ? note.pages[page] : "", page, pageCount, ShowNextPage);
        }
        
        titleText.ChangeText(note.title);
        ShowNextPage();
    }

    private void ShowTextPage(string text, int page, int pageCount, TweenCallback onShowCompleted)
    {
        DOTween.Kill(pageSequence);
        
        var multiplePages = pageCount > 1;
        counterText.DOFade(multiplePages ? 1f : 0f, 0.5f);
        
        if (multiplePages)
        {
            counterText.text = $"{page + 1} / {pageCount}";
        }

        DOTween.Sequence()
            .SetTarget(pageSequence)
            .Insert(0, noteText.ChangeText(text, onTextChanged: () =>
            {
                // jump to top
                noteRect.pivot = new Vector2(0, 1);
            }))
            .AppendCallback(() => displaySequence = CreatePageDisplaySequence(multiplePages, onShowCompleted))
            .Play();
    }

    private Sequence CreatePageDisplaySequence(bool multiplePages, TweenCallback onShowCompleted)
    {
        var note = noteRect.rect;
        var container = noteContainer.rect;
        var diff = note.height - container.height;
        noteRect.pivot = new Vector2(0, 1);

        if (diff > 0)
        {
            var halfDuration = timePerPage / 2f;
            var scrollDuration = diff / scrollSpeed;
            var sequence = DOTween.Sequence()
                .SetTarget(pageSequence)
                .Insert(halfDuration, noteRect.DOPivotY(0, scrollDuration)
                    .SetEase(Ease.Linear));

            if (multiplePages)
            {
                sequence.InsertCallback(timePerPage + scrollDuration, onShowCompleted);
            }
            else
            {
                sequence.AppendInterval(halfDuration)
                    .Append(noteRect.DOPivotY(1f, scrollDuration).SetEase(Ease.Linear))
                    .AppendInterval(halfDuration)
                    .AppendCallback(onShowCompleted);
            }
                        
            return sequence.Play();
        }

        return DOTween.Sequence()
            .SetId(pageSequence)
            .InsertCallback(timePerPage, onShowCompleted)
            .Play();
    }
}
