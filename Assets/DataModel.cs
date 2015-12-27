using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class DataModel
{
    public string Data { get; private set; }

    public DataModel(string data)
    {
        Data = data;
    }
}
