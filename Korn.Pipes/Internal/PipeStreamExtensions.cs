using System;
using System.IO.Pipes;
using System.Reflection;

static class PipeStreamExtensions
{
    static Type type = typeof(PipeStream);
    static FieldInfo stateField = type.GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic);
    public static PipeState GetState(this PipeStream pipe) => (PipeState)(int)stateField.GetValue(pipe);
}

public enum PipeState
{
    WaitingToConnect,
    Connected,
    Broken,
    Disconnected,
    Closed
}