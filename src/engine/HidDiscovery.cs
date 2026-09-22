using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;

// SetupAPI HID enumeration and shared read-only input. No feature/output writes.
internal static class HidDiscovery {
 internal static int Failed;
 internal static void Test(){
  if(Marshal.SizeOf(typeof(Interface))!=(IntPtr.Size==8?32:28) || Marshal.SizeOf(typeof(Attributes))!=12)throw new Exception("HID native structure layout");
  var raw=new Device{Path="HID#SLOT-A",Handle=new IntPtr(123)};var list=Combine(new[]{raw},new[]{new Device{Path="hid#slot-a",Source="HID"},new Device{Path="hid#slot-b",Source="HID"}});
  if(list.Count!=2 || !object.ReferenceEquals(list[0],raw) || list[1].Source!="HID")throw new Exception("HID dedup lost Raw Input precedence or receiver slot");
  uint bits;if(Decoder.Decode(new byte[]{0x44,0,0,0,0},out bits)!=null)throw new Exception("Receiver status mistaken for controller state");
 }

 internal static List<Device> Merge(IEnumerable<Device> raw){var original=new List<Device>(raw);try{return Combine(original,Enumerate());}catch{Failed++;return original;}}
 internal static List<Device> Combine(IEnumerable<Device> raw,IEnumerable<Device> hid){var result=new List<Device>(raw);foreach(var d in hid){if(!result.Exists(x=>string.Equals(x.Path,d.Path,StringComparison.OrdinalIgnoreCase)))result.Add(d);}return result;}
 internal static List<Device> Enumerate(){Failed=0;var list=new List<Device>();Guid guid;HidD_GetHidGuid(out guid);IntPtr set=SetupDiGetClassDevs(ref guid,null,IntPtr.Zero,0x12);if(set==new IntPtr(-1))throw new Win32Exception();try{for(uint i=0;i<512;i++){var iface=new Interface{Size=Marshal.SizeOf(typeof(Interface))};if(!SetupDiEnumDeviceInterfaces(set,IntPtr.Zero,ref guid,i,ref iface)){if(Marshal.GetLastWin32Error()!=259)Failed++;break;}uint needed;SetupDiGetDeviceInterfaceDetail(set,ref iface,IntPtr.Zero,0,out needed,IntPtr.Zero);if(needed<8 || needed>65536){Failed++;continue;}IntPtr detail=Marshal.AllocHGlobal((int)needed);try{Marshal.WriteInt32(detail,IntPtr.Size==8?8:6);if(!SetupDiGetDeviceInterfaceDetail(set,ref iface,detail,needed,out needed,IntPtr.Zero)){Failed++;continue;}string path=Marshal.PtrToStringUni(IntPtr.Add(detail,4));using(var h=HidInput.Open(path,0)){if(h.IsInvalid){Failed++;continue;}var a=new Attributes{Size=Marshal.SizeOf(typeof(Attributes))};if(!HidD_GetAttributes(h,ref a)){Failed++;continue;}if(a.Vid!=0x28de)continue;IntPtr data;if(!HidD_GetPreparsedData(h,out data)){Failed++;continue;}IntPtr caps=Marshal.AllocHGlobal(64);try{if(HidP_GetCaps(data,caps)!=0x110000){Failed++;continue;}list.Add(new Device{Type=2,Vid=a.Vid,Pid=a.Pid,Page=(ushort)Marshal.ReadInt16(caps,2),Usage=(ushort)Marshal.ReadInt16(caps,0),Path=path,Source="HID"});}finally{Marshal.FreeHGlobal(caps);HidD_FreePreparsedData(data);}}}finally{Marshal.FreeHGlobal(detail);}}}finally{SetupDiDestroyDeviceInfoList(set);}return list;}
 internal static void VerifySlots(List<Device> devices){int attempted=0;foreach(var d in devices){if(!DeviceGate.IsSc2(d.Type,d.Vid,d.Pid,d.Page,d.Usage))continue;if(d.Source!="HID" && DeviceGate.Transport(d.Pid,d.Path)!="receiver"){d.State="not-probed";continue;}if(attempted++>=16){d.State="not-probed (scan limit)";continue;}try{using(var input=new HidInput(d.Path)){var clock=Stopwatch.StartNew();d.State="no-state-yet";while(clock.ElapsedMilliseconds<200){var report=input.Poll();uint bits;if(report!=null && Decoder.Decode(report,out bits)!=null){d.State="state-confirmed";break;}Thread.Sleep(2);}}}catch(Win32Exception e){d.State="read-error-"+e.NativeErrorCode;}catch{d.State="read-error";}}}
 [StructLayout(LayoutKind.Sequential)]struct Interface{internal int Size;internal Guid Guid;internal uint Flags;internal UIntPtr Reserved;}
 [StructLayout(LayoutKind.Sequential)]struct Attributes{internal int Size;internal ushort Vid,Pid,Version;}
 [DllImport("hid.dll")]static extern void HidD_GetHidGuid(out Guid guid);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr SetupDiGetClassDevs(ref Guid guid,string enumerator,IntPtr parent,uint flags);
 [DllImport("setupapi.dll",SetLastError=true)]static extern bool SetupDiEnumDeviceInterfaces(IntPtr set,IntPtr info,ref Guid guid,uint index,ref Interface data);
 [DllImport("setupapi.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr set,ref Interface data,IntPtr detail,uint size,out uint required,IntPtr info);
 [DllImport("setupapi.dll")]static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
 [DllImport("hid.dll",SetLastError=true)]static extern bool HidD_GetAttributes(SafeFileHandle h,ref Attributes a);
 [DllImport("hid.dll",SetLastError=true)]internal static extern bool HidD_GetPreparsedData(SafeFileHandle h,out IntPtr data);
 [DllImport("hid.dll")]internal static extern bool HidD_FreePreparsedData(IntPtr data);
 [DllImport("hid.dll")]internal static extern int HidP_GetCaps(IntPtr data,IntPtr caps);
}

internal sealed class HidInput:IDisposable {
 SafeFileHandle file;IntPtr buffer,overlapped,signal;int size;bool pending,disposed;
 internal static SafeFileHandle Open(string path,uint access){return CreateFile(path,access,3,IntPtr.Zero,3,0x40000000,IntPtr.Zero);}
 internal HidInput(string path){try{file=Open(path,0x80000000);if(file.IsInvalid)throw new Win32Exception();IntPtr data;if(!HidDiscovery.HidD_GetPreparsedData(file,out data))throw new Win32Exception();IntPtr caps=Marshal.AllocHGlobal(64);try{if(HidDiscovery.HidP_GetCaps(data,caps)!=0x110000)throw new Exception("HID caps");size=(ushort)Marshal.ReadInt16(caps,4);}finally{Marshal.FreeHGlobal(caps);HidDiscovery.HidD_FreePreparsedData(data);}if(size<46 || size>4096)throw new Exception("Unsupported input report size");buffer=Marshal.AllocHGlobal(size);overlapped=Marshal.AllocHGlobal(32);for(int i=0;i<32;i++)Marshal.WriteByte(overlapped,i,0);signal=CreateEvent(IntPtr.Zero,true,false,null);if(signal==IntPtr.Zero)throw new Win32Exception();Marshal.WriteIntPtr(overlapped,IntPtr.Size==8?24:16,signal);}catch{Dispose();throw;}}
 internal byte[] Poll(){if(disposed)throw new ObjectDisposedException("HidInput");if(!pending){if(!ResetEvent(signal))throw new Win32Exception();bool ok=ReadFile(file,buffer,(uint)size,IntPtr.Zero,overlapped);int error=ok?0:Marshal.GetLastWin32Error();if(!ok && error!=997)throw new Win32Exception(error);pending=true;}uint wait=WaitForSingleObject(signal,0);if(wait==258)return null;if(wait!=0)throw new Win32Exception();uint got;bool done=GetOverlappedResult(file,overlapped,out got,false);pending=false;if(!done)throw new Win32Exception();if(got==0 || got>size)throw new Exception("Invalid HID read");var bytes=new byte[got];Marshal.Copy(buffer,bytes,0,(int)got);return bytes;}
 public void Dispose(){if(disposed)return;disposed=true;if(pending && file!=null && !file.IsInvalid){CancelIoEx(file,overlapped);uint ignored;GetOverlappedResult(file,overlapped,out ignored,true);pending=false;}if(file!=null)file.Dispose();if(signal!=IntPtr.Zero)CloseHandle(signal);if(overlapped!=IntPtr.Zero)Marshal.FreeHGlobal(overlapped);if(buffer!=IntPtr.Zero)Marshal.FreeHGlobal(buffer);}
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern SafeFileHandle CreateFile(string p,uint access,uint share,IntPtr security,uint creation,uint flags,IntPtr template);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool ReadFile(SafeFileHandle h,IntPtr b,uint count,IntPtr read,IntPtr ov);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool GetOverlappedResult(SafeFileHandle h,IntPtr ov,out uint count,bool wait);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool CancelIoEx(SafeFileHandle h,IntPtr ov);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)]static extern IntPtr CreateEvent(IntPtr security,bool manual,bool initial,string name);
 [DllImport("kernel32.dll",SetLastError=true)]static extern bool ResetEvent(IntPtr signal);
 [DllImport("kernel32.dll",SetLastError=true)]static extern uint WaitForSingleObject(IntPtr signal,uint ms);
 [DllImport("kernel32.dll")]static extern bool CloseHandle(IntPtr h);
}
