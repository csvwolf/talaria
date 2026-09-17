using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
internal sealed partial class PadHop {
 const string StartupKey=@"Software\Microsoft\Windows\CurrentVersion\Run";
 static string StartupCommand(string exe){return Quote(exe)+" --autostart";}
 static string CurrentStartupCommand(){return StartupCommand(Process.GetCurrentProcess().MainModule.FileName);}
 static bool StartupEnabled(){using(var key=Registry.CurrentUser.OpenSubKey(StartupKey)){return key!=null && string.Equals(key.GetValue("Talaria") as string,CurrentStartupCommand(),StringComparison.OrdinalIgnoreCase);}}
 void BuildStartup(bool live){
  var card=AboutCard(L.T("启动设置"));
  var toggle=new CheckBox{Content=L.T("登录 Windows 后自动启动"),IsChecked=false};card.Children.Add(toggle);
  var hint=Hint(card,L.T("默认关闭。开启后登录时启动到托盘，不自动启用接管；取消勾选即可关闭。仅对当前 Windows 用户生效。"));hint.TextWrapping=TextWrapping.Wrap;
  try{if(live)toggle.IsChecked=StartupEnabled();}catch(Exception e){hint.Text=e.Message;toggle.IsEnabled=false;}
  bool syncing=false;RoutedEventHandler change=delegate{
   if(!live || syncing)return;bool wanted=toggle.IsChecked==true;
   try{using(var key=Registry.CurrentUser.CreateSubKey(StartupKey)){if(wanted)key.SetValue("Talaria",CurrentStartupCommand(),RegistryValueKind.String);else key.DeleteValue("Talaria",false);}Notice(wanted?L.T("已开启登录自启，下次登录时启动到托盘。"):L.T("已关闭登录自启。"));}
   catch(Exception e){syncing=true;toggle.IsChecked=!wanted;syncing=false;Notice(L.T("无法修改登录自启：")+e.Message);}
  };toggle.Checked+=change;toggle.Unchecked+=change;
 }
 static void TestStartup(){if(StartupCommand(@"C:\Program Files\PadHop\PadHop.exe")!="\"C:\\Program Files\\PadHop\\PadHop.exe\" --autostart")throw new Exception("Startup path quoting failed");}
}
