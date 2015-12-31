using System;
using BehaviourInject;

public class Connection
{
    private Settings _settings;

    public bool Connected { get; set; }

    public Connection()
    {
        _settings = new Settings("NO IP");
    }

    [Inject]
    public Connection(Settings settings)
    {
        _settings = settings;
    }


    public string Read()
    {
        return String.Format("message from {0} : blah blah blah", _settings.IP);
    }
}
