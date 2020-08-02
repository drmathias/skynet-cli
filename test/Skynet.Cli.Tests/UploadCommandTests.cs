using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;
using Sia.Skynet;
using Skynet.Cli;
using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Hosting;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Nark.Cli.Tests.Upload
{
    public class UploadCommandTests
    {
        private Mock<ISkynetWebPortal> _skynetWebPortal;

        private UploadResponse UploadResponseStub => new UploadResponse { Skylink = "EAA1fG_ip4C1Vi1Ijvsr1oyr8jpH0Bo9HXya0T3kw-elGw" };

        [SetUp]
        public void SetUp()
        {
            _skynetWebPortal = new Mock<ISkynetWebPortal>();
            _skynetWebPortal
                .Setup(callTo => callTo.UploadFiles(It.IsAny<string>(), It.IsAny<IEnumerable<UploadItem>>())).ReturnsAsync(UploadResponseStub)
                .Verifiable();
        }

        [Test]
        public async Task Upload_File_SingleFile()
        {
            // Arrange
            var parser = SetUpCommandLine(_skynetWebPortal.Object).Build();

            // Act
            var result = await parser.InvokeAsync("upload samples/empty.html");

            // Assert
            _skynetWebPortal
                .Verify(callTo => callTo.UploadFiles(It.IsAny<string>(), It.Is<IEnumerable<UploadItem>>(
                    items => items.Count() == 1 && items.FirstOrDefault().FileInfo.Name == "empty.html")), Times.Once);
        }

        [Test]
        public async Task Upload_Directory_NotRecursive()
        {
            // Arrange
            var parser = SetUpCommandLine(_skynetWebPortal.Object).Build();

            // Act
            var result = await parser.InvokeAsync("upload samples");

            // Assert
            _skynetWebPortal
                .Verify(callTo => callTo.UploadFiles(It.IsAny<string>(), It.Is<IEnumerable<UploadItem>>(
                    items => items.Count() == GetFileCount(new DirectoryInfo("samples"), false))), Times.Once);
        }

        [Test]
        public async Task Upload_Directory_Recursive()
        {
            // Arrange
            var parser = SetUpCommandLine(_skynetWebPortal.Object).Build();

            // Act
            var result = await parser.InvokeAsync("upload samples -r");

            // Assert
            _skynetWebPortal
                .Verify(callTo => callTo.UploadFiles(It.IsAny<string>(), It.Is<IEnumerable<UploadItem>>(
                    items => items.Count() == GetFileCount(new DirectoryInfo("samples"), true))), Times.Once);
        }

        private int GetFileCount(DirectoryInfo rootDirectory, bool recurse)
        {
            var count = 0;

            var children = rootDirectory.GetFileSystemInfos();
            foreach (var item in children)
            {
                if (item is FileInfo file) count++;
                if (item is DirectoryInfo directory && recurse) count += GetFileCount(directory, recurse);
            }

            return count;
        }

        private CommandLineBuilder SetUpCommandLine(ISkynetWebPortal skynetWebPortal)
        {
            return new CommandLineBuilder()
                   .UseHost(CreateHostBuilder, host =>
                   {
                       host.ConfigureServices(services =>
                       {
                           services.AddTransient(mock => skynetWebPortal);
                       });
                   })
                   .AddCommand(new UploadCommand());
        }

        private IHostBuilder CreateHostBuilder(string[] args)
        {
            return new HostBuilder();
        }
    }
}
