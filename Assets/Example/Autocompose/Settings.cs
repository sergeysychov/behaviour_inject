using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Settings
{
    public string IP { get; private set; }

    public Settings(string address)
    {
        IP = address;
    }
}
