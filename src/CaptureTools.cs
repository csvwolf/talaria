using System;
using System.IO;
using System.Collections.Generic;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
internal sealed partial class PadHop {
 ProgressBar captureSetupProgress;Button captureSetup,captureSetupLog;TextBlock captureSetupStatus;bool captureSetupBusy,captureSetupNeedsRepair;string captureSetupLogPath;
 string CaptureTool(string key){string file=Path.Combine(AppPaths.Data,"capture-tools.json");if(!File.Exists(file))throw new Exception(L.T("请先点击“安装录制组件”，完成后即可录制。"));var d=new JavaScriptSerializer().Deserialize<Dictionary<string,string>>(File.ReadAllText(file));string path;if(d==null||!d.TryGetValue(key,out path)||!Path.IsPathRooted(path)||!File.Exists(path))throw new Exception(L.T("录制组件不可用，请点击“安装录制组件”修复。"));return path;}
 bool CaptureToolsReady(){try{CaptureTool("Python");CaptureTool("Parser");return File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"System32","wpr.exe"));}catch{return false;}}
 void BuildCaptureSetup(StackPanel parent){var row=new StackPanel{Orientation=Orientation.Horizontal};parent.Children.Add(row);captureSetup=Btn(row,CaptureSetupCaption(),InstallCaptureTools);captureSetupLog=Btn(row,L.T("打开安装日志"),ShowCaptureSetupLog);captureSetupLog.Visibility=Visibility.Collapsed;captureSetupProgress=new ProgressBar{Height=4,IsIndeterminate=true,Visibility=Visibility.Collapsed,Margin=new Thickness(0,8,0,4)};parent.Children.Add(captureSetupProgress);captureSetupStatus=Hint(parent,CaptureToolsReady()?L.T("录制组件已配置，无需重复安装。可直接录制，或点击检查。"):L.T("首次录制需安装组件，完成后自动配置，无需填写路径。"));}
 async void InstallCaptureTools(){
  if(captureSetupBusy)return;
  if(recorder!=null){Notice(L.T("请先结束录制。"));return;}
  bool checkOnly=CaptureToolsReady() && !captureSetupNeedsRepair;
  if(!checkOnly && MessageBox.Show(window,L.T("将复用已有组件；缺少时从 Python 和微软官方网站下载约 52 MB。Python 仅供 Talaria 使用，不修改系统 PATH；微软蓝牙分析组件安装时可能需要管理员确认。完成后自动配置。继续？"),L.T("安装录制组件"),MessageBoxButton.OKCancel,MessageBoxImage.Information)!=MessageBoxResult.OK){CaptureSetupNote("USER cancelled-before-install");return;}
  captureSetupBusy=true;captureSetup.IsEnabled=false;captureSetup.Content=L.T("检测中…");captureSetupProgress.Visibility=Visibility.Visible;recordButton.IsEnabled=false;captureSetupStatus.Text=L.T("正在检测录制组件…");lock(captureSetupLogGate){captureSetupMemory.Clear();captureSetupLogError=null;}captureSetupLogPath=Path.Combine(AppPaths.Logs,"capture-setup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".log");captureSetupLog.Visibility=Visibility.Visible;
  try{
   int result=await Task.Run(delegate{
    CaptureSetupNote("START version="+ProductVersion+" mode="+(checkOnly?"check":"install"));
    string script=Path.Combine(Root,"Install-CaptureTools.ps1");if(!File.Exists(script))throw new FileNotFoundException();
    var start=new ProcessStartInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"System32","WindowsPowerShell","v1.0","powershell.exe"),"-NoProfile -ExecutionPolicy Bypass -File "+Quote(script)+(checkOnly?" -CheckOnly":"")){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
    start.EnvironmentVariables["PSModulePath"]=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Windows),"System32","WindowsPowerShell","v1.0","Modules")+";"+Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"WindowsPowerShell","Modules");
    using(var process=new Process{StartInfo=start}){
     process.OutputDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data==null)return;string line=e.Data;CaptureSetupNote(line);window.Dispatcher.BeginInvoke(new Action(delegate{if(line.StartsWith("STEP ")){string step=line.Substring(5);captureSetupStatus.Text=CaptureSetupStage(step);captureSetup.Content=L.T(step.EndsWith("download") && step!="verify-download"?"下载中…":step.Contains("install")?"安装中…":step.StartsWith("verify")?"校验中…":"检测中…");}}));};
     process.ErrorDataReceived+=delegate(object sender,DataReceivedEventArgs e){if(e.Data!=null)CaptureSetupNote("STDERR present (details omitted)");};
     process.Start();process.BeginOutputReadLine();process.BeginErrorReadLine();process.WaitForExit();return process.ExitCode;
    }
   });
   CaptureSetupNote("END exit="+result);captureSetupNeedsRepair=result!=0;
   captureSetupStatus.Text=checkOnly && result!=0?L.T("组件检测未通过，请点击“修复录制组件”。"):result==0 && CaptureToolsReady()?L.T("录制组件已就绪，可以开始录制。"):result==1223?L.T("已取消安装，可随时重试。"):L.T("录制组件安装未完成，请打开安装日志查看原因后重试。");
  }catch(Exception e){captureSetupNeedsRepair=true;CaptureSetupNote("FAILED type="+e.GetType().Name+" hresult="+e.HResult);captureSetupStatus.Text=L.T("录制组件安装未完成，请打开安装日志查看原因后重试。");}
  finally{captureSetupBusy=false;captureSetup.IsEnabled=true;captureSetup.Content=CaptureSetupCaption();captureSetupProgress.Visibility=Visibility.Collapsed;recordButton.IsEnabled=true;}
 }
 readonly object captureSetupLogGate=new object();readonly StringBuilder captureSetupMemory=new StringBuilder();string captureSetupLogError;
 void CaptureSetupNote(string value){lock(captureSetupLogGate){string line=DateTime.UtcNow.ToString("o")+" "+value+Environment.NewLine;captureSetupMemory.Append(line);try{Directory.CreateDirectory(AppPaths.Logs);if(captureSetupLogPath==null)captureSetupLogPath=Path.Combine(AppPaths.Logs,"capture-setup-"+DateTime.Now.ToString("yyyyMMdd-HHmmss-fff")+".log");File.AppendAllText(captureSetupLogPath,line);}catch(IOException e){captureSetupLogError="LOG_WRITE_FAILED hresult="+e.HResult;}catch(UnauthorizedAccessException e){captureSetupLogError="LOG_WRITE_FAILED hresult="+e.HResult;}}}
 string CaptureSetupLogText(){lock(captureSetupLogGate){if(captureSetupLogError!=null)return captureSetupLogError+Environment.NewLine+captureSetupMemory;try{if(captureSetupLogPath!=null && File.Exists(captureSetupLogPath))return File.ReadAllText(captureSetupLogPath);}catch(IOException){}catch(UnauthorizedAccessException){}return captureSetupMemory.Length>0?captureSetupMemory.ToString():L.T("尚无安装日志。点击安装后，这里会显示检测和安装结果。");}}
 string CaptureSetupCaption(){return L.T(captureSetupNeedsRepair?"修复录制组件":CaptureToolsReady()?"检查录制组件":"安装录制组件");}
 void ShowCaptureSetupLog(){ShowLogViewer(L.T("录制组件安装日志"),CaptureSetupLogText);}
 void ShowLogViewer(string title,Func<string> read){var panel=new DockPanel{Margin=new Thickness(20)};var view=new Window{Title=title,Owner=window,Width=800,Height=520,MinWidth=520,MinHeight=320,WindowStartupLocation=WindowStartupLocation.CenterOwner,Background=new SolidColorBrush(Color.FromRgb(24,36,48)),Foreground=Brushes.White,Content=panel};var buttons=new StackPanel{Orientation=Orientation.Horizontal,Margin=new Thickness(0,0,0,12)};DockPanel.SetDock(buttons,Dock.Top);panel.Children.Add(buttons);var text=new TextBox{Text=read(),IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,FontFamily=new FontFamily("Consolas"),FontSize=13,Padding=new Thickness(12),Background=new SolidColorBrush(Color.FromRgb(15,26,36)),Foreground=Brushes.White};Btn(buttons,L.T("刷新"),delegate{text.Text=read();text.ScrollToEnd();});Btn(buttons,L.T("复制日志"),delegate{Guard(delegate{Clipboard.SetText(text.Text);});});panel.Children.Add(text);view.ShowDialog();}
 string ReadLogText(string file){try{if(!File.Exists(file))return L.T("日志文件不存在。");using(var stream=new FileStream(file,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete)){bool tail=stream.Length>2*1024*1024;if(tail)stream.Seek(-2*1024*1024,SeekOrigin.End);using(var reader=new StreamReader(stream)){return (tail?L.T("日志较大，仅显示末尾 2 MB。")+Environment.NewLine:"")+reader.ReadToEnd();}}}catch(Exception e){return L.T("无法读取日志：")+e.GetType().Name+" ("+e.HResult+")";}}
 string CaptureSetupStage(string step){switch(step){case "detect":return L.T("正在检测录制组件…");case "verify-download":return L.T("正在校验下载文件…");case "python-download":return L.T("正在下载 Python 录制运行环境…");case "python-install":return L.T("正在配置专用 Python…");case "microsoft-download":return L.T("正在下载微软蓝牙分析组件…");case "microsoft-install":return L.T("正在安装微软组件，请留意管理员确认…");default:return L.T("正在验证并保存录制组件设置…");}}
}
