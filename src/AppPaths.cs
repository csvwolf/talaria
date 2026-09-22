using System;
using System.IO;
using System.Linq;
internal static class AppPaths {
 internal static readonly string Install=AppDomain.CurrentDomain.BaseDirectory;
 internal static string Data;
 internal static string Logs;
 internal static void Initialize(bool isolated){Data=isolated?Path.Combine(Path.GetTempPath(),"PadHop-tests-"+Guid.NewGuid().ToString("N")):Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"PadHop");Directory.CreateDirectory(Data);Logs=isolated?Path.Combine(Data,"logs"):Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"Talaria","logs");Directory.CreateDirectory(Logs);if(!isolated){MigrateLogs(Path.Combine(Data,"logs"),Logs);PruneLogs();}}
 // Copy old logs without changing configuration paths or deleting the originals.
 internal static void MigrateLogs(string source,string destination){
  if(!Directory.Exists(source))return;
  string marker=Path.Combine(destination,".padhop-migrated");if(File.Exists(marker))return;
  try{CopyLogs(new DirectoryInfo(source),destination);File.WriteAllText(marker,"Migrated legacy logs; originals retained.");}catch(IOException){}catch(UnauthorizedAccessException){}
 }
 static void CopyLogs(DirectoryInfo source,string destination){
  if((source.Attributes&FileAttributes.ReparsePoint)!=0)return;
  Directory.CreateDirectory(destination);
  foreach(var f in source.GetFiles()){if((f.Attributes&FileAttributes.ReparsePoint)!=0 || f.Name.StartsWith("stop-",StringComparison.Ordinal))continue;string target=Path.Combine(destination,f.Name);if(!File.Exists(target))f.CopyTo(target);}
  foreach(var d in source.GetDirectories())CopyLogs(d,Path.Combine(destination,d.Name));
 }
 internal static void PruneLogs(){var files=new DirectoryInfo(Logs).GetFiles().Where(f=>f.Name!=".padhop-migrated").OrderByDescending(f=>f.LastWriteTimeUtc).ToArray();long bytes=0;foreach(var f in files){bytes+=f.Length;if(f.LastWriteTimeUtc<DateTime.UtcNow.AddDays(-7) || bytes>20*1024*1024)try{if((f.Attributes&FileAttributes.ReparsePoint)==0)f.Delete();}catch(IOException){}}}
 internal static string Engine{get{return Path.Combine(Install,"PadHop.Engine.exe");}}
 internal static string Capture{get{return Path.Combine(Install,"capture");}}
}
