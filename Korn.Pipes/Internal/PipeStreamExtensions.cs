using System;
using System.IO.Pipes;
using System.Reflection;

static class PipeStreamExtensions
{
    static PipeStreamExtensions()
    {
        var type = typeof(PipeStream);
        stateField = 
            type.GetField("_state", BindingFlags.Instance | BindingFlags.NonPublic)/*.net*/
            ??type.GetField("m_state", BindingFlags.Instance | BindingFlags.NonPublic)/*.netframework*/;

        if (stateField == null)
            throw new Exception("PipeStreamExtensions->.cctor(): Unable find the _state field.");
    }

    static FieldInfo stateField;
    public static PipeState GetState(this PipeStream pipe) => pipe == null ? PipeState.Closed : (PipeState)(int)stateField.GetValue(pipe);
}

public enum PipeState
{
    WaitingToConnect,
    Connected,
    Broken,
    Disconnected,
    Closed
}