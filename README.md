Benchmark code of a Gradient5x5

List of tested languages :
- Cpp : Common C++ code
- CSharp : Common unsafe C# code
- CSharp_ILGPU : Using GPU with ILGPU library
- CSharp_DLL : C# code using a C++ DLL to call the process
- CSharp_CLR : C# code using a CLR DLL which use a C++ native code


Launching a test :
There are 2 different way to start a test :
- To launch the process only one time, save the result and get all times : Just launch Cpp.exe or anonther exe in your console
- To launch the process many times and get the average for the process only : Launch Cpp.exe or enother exe with argument the number of loop

To launch CShap_ILGPU.exe on CPU accelerator or onlu GPU accelerator, plese specifiate it in your arguments :
- "Csharp_ILGPU.exe 20 cpu" Will launch the process 20 times on the CPU accelerator
- "Csharp_ILGPU.exe gpu" will launch the process only one time on the GPU accelerator, get all the timers and save the result