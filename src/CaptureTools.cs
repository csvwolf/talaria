using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class PadHop {
 ProgressBar captureSetupProgress;Button captureSetup,captureSetupLog;TextBlock captureSetupStatus;bool captureSetupBusy;string captureSetupLogPath;
 string CaptureTool(string key){string file=Path.Combine(AppPaths.Data,"capture-tools.json");if(!File.Exists(file))throw new Exception(L.T("请先点击“安装录制组件”，完成后即可录制。"));var d=new JavaScriptSerializer().Deserialize<Dictionary<string,string>>(File.ReadAllText(file));string path;if(d==null||!d.TryGetValue(key,out path)||!Path.IsPathRooted(path)||!File.Exists(path))throw new Exception(L.T("录制组件不可用，请点击“安装录制组件”修复。"));return path;}
 bool CaptureToolsReady(){try{CaptureTool("Python");CaptureTool("Parser");return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"System32","wpr.exe"));}catch{return false;}}
 void BuildCaptureSetup(StackPanel parent){var row=new StackPanel{Orientation=Orientation.Horizontal};parent.Children.Add(row);captureSetup=Btn(row,L.T("安装录制组件"),InstallCaptureTools);captureSetupLog=Btn(row,L.T("打开安装日志"),delegate{if(captureSetupLogPath!=null && File.Exists(captureSetupLogPath))Process.Start(new ProcessStartInfo("notepad.exe",Quote(captureSetupLogPath)){UseShellExecute=true});});captureSetupLog.Visibility=Visibility.Collapsed;captureSetupProgress=new ProgressBar{Height=4,IsIndeterminate=true,Visibility=Visibility.Collapsed,Margin=new Thickness(0,8,0,4)};parent.Children.Add(captureSetupProgress);captureSetupStatus=Hint(parent,CaptureToolsReady()?L.T("录制组件已配置，可直接录制；遇到问题可重新检测并修复。"):L.T("首次录制需安装组件，完成后自动配置，无需填写路径。"));}
 async void InstallCaptureTools(){
  if(captureSetupBusy)return;
  if(recorder!=null){Notice(L.T("请先结束录制。"));return;}
  if(MessageBox.Show(window,L.T("将复用已有组件；缺少时从 Python 和微软官方网站下载约 52 MB。Python 仅供 Talaria 使用，不修改系统 PATH；微软蓝牙分析组件安装时可能需要管理员确认。完成后自动配置。继续？"),L.T("安装录制组件"),MessageBoxButton.OKCancel,MessageBoxImage.Information)!=MessageBoxResult.OK){CaptureSetupNote("USER cancelled-before-install");return;}
  captureSetupBusy=true;captureSetup.IsEnabled=false;captureSetup.Content=L.T("检测中…");captureSetupProgress.Visibility=Visibility.Visible;recordButton.IsEnabled=false;captureSetupStatus.Text=L.T("正在检测录制组件…");Directory.CreateDirectory(AppPaths.Logs);captureSetupLogPath=Path.Combine(AppPaths.Logs,"capture-setup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".log");captureSetupLog.Visibility=Visibility.Visible;
  try{
   int result=await Task.Run(delegate{
    CaptureSetupNote("START version="+ProductVersion);
    string script=Path.Combine(Root,"Install-CaptureTools.ps1");if(!File.Exists(script))throw new FileNotFoundException();
    var start=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"System32","WindowsPowerShell","v1.0","powershell.exe"),"-NoProfile -ExecutionPolicy Bypass -File "+Quote(script)){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
    using(var process=new Process{StartInfo=start}){
     process.OutputDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data==null)return;string line=e.Data;CaptureSetupNote(line);window.Dispatcher.BeginInvoke(new Action(delegate{if(line.StartsWith("STEP ")){string step=line.Substring(5);captureSetupStatus.Text=CaptureSetupStage(step);captureSetup.Content=L.T(step.EndsWith("download") && step!="verify-download"?"下载中…":step.Contains("install")?"安装中…":step.StartsWith("verify")?"校验中…":"检测中…");}}));};
     process.ErrorDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data!=null)CaptureSetupNote("STDERR present (details omitted)");};
     process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();process.WaitForExit();return process.ExitCode;
    }
   });
   CaptureSetupNote("END exit="+result);
   captureSetupStatus.Text=result==0 && CaptureToolsReady()?L.T("录制组件已就绪，可以开始录制。"):result==1223?L.T("已取消安装，可随时重试。"):L.T("录制组件安装未完成，请打开安装日志查看原因后重试。");
  }catch(Exception e){CaptureSetupNote("FAILED type="+e.GetType().Name+" hresult="+e.HResult);captureSetupStatus.Text=L.T("录制组件安装未完成，请打开安装日志查看原因后重试。");}
  finally{captureSetupBusy=false;captureSetup.IsEnabled=true;captureSetup.Content=L.T("安装录制组件");captureSetupProgress.Visibility=Visibility.Collapsed;recordButton.IsEnabled=true;}
 }
 readonly object captureSetupLogGate=new object();
 void CaptureSetupNote(string value){lock(captureSetupLogGate){try{Directory.CreateDirectory(AppPaths.Logs);if(captureSetupLogPath==null)captureSetupLogPath=Path.Combine(AppPaths.Logs,"capture-setup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".log");File.AppendAllText(captureSetupLogPath,DateTime.UtcNow.ToString("o")+" "+value+Environment.NewLine);}catch(IOException){}catch(UnauthorizedAccessException){}}}
 string CaptureSetupStage(string step){switch(step){case "detect":return L.T("正在检测录制组件…");case "verify-download":return L.T("正在校验下载文件…");case "python-download":return L.T("正在下载 Python 录制运行环境…");case "python-install":return L.T("正在配置专用 Python…");case "microsoft-download":return L.T("正在下载微软蓝牙分析组件…");case "microsoft-install":return L.T("正在安装微软组件，请留意管理员确认…");default:return L.T("正在验证并保存录制组件设置…");}}
}
