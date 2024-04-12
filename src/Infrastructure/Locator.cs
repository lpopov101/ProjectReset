using System;
using System.Collections;
using Godot;

public class Locator<T>
{
    private static T _value;

    public static void Register(T value)
    {
        _value = value;
    }

    public static T Get()
    {
        return _value;
    }
}
