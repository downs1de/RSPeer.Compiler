using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using RSPeer.Services.Base;

namespace RSPeer.Services
{
    public class GitToJarService : IGitToJarService
    {
        private readonly IRspeerApiService _rspeerApi;
        private readonly IConfiguration _configuration;

        public GitToJarService(IRspeerApiService rspeerApi, IConfiguration configuration)
        {
            _rspeerApi = rspeerApi;
            _configuration = configuration;
        }

        public async Task<byte[]> BuildJarFromGit(string repository)
        {
            var path = CloneRepo(repository);
            try
            {
                var buildFolder = Path.Combine(path, "build");
                if (Directory.Exists(buildFolder))
                {
                    Directory.Delete(buildFolder, true);
                }

                var sources = string.Empty;
                Directory.CreateDirectory(buildFolder);
                bool hasKotlin = false;
                if (Directory.GetFiles(path, "*.kt", SearchOption.AllDirectories).Length > 0)
                {
                    hasKotlin = true;
                    Console.WriteLine("Compiling Kotlin........");
                    sources = await CompileSources(path, buildFolder, true);
                }
                if (Directory.GetFiles(path, "*.java", SearchOption.AllDirectories).Length > 0)
                {
                    if (hasKotlin)
                    {
                        throw new Exception("Mixing Kotlin and Java is unsupported by the obfuscator at this time, please submit a script either with all Java or all Kotlin.");
                    }
                    Console.WriteLine("Compiling Java........");
                    sources = await CompileSources(path, buildFolder, false);
                }
                var bytes = await BuildJar(sources);
                return bytes;
            }
            finally
            {
                var info = new DirectoryInfo(path);
                SetAttributesNormal(info);
                info.Delete(true);
            }
        }

        private string CloneRepo(string repoUrl)
        {
            var credentials = new UsernamePasswordCredentials
            {
                Username = _configuration.GetValue<string>("Gitlab:Username"),
                Password = _configuration.GetValue<string>("Gitlab:Password")
            };

            var folderName = repoUrl.Replace("http://", "").Replace("https://", "").Replace(" ", "").Remove(0, 1)
                .Replace("gitlab.com", "");
            var path = Path.Combine(Directory.GetCurrentDirectory(), "gitToJarTemp", folderName);

            if (Directory.Exists(path))
            {
                var directory = new DirectoryInfo(path);
                SetAttributesNormal(directory);
                directory.Delete(true);
            }

            var cloneOptions = new CloneOptions
            {
                BranchName = "master",
                Checkout = true,
                CredentialsProvider = (url, user, cred) => credentials,
                RecurseSubmodules = true
            };

            Repository.Clone(repoUrl, path, cloneOptions);
            return path;
        }

        private async Task<string> GetBotJarPath()
        {
            var version = await _rspeerApi.GetBotVersion();
            var dependenciesFolder = Path.Combine(Directory.GetCurrentDirectory(), "gitToJarTemp", "botDependencies");
            if (!Directory.Exists(dependenciesFolder)) Directory.CreateDirectory(dependenciesFolder);
            var files = Directory.GetFiles(dependenciesFolder);
            //Delete older versions of the bot.
            foreach (var s in files.Where(w => w.StartsWith("rspeer-"))) File.Delete(s);
            var path = Path.Combine(dependenciesFolder, $"rspeer-{version}.jar");
            if (File.Exists(path))
                return path;
            var bytes = await _rspeerApi.GetCurrentJar();
            await File.WriteAllBytesAsync(path, bytes);
            return path;
        }

        private string AggregateSoucesToFile(string path, bool kotlin)
        {
            var extensions = kotlin ? new[] {".java", ".kt"} : new[] {".java"};
            
            Console.WriteLine("Reading files with extensions: ");
            
            foreach (string extension in extensions)
            {
                Console.Write("," + extension);
            }

            Console.WriteLine();

            var file = Path.Combine(path, $"{(kotlin ? "kt" : "java")}-sources-{Guid.NewGuid()}.txt");
            if (File.Exists(file))
                File.Delete(file);
            var files = Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                .Where(f => extensions.Contains(Path.GetExtension(f).ToLower()));
            File.WriteAllLines(file, files);
            return file;
        }

        private async Task<string> CompileSources(string path, string buildFolder, bool kotlin)
        {
            var fileWithSources = AggregateSoucesToFile(path, kotlin);

            var info = new ProcessStartInfo
            {
                ArgumentList =
                {
                    "-Xlint:unchecked",
                    "-nowarn",
                    "-cp",
                    $"{await GetBotJarPath()}:{buildFolder}",
                    "-d",
                    $"{buildFolder}",
                    $"@{fileWithSources}"
                },
                FileName = kotlin ? "/root/.sdkman/candidates/kotlin/1.3.31/bin/kotlinc" : "javac",                UseShellExecute = false,
                WorkingDirectory = path,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            
            var process = Process.Start(info);
            if (process == null) throw new Exception("Failed to start process.");

            process.WaitForExit();
            string errors = null;

            while (!process.StandardError.EndOfStream)
            {
                if (errors != null && errors.Length > 1000)
                    break;
                var line = await process.StandardError.ReadLineAsync();
                Console.WriteLine(line);
                errors += line;
            }

            if (errors != null && errors.Contains("error:"))
            {
                Console.WriteLine("Error compiling: ");
                throw new Exception(errors);
            }

            var current = DateTime.UtcNow;

            while (!process.HasExited)
            {
                if (current.AddSeconds(60) < DateTime.UtcNow)
                {
                    throw new Exception("Timed out after compiling script.");
                }

                Thread.Sleep(1000);
                Console.WriteLine("Waiting for exit.");
            }

            return buildFolder;
        }

        private async Task<byte[]> BuildJar(string path)
        {
            Thread.Sleep(1000);
            var process = Process.Start(new ProcessStartInfo
            {
                ArgumentList =
                {
                    "cvf",
                    "compiled-script.jar",
                    "."
                },
                FileName = "jar",
                UseShellExecute = false,
                WorkingDirectory = path,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            });
            process.WaitForExit(60);
            string logs = null;
            string errors = null;

            while (!process.StandardOutput.EndOfStream)
            {
                logs += await process.StandardOutput.ReadLineAsync();
                logs += Environment.NewLine;
            }

            while (!process.StandardError.EndOfStream)
            {
                errors += await process.StandardError.ReadLineAsync();
                errors += Environment.NewLine;
            }

            var bytes = File.ReadAllBytes(Path.Combine(path, "compiled-script.jar"));

            if (bytes.Length == 0)
            {
                throw new Exception(logs + "@@" + errors);
            }

            return bytes;
        }

        private void SetAttributesNormal(DirectoryInfo dir)
        {
            foreach (var subDir in dir.GetDirectories())
            {
                SetAttributesNormal(subDir);
            }

            foreach (var file in dir.GetFiles())
            {
                file.Attributes = FileAttributes.Normal;
            }
        }
    }
}
