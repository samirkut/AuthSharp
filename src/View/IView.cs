using System;
using System.Collections.Generic;
using AuthSharp.Model;

namespace AuthSharp.View
{
    public interface IView
    {
        bool Login();

        void Home();

        void Prefs();

        void New();

        void Delete();
    }
}