using System;
using System.Diagnostics;

namespace ItServiceApp.InjectOrnek
{
    public class newMyDependency : IMyDependency
    {
        public void Log(string message)
        {
            Debug.WriteLine($"{DateTime.Now}-{message}");
        }
    }
}
