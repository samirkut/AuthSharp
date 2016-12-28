using System;
using System.Collections.Generic;
using AuthSharp.Model;

namespace AuthSharp.View
{
    public interface IView : IDisposable
    {
        bool Login();

        void Home(bool forceRedraw, double progress);

        void Prefs();

        void New();

        void Delete();
    }
}