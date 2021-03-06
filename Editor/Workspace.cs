﻿using System;
using System.Collections.Generic;

namespace Editor
{
    public class Workspace
    {
        public Editor ActiveEditor;
        public List<Editor> Editors = new List<Editor>();

        private IConsole _console;
        private int numUntitled = 0;

        public Workspace(IConsole console)
        {
            _console = console;
        }

        public void Run()
        {
            if (Editors.Count == 0)
            {
                _console.Clear();
                _console.CursorTop = 2;
                _console.CursorLeft = 4;
                _console.Write("* No editors! Press Ctrl+N to create or Ctrl+O to open!");
            }
            else
            {
                RedrawEditors();
            }

            while (true)
            {
                var keyInfo = _console.ReadKey(true);
                if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                {
                    switch (keyInfo.Key)
                    {
                        case ConsoleKey.N:
                            NewEditor();
                            break;
                        case ConsoleKey.O:
                            OpenEditor();
                            break;
                        case ConsoleKey.LeftArrow:
                            {
                                var idx = Editors.FindIndex(x => x == ActiveEditor);
                                ActiveEditor = Editors[idx == 0 ? Editors.Count - 1 : idx - 1];
                                ActiveEditor.ActivateCursor();
                                continue;
                            }
                        case ConsoleKey.RightArrow:
                            {
                                var idx = Editors.FindIndex(x => x == ActiveEditor);
                                ActiveEditor = Editors[(idx + 1) % Editors.Count];
                                ActiveEditor.ActivateCursor();
                                continue;
                            }
                    }
                }

                if (Editors.Count == 0)
                {
                    continue;
                }

                bool redraw = false;

                switch (keyInfo.Key)
                {
                    case ConsoleKey.UpArrow:
                        redraw = ActiveEditor.HandleInput(EditorInput.UpArrow);
                        break;
                    case ConsoleKey.DownArrow:
                        redraw = ActiveEditor.HandleInput(EditorInput.DownArrow);
                        break;
                    case ConsoleKey.LeftArrow:
                        redraw = ActiveEditor.HandleInput(EditorInput.LeftArrow);
                        break;
                    case ConsoleKey.RightArrow:
                        redraw = ActiveEditor.HandleInput(EditorInput.RightArrow);
                        break;
                    case ConsoleKey.Enter:
                        redraw = ActiveEditor.HandleInput(EditorInput.Enter);
                        break;
                    case ConsoleKey.Backspace:
                        redraw = ActiveEditor.HandleInput(EditorInput.Backspace);
                        break;
                    case ConsoleKey.Tab:
                        if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift))
                            redraw = ActiveEditor.HandleInput(EditorInput.ShiftTab);
                        else
                            redraw = ActiveEditor.HandleInput(EditorInput.Tab);
                        break;
                }

                if (!Char.IsControl(keyInfo.KeyChar))
                {
                    redraw = ActiveEditor.HandleInput(keyInfo.KeyChar.ToString());
                }

                if (redraw)
                {
                    ActiveEditor.AdjustWindow();
                    ActiveEditor.Render();
                }
                else if (ActiveEditor.AdjustWindow())
                {
                    ActiveEditor.Render();
                }

                ActiveEditor.ActivateCursor();
            }
        }

        private void OpenEditor()
        {
            throw new NotImplementedException();
        }

        private void NewEditor()
        {
            Editors.Insert(0, new Editor(_console, string.Format("untitled {0}", ++numUntitled), 0, 0, 80, 24));
            ActiveEditor = Editors[0];
            ResizeEditors();
            RedrawEditors();
        }

        private void ResizeEditors()
        {
            int nextX = 0;
            int termWidth = Editors[0].Width;
            int termHeight = Editors[0].Height;

            foreach (var editor in Editors)
            {
                editor.X = nextX;
                editor.Y = 0;
                editor.Width = termWidth / Editors.Count;
                editor.Height = termHeight;

                nextX += editor.Width;
            }
        }

        private void RedrawEditors()
        {
            foreach (var editor in Editors)
            {
                editor.AdjustWindow();
                editor.Render();
            }

            ActiveEditor.ActivateCursor();
        }
    }
}
