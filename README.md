# vibranceGUI

VibranceGUI is a Windows Utility written in C# that automates NVIDIAs Digitial Vibrance Control and AMDs Saturation for Games, e.g. Counter-Strike: Global Offensive by utilizing native graphic card driver APIs. 

As of 18th April 2015, vibranceGUI also fully supports AMD graphic cards. Prior to that, vibanceGUI was developed to support NVIDIA graphic cards only. 

Note that NVIDIA Laptop GPUs are not supported because their drivers do not contain the needed functionality.
Intel did not publish an API for their integrated GPUs and are not supported. 

More information and binary download available at: http://vibrancegui.com/

## Compiling

On Windows, install Visual Studio 2022 or its Build Tools with the .NET desktop build tools and the [.NET Framework 4.8 Developer Pack](https://learn.microsoft.com/en-us/dotnet/framework/install/guide-for-developers). Open a Developer Command Prompt and run:

```bat
msbuild vibrance.GUI.sln /restore /m /p:Configuration=Release /p:Platform=x86
```

Local builds output to `vibrance.GUI\bin\x86\Release\`. The GitHub Actions ZIP puts the dependency DLLs in `lib`. Extract the whole ZIP and keep `lib` and `vibrance.GUI.exe.config` alongside the executable. The NVIDIA bridge requires an x86 executable, including on 64-bit Windows.

### NVIDIA bridge provenance

The build embeds `vibrance.GUI/NVIDIA/vibranceDLL.dll` and checks its SHA-256 before compiling. The bundled DLL was added in vibranceGUI commit `5ee9adef0e8b04efbb1a00f53f231145cdfedefd`, whose message records static linking of the C++ runtime with `/MT`. The author publishes the C/C++ project separately, with build instructions for x86 and Unicode. [vibranceDLL source](https://github.com/juv/vibranceDLL)

```powershell
Get-FileHash .\vibrance.GUI\NVIDIA\vibranceDLL.dll -Algorithm SHA256
```

The expected SHA-256 is `0F229F79934F21617337C28915A9449F7B2395B20D7FB0E4A02D14277163CEA0`. The DLL has no Authenticode signature. The hash identifies the checked-in bytes. The exact source revision and toolchain that produced those bytes have not been verified.

## Contributing

Every contribution is greatly appreciated. Do not hesitate to submit every issue and pull request that comes to your mind.

## Contact

NVIDIA support: https://twitter.com/juvlarN
  
AMD support: https://twitter.com/juRiiir3

`Please do not add either of us at Steam to ask questions about vibranceGUI. Thank you.`
