// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Management.Automation;

string python_version = "";

bool IsPython()
{
    string? output = ExecutePowerShell("python", "--version");
    if (output == null)
        return false;
    if (output.StartsWith("Python"))
    {
        string ver_str = output.Substring(6);
        ver_str = ver_str.IndexOf('.') != ver_str.LastIndexOf('.') ? ver_str.Substring(0, ver_str.LastIndexOf('.')) : ver_str;
        python_version = ver_str.Trim();
        float version = float.Parse(ver_str);
        if (version < 3.6)
        {
            Console.WriteLine($"Python {version} not supported. Version 3.6 atleast required.");
            return false;
        }

        return true;
    }
    
    return false;
}

void InstallModules()
{
    PowerShell.Create()
        .AddCommand("pip")
        .AddArgument("install")
        .AddParameter("-r", "requirements.txt")
        .Invoke();
}

void Launch()
{
    string? python_path = PowerShell.Create()
        .AddCommand("python")
        .AddParameter("-c", "\"import sys; print(sys.executable)\"")
        .Invoke()[0].ToString();

    ProcessStartInfo psi = new(python_path, "./scripts/app.py");

    Process process = Process.Start(psi);
    process.WaitForExit();
    
}

string? ExecutePowerShell(string command, string? argument = null)
{
    PowerShell power_shell = PowerShell.Create();

    var ps = power_shell.AddCommand(command);
    if(argument != null)
        ps = ps.AddArgument(argument);
    var collection = ps.Invoke();
    if(collection.Count > 0)
        return collection[0].ToString();
    return null;
}
//run

if (!IsPython())
{
    Console.WriteLine("Python not found");
}
else
    Console.WriteLine("Python version: " + python_version);

Console.WriteLine("Installing Modules");
InstallModules();
Console.WriteLine("Launching");
Launch();