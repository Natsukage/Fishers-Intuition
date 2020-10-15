# Nhaama
Multi-purpose .NET memory-editing library

## Getting a process

To get a Nhaama-wrapped process, just call ``GetNhaamaProcess()`` on a normal ``System.Diagnostics.Process`` or pass it as a parameter to a new ``NhaamaProcess``.

```cs
var process1 = Process.GetProcessesByName("ffxiv_dx11")[0].GetNhaamaProcess();
var process2 = new NhaamaProcess(Process.GetProcessesByName("ffxiv_dx11")[0]);
```

## Pointers

To resolve pointers, just pass their path following a ``NhaamaProcess`` into a new ``Pointer``. This can be in parmeter- or string-form.

In parameter form, it's assumed to be starting from the main module of the passed process. This can be changed by passing another ``ProcessModule`` after the ``NhaamaProcess``.

```cs
var pointer1 = new Pointer(process1, 0x19815F0, 0x10, 0x8, 0x28, 0x80);
var pointer2 = new Pointer(process1, "ffxiv_dx11.exe+019815F0,10,8,28,80");

Console.WriteLine(pointer1.Address.ToString("X"));
Console.WriteLine(pointer2.Address.ToString("X"));
```

### Serialization

## Reading

You can read values by calling a ``Read`` function of the wanted type on a ``NhaamaProcess``, passing the offset to read from.

```cs
Console.WriteLine(process1.ReadUInt64(pointer1));
```
