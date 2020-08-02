using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Physical;
using Microsoft.Extensions.Hosting;
using Sia.Skynet;
using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;

namespace Skynet.Cli
{
    public class UploadCommand : Command
    {
        public UploadCommand() : base(name: "upload", description: "Uploads an item to Sia Skynet")
        {
            var itemArgument = new Argument<FileSystemInfo>("item", "File or directory to upload")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            var recurseOption = new Option(new string[] { "-r", "--recurse" }, "If you want to recurse the directory")
            {
                IsRequired = false
            };

            AddArgument(itemArgument.ExistingOnly());
            AddOption(recurseOption);
            Handler = CommandHandler.Create<IHost, FileSystemInfo, bool>(UploadOneTime);
        }

        public async Task UploadOneTime(IHost host, FileSystemInfo item, bool recurse)
        {
            await Console.Out.WriteLineAsync($"Attempting to upload {item.Name}...");

            UploadResponse uploadResponse;
            var skynetWebPortal = host.Services.GetRequiredService<ISkynetWebPortal>();

            try
            {
                uploadResponse = item switch
                {
                    FileInfo file => await skynetWebPortal.UploadFile(
                        new UploadItem(new PhysicalFileInfo(file))),
                    DirectoryInfo directory => await skynetWebPortal.UploadDirectory(
                        new PhysicalFileProvider(directory.FullName.Remove(directory.FullName.LastIndexOf(directory.Name))), directory.Name, recurse),
                    _ => throw new ArgumentException("Unrecognised file system item", nameof(item))
                };

                await Console.Out.WriteLineAsync($"Successfully uploaded {item.Name} to Skynet");
                await Console.Out.WriteLineAsync($"https://siasky.net/{uploadResponse.Skylink}");
            }
            catch (Exception exception)
            {
                await Console.Error.WriteLineAsync(exception.Message);
            }
        }
    }
}
