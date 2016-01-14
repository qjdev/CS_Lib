using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace NugetPushTool
{
    using System.Threading;

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var process = new Process();
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardInput = true;
                process.StartInfo.CreateNoWindow = false;
                process.StartInfo.FileName = "CMD.EXE";
                process.Start();

                //在同一目录查找Nuget程序
                var currentDir = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
                var nugetExe = currentDir.GetFiles("NuGet.exe");
                if (!nugetExe.Any())
                {
                    Console.WriteLine("没有找到NuGet.exe程序！");
                    Console.ReadKey();
                    return;
                }
                else
                {
                    //设置Key
                    process.StandardInput.WriteLine("nuget setApiKey {0}", ConfigurationManager.AppSettings["NugetApiKey"]);
                }

                //查找Src目录
                //向上两层
                var nugetDir = currentDir.Parent;
                if (nugetDir == null || !nugetDir.Exists)
                {
                    Console.WriteLine("没有找到NuGet目录！");
                    Console.ReadKey();
                    return;
                }

                var rootDir = nugetDir.Parent;
                if (rootDir == null || !rootDir.Exists)
                {
                    Console.WriteLine("没有找到Src的上层目录！");
                    Console.ReadKey();
                    return;
                }

                var srcDir = rootDir.GetDirectories("Src");
                if (!srcDir[0].Exists)
                {
                    Console.WriteLine("没有找到Src的目录！");
                    Console.ReadKey();
                    return;
                }

                //调用一次Nuget打包脚本
                var nugetPackBat = srcDir[0].GetFiles("Nuget_Pack_Release.bat");
                if (!nugetPackBat.Any())
                {
                    Console.WriteLine("Src目录下没有找到Nuget_Pack_Release.bat！");
                    Console.ReadKey();
                    return;
                }                

                //切换目录
                process.StandardInput.WriteLine("CD {0}", srcDir[0].FullName);

                //调用打包脚本
                Console.WriteLine("开始打包");
                process.StandardInput.WriteLine(nugetPackBat[0].FullName);

                //切换回工具目录
                process.StandardInput.WriteLine("CD {0}", currentDir.FullName);

                Console.WriteLine("打包完毕，准备推送");
                //收集Src下的待推送文件
                //规则为目录下有两个nupkg文件，删除版本号小的那个，推送大的那个
                var nupkgList = srcDir[0].GetFiles("*.nupkg", SearchOption.AllDirectories);

                foreach (var fileInfo in nupkgList)
                {
                    //排除第三方库的pack
                    if (!fileInfo.Directory.GetFiles("*.csproj", SearchOption.TopDirectoryOnly).Any())
                    {
                        continue;
                    }

                    List<FileInfo> toDelelteList;
                    //执行一次删除多余的操作，如果自身还存在再执行推送
                    var newFileInfo = DeleteMoreNupkgFile(fileInfo, out toDelelteList);

                    //执行过删除旧dll的操作才推送
                    if (newFileInfo != null && newFileInfo.Exists)
                    {
                        Console.WriteLine("准备推送文件{0}", newFileInfo.FullName);
                        process.StandardInput.WriteLine("nuget push {0}", newFileInfo.FullName);
                        if (toDelelteList != null && toDelelteList.Any())
                        {
                            foreach (var info in toDelelteList)
                            {
                                Console.WriteLine("发现低版本号文件{0}，执行删除", info.FullName);
                                File.Delete(info.FullName);
                            }
                        }
                    }
                }

                Console.ReadKey();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }

        private static FileInfo DeleteMoreNupkgFile(FileInfo fileInfo, out List<FileInfo> toDelelteList)
        {
            FileInfo result = null;
            toDelelteList = new List<FileInfo>();

            if (fileInfo != null && fileInfo.Exists)
            {
                var currentDirNupkgFiles = fileInfo.Directory.GetFiles("*.nupkg", SearchOption.TopDirectoryOnly);
                if (currentDirNupkgFiles.Any())
                {
                    //暂时不考虑按文件前缀分组
                    var nupkgFiles = currentDirNupkgFiles.OrderByDescending<FileInfo, string>(
                        (s) => s.FullName, new FileVersionCompare()).ToList();

                    for (int i = 1; i < nupkgFiles.Count(); i++)
                    {
                        toDelelteList.Add(nupkgFiles[i]);
                    }

                    result = nupkgFiles[0];
                }
            }

            return result;
        }        
    }
}
