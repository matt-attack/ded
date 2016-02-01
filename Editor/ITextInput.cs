﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Editor
{
    public interface ITextInput
    {
        bool HandleInput(EditorInput input);
        bool HandleInput(string input);
        void ActivateCursor();
        bool AdjustWindow();

        void Render();
    }
}
