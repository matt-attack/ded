﻿using System;

namespace Editor
{
    public enum EditorInput
    {
        UpArrow,
        DownArrow,
        LeftArrow,
        RightArrow,
        Enter,
        Backspace,
        Tab,
        ShiftTab,
        PageUp,
        PageDown,
        Home,
        End,
    }

    public class Editor
    {
        public string Name;

        public int X = 0;
        public int Y = 0;
        public int Width = 0;
        public int Height = 0;

        public int StartLine = 0;
        public int StartColumn = 0;

        public Cursor Cursor = new Cursor(0, 0);
        public Buffer Buffer = new Buffer();

        private IConsole _console;

        public Editor(IConsole console, string name, int x, int y, int w, int h)
        {
            _console = console;
            Name = name;
            X = x;
            Y = y;
            Width = w;
            Height = h;
        }

        public bool AdjustWindow()
        {
            if (Cursor.Line < StartLine)
            {
                StartLine = Cursor.Line;
                return true;
            }
            else if (Cursor.Line >= StartLine + (Height - 1))
            {
                StartLine = Cursor.Line - Height + 2;
                return true;
            }

            if (Cursor.Column < StartColumn)
            {
                StartColumn = Cursor.Column;
                return true;
            }
            else if (Cursor.Column >= StartColumn + Width - GetGutterWidth())
            {
                StartColumn = Cursor.Column - (Width - GetGutterWidth()) + 1;
                return true;
            }

            return false;
        }

        public void Render()
        {
            var gutterWidth = GetGutterWidth();

            RenderHeader(gutterWidth);

            for (int cy = Y + 1; cy < (Y + Height); cy++)
            {
                var lineIndex = StartLine + cy - Y - 1;

                ClearLine(X, cy, Width);

                _console.CursorTop = cy;
                _console.CursorLeft = X;

                if (lineIndex < Buffer.Lines.Count)
                {
                    var line = Buffer.Lines[lineIndex];

                    _console.ForegroundColor = ConsoleColor.Black;
                    _console.BackgroundColor = StartColumn == 0 ? ConsoleColor.Gray : ConsoleColor.Red;
                    {
                        _console.Write("{0}", (lineIndex + 1).ToString().PadLeft(gutterWidth - 1));
                    }
                    _console.ResetColor();

                    var cropped = line.Data;

                    if (StartColumn != 0)
                        if (StartColumn < line.Data.Length)
                            cropped = cropped.Substring(StartColumn);
                        else
                            cropped = "";

                    if (cropped.Length > Width - gutterWidth)
                        cropped = cropped.Substring(0, Width - gutterWidth);

                    _console.Write(" {0}", cropped);
                }
            }
        }

        private void ClearLine(int x, int y, int w)
        {
            _console.CursorTop = y;
            _console.CursorLeft = x;

            for (int i = 0; i < w; i++)
                _console.Write(' ');
        }

        private void RenderHeader(int gutterWidth)
        {
            _console.ForegroundColor = ConsoleColor.Black;
            _console.BackgroundColor = ConsoleColor.Gray;
            {
                ClearLine(X, Y, Width);

                _console.CursorTop = Y;
                _console.CursorLeft = X;
                _console.Write("{0} {1}", "#".PadLeft(gutterWidth - 1), Name);
            }
            _console.ResetColor();
        }

        public void ActivateCursor()
        {
            int x, y;
            Translate(Cursor, out x, out y);

            _console.CursorTop = y;
            _console.CursorLeft = x;
        }

        public bool HandleInput(EditorInput input)
        {
            switch (input)
            {
                case EditorInput.UpArrow:
                    Cursor = Cursor.Move(Buffer, MoveDirection.Up);
                    return false;
                case EditorInput.DownArrow:
                    Cursor = Cursor.Move(Buffer, MoveDirection.Down);
                    return false;
                case EditorInput.LeftArrow:
                    Cursor = Cursor.Move(Buffer, MoveDirection.Left);
                    return false;
                case EditorInput.RightArrow:
                    Cursor = Cursor.Move(Buffer, MoveDirection.Right);
                    return false;
                case EditorInput.Enter:
                    Buffer.SplitLine(Cursor);
                    Cursor = Cursor.Move(Buffer, MoveDirection.Right);
                    return true;
                case EditorInput.Backspace:
                    if (Cursor.AtFirstColumn(Buffer))
                    {
                        if (!Cursor.AtStart(Buffer))
                        {
                            Cursor = Cursor.Move(Buffer, MoveDirection.Left);
                            Buffer.MergeLine(Cursor.Line + 1);
                            return true;
                        }
                    }
                    else
                    {
                        Buffer.RemoveAt(Cursor, 1);
                        Cursor = Cursor.Move(Buffer, MoveDirection.Left);
                        return true;
                    }
                    return false;
                case EditorInput.Tab:
                    Buffer.InsertAt(Cursor, "    ");
                    Cursor.Column += 4;
                    return true;
                case EditorInput.ShiftTab:
                    return false;
            }

            return false;
        }

        public bool HandleInput(string str)
        {
            Buffer.InsertAt(Cursor, str);
            Cursor.Column += str.Length;
            return true;
        }

        private void Translate(Cursor where, out int x, out int y)
        {
            x = X + GetGutterWidth() + where.Column - StartColumn;
            y = Y + 1 + where.Line - StartLine;
        }

        private int GetGutterWidth()
        {
            int num = Buffer.Lines.Count;
            int length = 0;

            do
            {
                num /= 10;
                length++;
            } while (num != 0);

            return length + 1;
        }
    }
}
